# 資料庫 Schema 設計計畫

## 設計原則
1. **Multi-tenant First**：所有業務表含 `clinic_id`，複合索引以 `clinic_id` 開頭
2. **Workflow 資料驅動**：流程定義存 DB，不 hardcode 狀態機
3. **Event Sourcing Lite**：每次狀態變更寫入 VisitEvent，支援完整軌跡回溯
4. **Identity Resolution by Phone**：手機號為病患唯一識別鍵，允許漸進式補齊資料

---

## 資料表分層架構

### Layer 1：租戶與認證（4 張表）
| 表名 | 用途 |
|------|------|
| `clinics` | 診所主檔（租戶） |
| `users` | 所有角色統一帳號表 |
| `roles` | 角色定義（Admin/Nurse/Doctor/Pharmacist） |
| `user_roles` | 使用者-角色多對多 |

### Layer 2：病患與裝置（3 張表）
| 表名 | 用途 |
|------|------|
| `patients` | 病患主檔（Identity Resolution 核心） |
| `patient_devices` | 病患裝置（Web Push subscription） |
| `otp_verifications` | OTP 驗證暫存 |

### Layer 3：門診基礎設定（4 張表）
| 表名 | 用途 |
|------|------|
| `departments` | 科別（家醫科、牙科、醫美等） |
| `rooms` | 診間/窗口 |
| `schedules` | 醫師排班表 |
| `medications` | 藥品主檔 |

### Layer 4：Workflow 引擎（3 張表）
| 表名 | 用途 |
|------|------|
| `workflow_definitions` | 流程定義（每診所一套或多套） |
| `workflow_steps` | 流程步驟定義（順序、名稱、可跳過條件） |
| `workflow_transitions` | 步驟間轉移規則（含條件 JSON） |

### Layer 5：門診核心流程（5 張表）
| 表名 | 用途 |
|------|------|
| `appointments` | 預約掛號 |
| `visits` | 門診紀錄（當日核心實體） |
| `visit_events` | 狀態變更事件流（Event Sourcing） |
| `prescriptions` | 處方主檔 |
| `prescription_items` | 處方明細（藥品+劑量+頻次） |

### Layer 6：叫號佇列（2 張表）
| 表名 | 用途 |
|------|------|
| `queue_configs` | 叫號規則設定（優先權重等） |
| `queue_entries` | 候診佇列條目（Redis 主、MySQL 持久化） |

### Layer 7：通知（2 張表）
| 表名 | 用途 |
|------|------|
| `notification_templates` | 通知模板（各步驟觸發的訊息模板） |
| `notification_logs` | 通知發送紀錄 |

### Layer 8：稽核（1 張表）
| 表名 | 用途 |
|------|------|
| `audit_logs` | 操作稽核紀錄 |

---

## 總計：24 張表

## Workflow Engine 設計思路

```
workflow_definitions (1) ──< workflow_steps (N) ──< workflow_transitions (N)
        │
        └── visits.workflow_definition_id FK
              │
              └── visit_events（記錄每次步驟轉移）
```

### 核心概念
- **WorkflowDefinition**：一套完整流程模板，診所可建多套（如：一般門診流程、免藥快速流程）
- **WorkflowStep**：流程中的單一步驟，含 `step_order`、`step_code`、`display_name`、`is_skippable`
- **WorkflowTransition**：步驟 A → 步驟 B 的轉移規則，`condition_json` 儲存跳步條件
- **Visit**：綁定一個 WorkflowDefinition，`current_step_id` 追蹤當前位置
- **VisitEvent**：每次 `current_step_id` 變更時寫入，含 `from_step`、`to_step`、`triggered_by`、`timestamp`

### 條件跳轉範例
```json
{
  "skip_when": {
    "field": "visit.needs_medication",
    "operator": "eq",
    "value": false
  }
}
```
當 `visit.needs_medication = false` 時，跳過 SentToPharmacy/Dispensing/ReadyForPickup 直達 Completed。

---

## 前後端資料流與 API 端點

### 病患端 PWA
| 端點 | 方法 | 說明 |
|------|------|------|
| `POST /api/checkin/otp/send` | POST | 發送 OTP |
| `POST /api/checkin/otp/verify` | POST | 驗證 OTP 完成報到 |
| `POST /api/checkin/qrcode` | POST | QR Code 掃碼報到 |
| `GET /api/visits/{id}/status` | GET | 查詢當前流程狀態 |
| `GET /api/queue/{clinicId}/position` | GET | 查詢候診位置 |
| `POST /api/devices/webpush/subscribe` | POST | 註冊 Web Push |
| **SignalR Hub**: `/hubs/visit` | WS | 即時狀態更新 |

### 護理師端
| 端點 | 方法 | 說明 |
|------|------|------|
| `POST /api/checkin/manual` | POST | 手動報到 |
| `GET /api/queue/{clinicId}` | GET | 候診佇列列表 |
| `POST /api/queue/call-next` | POST | 叫下一號 |
| `POST /api/queue/call-pickup/{visitId}` | POST | 叫領藥號 |
| `POST /api/visits/{id}/skip` | POST | 過號處理 |

### 醫師端
| 端點 | 方法 | 說明 |
|------|------|------|
| `GET /api/doctor/queue` | GET | 醫師待看診列表 |
| `POST /api/visits/{id}/start-consult` | POST | 開始看診 |
| `POST /api/visits/{id}/prescriptions` | POST | 開立處方 |
| `POST /api/visits/{id}/complete-consult` | POST | 完成看診（觸發下一步） |

### 藥劑師端
| 端點 | 方法 | 說明 |
|------|------|------|
| `GET /api/pharmacy/queue` | GET | 待配藥列表 |
| `POST /api/prescriptions/{id}/start-dispense` | POST | 開始配藥 |
| `POST /api/prescriptions/{id}/complete-dispense` | POST | 配藥完成 |

### 管理員端
| 端點 | 方法 | 說明 |
|------|------|------|
| `CRUD /api/admin/workflows` | ALL | 流程定義管理 |
| `CRUD /api/admin/workflow-steps` | ALL | 流程步驟管理 |
| `CRUD /api/admin/queue-configs` | ALL | 叫號規則管理 |
| `CRUD /api/admin/rooms` | ALL | 診間管理 |
| `CRUD /api/admin/departments` | ALL | 科別管理 |
| `PUT /api/admin/clinic/settings` | PUT | 診所設定（報到方式等） |
