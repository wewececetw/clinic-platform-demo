# 05. 業務規則彙總

## 5.1 報到（CheckIn）

- **BR-CI-01**：OTP 有效期限 5 分鐘（`OtpVerification.ExpiresAt`）；驗證成功後 `IsUsed=true`，不可重複使用。
- **BR-CI-02**：QR Code Token 為一次性，掃碼後綁定該 `Appointment`。
- **BR-CI-03**：手動報到可建立匿名 Patient（`IsAnonymous=true`），之後可補資料。
- **BR-CI-04**：診所可透過 `ClinicSettingsDto.AllowedCheckinMethods` 限定允許的報到方式。
- **BR-CI-05**：報到成功 → 建立 `Visit`（含 `CheckinMethod`）+ `QueueEntry(QueueType=Consulting)`。
- **BR-CI-06**：`QueueNumber` per-clinic per-day 遞增（每天從 1 重新編號）。

---

## 5.2 候診佇列（Queue）

- **BR-Q-01**：佇列排序 = `Priority DESC, CreatedAt ASC`。
- **BR-Q-02**：AI Triage 建議的 Priority 會寫入 `QueueEntry.Priority`，實現「急診優先」。
- **BR-Q-03**：`CallNextAsync` 為原子操作（取第一位 + 改狀態 + 推播）。
- **BR-Q-04**：Skipped 的項目不會自動重新叫號，需人工介入。
- **BR-Q-05**：同一 Visit 同時最多有一筆 Consulting QueueEntry + 一筆 Pharmacy QueueEntry。
- **BR-Q-06**：Queue 以 Redis 為主（查詢效能）+ MySQL 為持久化備份。

---

## 5.3 看診（Visit）

- **BR-V-01**：只有 `QueueEntry.Status=Called` 的 Visit 才可 `StartConsult`。
- **BR-V-02**：`CompleteConsult` 必須帶 `NeedsMedication` 參數，決定是否進入藥局流程。
- **BR-V-03**：`NeedsMedication=true` 但醫師未開處方 → 結束看診時報錯。
- **BR-V-04**：每次步驟變更都必須寫入 `VisitEvent`（稽核必備）。
- **BR-V-05**：Visit 僅能被其主治 Doctor 或 Admin 操作完成 / 取消。

---

## 5.4 處方（Prescription）

- **BR-P-01**：一個 Visit 最多一張 Prescription。
- **BR-P-02**：處方 Items 不可為空。
- **BR-P-03**：`CompleteConsult` 時若 `NeedsMedication=true`，Prescription 自動 `Draft → Sent`。
- **BR-P-04**：配藥流程為單向：`Sent → Dispensing → Dispensed → PickedUp`，不可倒退。
- **BR-P-05**：只有狀態 `Dispensing` 的處方可 `CompleteDispense`。

---

## 5.5 工作流（Workflow）

- **BR-WF-01**：每家診所至少有一個 `IsDefault=true` 的 WorkflowDefinition。
- **BR-WF-02**：新 Visit 未指定 workflow 時使用診所 Default。
- **BR-WF-03**：`WorkflowStep.IsSkippable=false` 的步驟不可跳過。
- **BR-WF-04**：`AutoAdvance=true` 的步驟在進入後立即評估 transitions 並推進。
- **BR-WF-05**：Transition 條件評估依 Priority 由高至低，第一個符合者勝出。
- **BR-WF-06**：修改 workflow 不影響進行中的 Visit（Visit 綁定建立時的 WorkflowDefinitionId）。

---

## 5.6 AI 指令（Command）

- **BR-AI-01**：Command Executor 以 `Action` 名稱為 key 註冊，找不到則拒絕執行。
- **BR-AI-02**：每個 Executor 宣告 `AllowedRoles`，Router 先驗角色才執行（雙層權限）。
- **BR-AI-03**：LLM 解析失敗 → 回 `Action=Unknown`，前端顯示「無法理解指令」。
- **BR-AI-04**：指令執行拋出例外 → Router 捕獲並回「指令執行失敗」（不洩漏內部錯誤）。
- **BR-AI-05**：危險指令（如刪除、取消）需由前端二次確認（UI 層規範）。

---

## 5.7 多租戶（Multi-tenant）

- **BR-MT-01**：所有 Service 方法必帶 `clinicId` 作為查詢條件。
- **BR-MT-02**：跨租戶資料存取視為越權，Service 應直接返回 NotFound / Forbidden。
- **BR-MT-03**：SignalR 推播以 `clinic_{clinicId}` 為群組隔離。

---

## 5.8 即時通知（SignalR）

- **BR-RT-01**：病患訂閱 `visit_{visitId}` 群組接收個人更新。
- **BR-RT-02**：診所員工訂閱 `clinic_{clinicId}` 群組接收全院訊息。
- **BR-RT-03**：叫號 / 狀態變更 / 配藥完成皆觸發推播。
