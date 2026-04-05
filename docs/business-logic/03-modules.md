# 03. 功能模組清單

## 3.1 已實作核心模組

| 模組 | 核心職責 | 主要 Service | API Controller |
|------|---------|-------------|---------------|
| **CheckIn 報到** | OTP / QR / 手動三種報到、驗證、建立 Visit | `ICheckInService` | `CheckInController` |
| **Queue 候診佇列** | 佇列查詢、位置查詢、叫號、過號 | `IQueueService` | `QueueController` |
| **Visit 看診流程** | 看診狀態查詢、事件歷史、開始 / 完成看診、推進步驟 | `IVisitService` | `DoctorController` |
| **Prescription 處方** | 開立處方、藥局佇列、配藥、發藥 | `IPrescriptionService` | `DoctorController` + `PharmacyController` |
| **Workflow Engine** | 有向圖流程引擎，推進步驟並評估跳轉條件 | `IWorkflowEngine` | （內部呼叫）|
| **AI Triage 智慧分流** | LLM 依症狀建議科別 + 優先度 + 預估等候 | `IAiService.TriageAsync` | `AiController` |
| **AI Command 自然語言指令** | LLM 解析自然語言 → Action → Executor 執行 | `IAiService.CommandAsync` + `CommandRouter` | `AiController` |
| **Admin 管理後台** | 工作流 CRUD、診所設定 | （DTOs 已定義） | `AdminController` |
| **Real-time 即時推播** | 病患 / 診所群組推播叫號與狀態 | SignalR `VisitHub` | `/hubs/visit` |

---

## 3.2 領域支援實體

以下實體無獨立 Service，作為核心模組的支援資料：

| 實體 | 用途 |
|------|------|
| `Clinic` | 租戶根；含 `SettingsJson` 存客製設定 |
| `Department` / `Room` | 科別、診間（用於排程與分流） |
| `User` / `Role` / `UserRole` | 員工帳號與角色綁定 |
| `Schedule` | 醫師排班（含 `DayOfWeekFlag` + `TimeSlot`）|
| `Patient` / `PatientDevice` | 病患與其綁定的通知裝置 |
| `Appointment` | 預約（含 `QrCodeToken`）|
| `Medication` / `PrescriptionItem` | 藥品與處方明細 |
| `OtpVerification` | OTP 驗證碼（含 `ExpiresAt` / `IsUsed`）|
| `NotificationLog` / `NotificationTemplate` | 通知紀錄與模板 |
| `VisitEvent` | Visit 狀態變更稽核事件流 |
| `AuditLog` | 全系統稽核記錄 |
| `QueueConfig` | 佇列類型設定（Consulting / Pharmacy + PriorityWeight）|

---

## 3.3 API Endpoint 速查

| Controller | 路徑 | 方法 |
|-----------|------|------|
| `CheckInController` | `/api/checkin/otp/send` | POST |
| | `/api/checkin/otp/verify` | POST |
| | `/api/checkin/qrcode` | POST |
| | `/api/checkin/manual` | POST |
| `QueueController` | `/api/queue/{clinicId}` | GET |
| | `/api/queue/{clinicId}/position/{visitId}` | GET |
| | `/api/queue/call-next` | POST |
| | `/api/queue/call-pickup/{visitId}` | POST |
| | `/api/queue/{visitId}/skip` | POST |
| `DoctorController` | `/api/doctor/queue` | GET |
| | `/api/doctor/visits/{visitId}/start-consult` | POST |
| | `/api/doctor/visits/{visitId}/prescriptions` | POST |
| | `/api/doctor/visits/{visitId}/complete-consult` | POST |
| `PharmacyController` | `/api/pharmacy/queue` | GET |
| | `/api/pharmacy/prescriptions/{id}/start-dispense` | POST |
| | `/api/pharmacy/prescriptions/{id}/complete-dispense` | POST |
| `AiController` | `/api/ai/triage` | POST |
| | `/api/ai/command` | POST |
| | `/api/ai/command/execute` | POST |
| `AdminController` | `/api/admin/workflows` | GET / POST |
| | `/api/admin/clinic/settings` | GET / PUT |

---

## 3.4 前端介面（Vue 3）

| 角色 | 介面 | 路徑 |
|------|------|------|
| Patient | 報到（OTP / QR）、候診位置 | `views/patient/CheckIn.vue` / `Queue.vue` |
| Nurse | 候診看板、手動報到、過號、自然語言指令 | `views/nurse/Dashboard.vue` |
| Doctor | 我的候診、開始 / 結束看診、開處方 | `views/doctor/Consult.vue` |
| Pharmacy | 藥局佇列、配藥、發藥 | `views/pharmacy/Queue.vue` |
| Admin | 營運儀表板、工作流設定 | `views/admin/Dashboard.vue` |

前端狀態管理：Pinia `stores/visit.ts`。
API 客戶端：`api/client.ts`。
