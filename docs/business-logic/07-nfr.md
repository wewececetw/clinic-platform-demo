# 07. 非功能性需求（NFR）

## 7.1 效能

- 佇列查詢 < 100ms（Redis）
- AI 分流 / 指令回應 < 3 秒（oMLX 本地非思考模型）
- SignalR 推播延遲 < 500ms

---

## 7.2 可靠性

- **LLM Provider Fallback**：oMLX 失敗 → Groq → 手動模式
- **Queue 雙寫**：Redis + MySQL，Redis 故障時降級為 MySQL 讀取
- **事件溯源**：VisitEvent 保留完整事件流，可事後重建任何 Visit 狀態

---

## 7.3 安全

- OTP 5 分鐘過期 + 一次性
- QR Token 一次性
- 所有操作寫入 `AuditLog`（含 IP / Old / New）
- Admin 操作二次確認（UI 層）

---

## 7.4 可維運

- 三軌稽核：`VisitEvent` + `AuditLog` + `NotificationLog`
- Clinic `SettingsJson` 允許動態調整設定，免發版
- Workflow Engine 資料驅動，新流程不需改 code

---

## 7.5 擴充性

| 擴充項目 | 機制 |
|---------|------|
| 多租戶 | 資料層 `ClinicId` 強制隔離 ready |
| LLM provider | 實作 `ILlmClient` 即可新增 |
| Workflow 步驟 / 跳轉 | 資料驅動，無需改 code |
| 自然語言指令 | 實作 `ICommandExecutor` 介面即可新增 |
