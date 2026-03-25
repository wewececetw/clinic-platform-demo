# 一致性分析報告

## Spec → Plan 比對

### ✅ Spec 已涵蓋
- Multi-tenant clinic_id → `clinics` 表 ✓
- Pluggable Workflow → `workflow_definitions` / `workflow_steps` / `workflow_transitions` ✓
- Identity Resolution → `patients` 表 phone 欄位 ✓
- 即時通知 SignalR → `notification_logs` / `notification_templates` ✓
- Web Push → `patient_devices` 表 ✓
- OTP 報到 → `otp_verifications` 表 ✓
- QR Code 報到 → `appointments` 表（產生 QR Code 依據）✓
- 護理師手動報到 → `visits` 表 checkin_method 欄位 ✓
- 叫號規則 → `queue_configs` / `queue_entries` ✓
- 5 個角色 → `users` / `roles` / `user_roles` ✓
- 9 步流程 → workflow_steps 資料驅動 ✓

### ⚠️ Spec 有提但 Plan 需補強
- [ ] **診所設定（報到方式選擇）**：Spec 說「每間診所選擇報到方式」，Plan 的 `clinics` 表需確保有 `settings_json` 欄位存報到方式配置
- [ ] **過號處理**：Spec 的叫號流程隱含過號邏輯，`queue_entries` 需有 `status` 欄位（waiting/called/skipped/completed）

## Plan → Spec 比對（Plan 額外新增的表）

以下表在 Spec 未明確提到，但 Plan 認為必要：
| 表 | 理由 | 是否必要 |
|----|------|----------|
| `departments` | 多科別診所需要 | ✅ 必要 |
| `rooms` | 叫號需指定診間 | ✅ 必要 |
| `schedules` | 醫師排班 | ✅ 必要 |
| `medications` | 處方配藥需要藥品主檔 | ✅ 必要 |
| `audit_logs` | 醫療合規需求 | ✅ 必要 |
| `visit_events` | Event Sourcing for 狀態追蹤 | ✅ 強烈建議 |
| `appointments` | QR Code 報到的前提 | ✅ 必要 |

**結論**：Plan 新增的表全部合理必要。

## Tasks → Plan 比對

### ✅ 所有 24 張表已在 Tasks 中分配到對應 Phase
### ✅ Index 策略有獨立 Phase (T9)
### ✅ 文件輸出有獨立 Phase (T10)

### ⚠️ Tasks 可能遺漏
- [ ] **Seed Data 任務**：缺少初始資料（預設 Workflow 模板、預設角色）的建立任務
- [ ] **Migration 腳本**：Tasks 未提 EF Core Migration 生成步驟
- [ ] **Redis 快取策略**：T6.2 提到 Redis 但未獨立為設計任務，建議補充 Redis 資料結構設計

## 建議修正
1. `clinics` 表增加 `allowed_checkin_methods` JSON 欄位
2. `queue_entries` 表增加 `status` 與 `called_at` / `skipped_at` 時間戳
3. Tasks 補充 Phase 0：Seed Data & Default Workflow Template
4. Tasks 補充：Redis Queue 資料結構設計（sorted set by priority + checkin_time）
