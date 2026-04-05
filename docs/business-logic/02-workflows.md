# 02. 業務流程

## 2.1 標準看診流程（預設 Workflow）

```
報到 → 等候看診 → 叫號 → 看診中 → 完成看診
                                  ├─ 需要用藥 → 送藥局 → 配藥中 → 等待領藥 → 領取完成
                                  └─ 不需用藥 → 結束
```

### 每一步驟觸發者與動作

| # | 步驟 (StepCode) | 觸發者 | 關鍵動作 | 後續狀態 |
|---|----------------|--------|---------|---------|
| 1 | 報到 | Patient / Nurse | OTP / QR / Manual 三擇一；建立 Visit + QueueEntry | `Waiting` |
| 2 | 等候看診 | — | SignalR 推播位置；隊列依 Priority + CheckedInAt 排序 | `Waiting` |
| 3 | 叫號 | Doctor / Nurse | `CallNextAsync` 取出佇列第一位，推播叫號通知 | `Called` |
| 4 | 看診中 | Doctor | `StartConsultAsync` | `InProgress` |
| 5 | 完成看診 | Doctor | `CompleteConsultAsync(needsMedication)`；若開處方則同時送藥局 | 依分支 |
| 6a | 送藥局 | 系統自動 | Prescription 狀態 `Draft` → `Sent`；加入藥局佇列 | `Waiting`（藥局） |
| 6b | 配藥中 | Pharmacist | `StartDispenseAsync` | `Dispensing` |
| 6c | 等待領藥 | Pharmacist | `CompleteDispenseAsync` + 推播叫號 | `Dispensed` |
| 6d | 領取完成 | Pharmacist | 病患到櫃台確認；狀態 `PickedUp`；Visit `Completed` | — |
| 7 | 不需用藥結束 | 系統 | Visit 直接 `Completed` | — |

---

## 2.2 異常分支

| 分支 | 觸發 | 結果 |
|------|------|------|
| **過號（Skip）** | Nurse/Doctor/Pharmacist 主動 Skip | QueueEntry 狀態 `Skipped`，不再自動叫號（需手動重排）|
| **No-Show** | 超時未到或多次過號 | Visit / Appointment 標記 `NoShow` |
| **取消** | Admin 手動取消 | Visit `Cancelled` |
| **條件式跳轉** | WorkflowTransition `ConditionJson` 評估 | 依 Visit 屬性（如 `NeedsMedication`）決定下一步 |

條件式跳轉詳見 [04-domain.md](./04-domain.md) §Workflow Engine。

---

## 2.3 報到三種方式比較

| 方式 | 觸發者 | 前置條件 | 病患資料來源 | 典型情境 |
|------|--------|---------|-------------|---------|
| **OTP** | Patient | 有手機號 + 已建檔 Patient | 既有 Patient | 回診病患自助報到 |
| **QR Code** | Patient | 預約時取得 `QrCodeToken` | Appointment → Patient | 預約後到院掃碼 |
| **Manual** | Nurse | — | 現場輸入（可匿名） | 初診 / 長輩 / 無手機 |

### 報到後共通行為

1. 建立 `Visit`（含 `CheckinMethod` 紀錄來源）
2. 建立 `QueueEntry(QueueType=Consulting)` 加入候診佇列
3. 分配 `QueueNumber`（per-clinic per-day 遞增）
4. 回傳 `CheckInResponse(VisitId, QueueNumber, CurrentStep)`
5. 推播 SignalR 給診所群組（新病患到達）
