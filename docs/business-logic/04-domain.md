# 04. 領域模型與狀態機

## 4.1 核心實體關聯（簡化）

```
Clinic ──┬── User ── UserRole ── Role
         ├── Department ── Room
         ├── Patient ── Appointment
         │               └── Visit ── VisitEvent
         │                    ├── QueueEntry
         │                    └── Prescription ── PrescriptionItem ── Medication
         ├── WorkflowDefinition ── WorkflowStep ── WorkflowTransition
         └── QueueConfig / NotificationTemplate / AuditLog
```

**多租戶規則**：除 `Role` 外，所有實體皆含 `ClinicId`，Service 查詢必須帶 `clinicId` 作為過濾。

---

## 4.2 Visit 狀態機（`VisitStatus`）

```
[建立] → Active ──┬──→ Completed  （看診 + 領藥完成）
                  ├──→ Cancelled  （Admin 取消）
                  └──→ NoShow     （超時未到）
```

同時 Visit 透過 `CurrentStepId` 指向 `WorkflowStep`，由 Workflow Engine 推進。

---

## 4.3 QueueEntry 狀態機（`QueueEntryStatus`）

```
Waiting ──→ Called ──→ InProgress ──→ Completed
   │           │
   │           └──→ NoShow
   └──→ Skipped
```

**排序規則**：`ORDER BY Priority DESC, CreatedAt ASC`（同優先度先到先看）。

---

## 4.4 Prescription 狀態機（`PrescriptionStatus`）

```
Draft ──→ Sent ──→ Dispensing ──→ Dispensed ──→ PickedUp
```

- `Draft`：醫師開立中（尚未結束看診）
- `Sent`：結束看診後自動送藥局，進入藥局佇列
- `Dispensing`：藥師開始配藥
- `Dispensed`：配藥完成，等待病患領取（推播通知）
- `PickedUp`：病患取藥，Visit 完成

---

## 4.5 Workflow Engine（有向圖）

**核心概念**：流程不寫死在程式碼，而是資料驅動。

```
WorkflowDefinition (1) ─┬─→ (N) WorkflowStep
                        └─→ (N) WorkflowTransition
                                  ├─ FromStepId
                                  ├─ ToStepId
                                  ├─ ConditionJson  ← 條件表達式
                                  └─ Priority       ← 多條件時的評估優先度
```

### 推進邏輯（`IWorkflowEngine.AdvanceAsync`）

1. 取得 Visit 的 `CurrentStepId`
2. 查詢所有 `OutgoingTransitions`（按 Priority 排序）
3. 逐一評估 `ConditionJson`（依 Visit 屬性如 `NeedsMedication`）
4. 第一個符合的 Transition 勝出 → 更新 `Visit.CurrentStepId`
5. 寫入 `VisitEvent`（記錄 `FromStep` / `ToStep` / `TriggerType` / `TriggeredByUserId`）
6. 若新步驟 `AutoAdvance=true` → 遞迴推進

### TriggerType
`Manual` / `Auto` / `System` / `Timeout` — 稽核用。

---

## 4.6 Enum 快速參照

| Enum | 值 |
|------|---|
| `VisitStatus` | Active, Completed, Cancelled, NoShow |
| `QueueEntryStatus` | Waiting, Called, InProgress, Completed, Skipped, NoShow |
| `QueueType` | Consulting, Pharmacy |
| `PrescriptionStatus` | Draft, Sent, Dispensing, Dispensed, PickedUp |
| `CheckinMethod` | Otp, QrCode, Manual |
| `AppointmentStatus` | Booked, CheckedIn, Cancelled, NoShow |
| `CommandAction` | CallNext, Skip, QueryQueue, CompleteConsult, CreatePrescription, QueryStats, Unknown |
| `TriggerType` | Manual, Auto, System, Timeout |
| `NotificationChannel` | SignalR, WebPush, Both |
| `NotificationStatus` | Pending, Sent, Delivered, Failed |
| `RoomType` | Consulting, Pharmacy, Treatment |
