# Tasks - 資料庫 Schema 設計

## Phase 1：租戶與認證層
- [ ] T1.1 設計 `clinics` 表（含診所設定 JSON：啟用的報到方式、營業時間等）
- [ ] T1.2 設計 `users` 表（Staff 帳號，含 clinic_id 隔離）
- [ ] T1.3 設計 `roles` + `user_roles` 表（RBAC）

## Phase 2：病患與裝置層
- [ ] T2.1 設計 `patients` 表（手機號為 Identity Resolution 主鍵）
- [ ] T2.2 設計 `patient_devices` 表（Web Push subscription）
- [ ] T2.3 設計 `otp_verifications` 表（含過期機制）

## Phase 3：門診基礎設定
- [ ] T3.1 設計 `departments` 表
- [ ] T3.2 設計 `rooms` 表
- [ ] T3.3 設計 `schedules` 表（醫師排班）
- [ ] T3.4 設計 `medications` 表

## Phase 4：Workflow 引擎
- [ ] T4.1 設計 `workflow_definitions` 表
- [ ] T4.2 設計 `workflow_steps` 表（含 step_order, is_skippable, required_role）
- [ ] T4.3 設計 `workflow_transitions` 表（含 condition_json）

## Phase 5：門診核心流程
- [ ] T5.1 設計 `appointments` 表
- [ ] T5.2 設計 `visits` 表（核心實體，含 current_step_id FK）
- [ ] T5.3 設計 `visit_events` 表（Event Sourcing）
- [ ] T5.4 設計 `prescriptions` + `prescription_items` 表

## Phase 6：叫號佇列
- [ ] T6.1 設計 `queue_configs` 表（優先權重規則）
- [ ] T6.2 設計 `queue_entries` 表（MySQL 持久化 + Redis 快取策略）

## Phase 7：通知
- [ ] T7.1 設計 `notification_templates` 表
- [ ] T7.2 設計 `notification_logs` 表

## Phase 8：稽核
- [ ] T8.1 設計 `audit_logs` 表

## Phase 9：Index 策略
- [ ] T9.1 定義所有表的 Index（含複合索引、唯一約束）

## Phase 10：輸出完整文件
- [ ] T10.1 撰寫完整 ER Diagram 文字描述
- [ ] T10.2 撰寫每張表設計說明
- [ ] T10.3 撰寫 Workflow Engine 設計思路
- [ ] T10.4 複雜度評估
