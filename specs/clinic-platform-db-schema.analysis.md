# Spec 完整性分析

## 遺漏的實體
- [ ] **Clinic（診所）表**：Multi-tenant 核心，spec 僅提 clinic_id 但未定義診所本身的屬性
- [ ] **User / StaffMember 表**：Nurse/Doctor/Pharmacist/Admin 需要統一帳號表，spec 未提認證機制
- [ ] **Role / Permission 表**：RBAC 權限控制，spec 列了角色但沒提權限粒度
- [ ] **Appointment（預約）表**：QR Code 報到基於預約單，但 spec 未提預約功能
- [ ] **Prescription（處方）表**：流程有「開立處方」但未定義處方結構
- [ ] **PrescriptionItem（處方明細）表**：處方與藥品多對多關係
- [ ] **Medication（藥品）表**：藥劑師配藥需要藥品主檔
- [ ] **Room / Counter（診間/窗口）表**：叫號需指定目的地
- [ ] **NotificationLog（通知紀錄）表**：spec 提即時通知但沒提通知紀錄
- [ ] **AuditLog（操作紀錄）表**：醫療系統合規需求
- [ ] **OtpVerification（OTP 驗證）表**：手機 OTP 報到的暫存機制
- [ ] **WebPushSubscription 表**：Web Push 需儲存 subscription endpoint

## 模糊需求（需假設）
- [ ] **Workflow 跳步邏輯**：「免藥直接離院」條件如何定義？由醫師標註？自動判斷？
  - → 假設：醫師在看診時標註「無需領藥」，workflow engine 據此跳步
- [ ] **Identity Resolution 觸發時機**：同手機號何時合併？報到時即時？還是批次？
  - → 假設：報到時即時比對，找到既有病患則關聯，否則新建
- [ ] **匿名報到後合併**：合併時如何處理衝突資料？
  - → 假設：以手機號為主鍵，後續補填時自動關聯到既有 Patient
- [ ] **叫號優先度**：優先號的優先層級有幾種？如何設定？
  - → 假設：用 priority 數字欄位，管理員可設定優先類別與對應權重
- [ ] **多醫師排班**：同一診所多位醫師如何分流？
  - → 假設：依診間（Room）分流，每個 Visit 綁定特定醫師+診間
- [ ] **處方格式**：處方是自由文字還是結構化（藥品+劑量+頻次）？
  - → 假設：結構化，PrescriptionItem 含藥品ID、劑量、頻次、天數

## 邊界案例
- [ ] 病患報到後未看診就離開（中途離院）
- [ ] 同一病患同日看多個科別
- [ ] 醫師看診後無需開藥（流程需跳過藥局段）
- [ ] 叫號後病患未到（過號處理）
- [ ] 藥局配藥發現缺藥
- [ ] 多個護理師同時操作同一候診佇列（並發控制）
- [ ] 病患用不同手機號多次報到（Identity Resolution 失敗場景）
- [ ] 診所營業時間外的報到請求

## 建議補充
- 考慮加入 **VisitTimeline / VisitEvent** 表記錄每次狀態變更的完整時間軸（可作為 audit trail）
- Workflow 的條件跳轉建議用 **JSON 規則引擎** 儲存在 WorkflowTransition 表
- 候診佇列建議 Redis 為主、MySQL VisitQueue 表為持久化備份
- Web Push subscription 應與 PatientDevice 表關聯，支援一人多裝置
