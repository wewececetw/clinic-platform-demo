# 醫療院所門診流程管理平台 — 完整資料庫 Schema 設計

---

## 目錄
1. [ER Diagram（文字描述）](#1-er-diagram)
2. [每張表的設計說明](#2-每張表的設計說明)
3. [Workflow Engine Schema 設計思路](#3-workflow-engine-schema-設計思路)
4. [Index 策略](#4-index-策略)
5. [資料表數量與複雜度評估](#5-資料表數量與複雜度評估)
6. [前後端資料流說明](#6-前後端資料流說明)

---

## 1. ER Diagram

### 關聯總覽

```
clinics (1)─┬──< users (N)
             ├──< patients (N)
             ├──< departments (N) ──< rooms (N)
             ├──< medications (N)
             ├──< workflow_definitions (N) ──< workflow_steps (N)
             │                               └──< workflow_transitions (N)
             ├──< queue_configs (N)
             ├──< notification_templates (N)
             └──< appointments (N)

users (N) >──── roles (M)          [透過 user_roles]
users (1) ──< schedules (N)

patients (1)─┬──< patient_devices (N)
              ├──< appointments (N)
              └──< visits (N)

visits (1)─┬──< visit_events (N)
            ├──  prescriptions (1)
            └──  queue_entries (1)

prescriptions (1) ──< prescription_items (N) >── medications

workflow_definitions (1) ──< visits (N)
workflow_steps (1) ──< visits.current_step_id
```

完整 24 張表的欄位、型別、關聯請見各 Entity 原始碼：
`backend/src/ClinicPlatform.Domain/Entities/`

完整 EF Core 欄位映射、Index 定義請見：
`backend/src/ClinicPlatform.Infrastructure/Persistence/Configurations/`

---

## 2. 每張表的設計說明

| 表 | 設計理由 |
|----|----------|
| **clinics** | Multi-tenant 根實體。`settings_json` 以 JSON 存診所設定（報到方式、營業時間），避免為每個設定建欄位 |
| **users / roles / user_roles** | RBAC 三件套。roles 全域共享，user_roles 含 clinic_id 支援跨診所角色 |
| **patients** | Identity Resolution 核心。`phone` 為辨識主鍵，`is_anonymous` 支援匿名報到後合併 |
| **patient_devices** | Web Push subscription 儲存（endpoint + p256dh + auth_key），一人多裝置 |
| **otp_verifications** | 短效暫存，刻意不建 Patient FK（發 OTP 時病患可能尚未存在） |
| **departments / rooms** | 科別→診間階層，`room_type` 區分看診間/藥局窗口/治療室 |
| **schedules** | `day_of_week_flags`（Flags 位元遮罩）+ `time_slot` 組合，一筆記錄代表多天排班 |
| **medications** | 藥品主檔，`code` 診所內唯一 |
| **workflow_definitions** | 流程模板，每診所可建多套（一般門診、免藥快速等） |
| **workflow_steps** | 流程步驟，`required_role` 控制誰可觸發轉移，`is_skippable` 支援條件跳過 |
| **workflow_transitions** | 步驟間有向圖，`condition_json` 存跳步條件，`priority` 控制評估順序 |
| **appointments** | 預約掛號，`qr_code_token` 為 QR Code 掃碼報到的唯一 token |
| **visits** | **系統核心**。`current_step_id` 追蹤 workflow 位置，`needs_medication` 控制跳步 |
| **visit_events** | Event Sourcing Lite — 每次狀態轉移都記錄，支援軌跡回溯和效能分析 |
| **prescriptions / prescription_items** | 處方 1:1 綁定 visit，明細為結構化藥品+劑量+頻次 |
| **queue_configs** | 叫號規則設定，`priority_weight` 定義優先權重 |
| **queue_entries** | MySQL 持久化層 + Redis Sorted Set 即時佇列雙寫 |
| **notification_templates** | 每個 workflow step 的通知模板，支援變數替換 |
| **notification_logs** | 通知發送紀錄，追蹤 Pending→Sent→Delivered/Failed |
| **audit_logs** | 醫療合規必備，JSON 存變更前後值 |

---

## 3. Workflow Engine Schema 設計思路

### 核心模型
```
WorkflowDefinition (1) ──< WorkflowStep (N) ──< WorkflowTransition (N)
        │
        └── Visit.workflow_definition_id FK
              │
              ├── Visit.current_step_id → WorkflowStep
              └── VisitEvent（記錄每次步驟轉移）
```

### 設計原則

1. **定義與實例分離**：模板（definitions/steps/transitions）vs 執行實例（visits/events）。修改模板不影響進行中門診。

2. **有向圖而非線性鏈**：`from_step_id → to_step_id` 支援線性流程、條件跳轉、未來可擴展迴圈。

3. **條件跳轉**：`condition_json` 存 JSON 規則：
```json
{"skip_when":{"field":"visit.needs_medication","operator":"eq","value":false}}
```
`priority` 控制評估順序（高優先先評估），條件成立則跳轉。

4. **角色控制**：`required_role` 指定觸發角色權限。

### 狀態轉移流程
```
前端觸發 → 後端 WorkflowEngine：
  1. 讀取當前步驟的 outgoing transitions（ORDER BY priority DESC）
  2. 逐一評估 condition_json
  3. 第一個成立 → 跳到 to_step；都不成立 → 走 priority=0 預設
  4. 更新 visit.current_step_id
  5. 寫入 visit_event
  6. 觸發 notification（查 templates by step_code）
  7. SignalR Hub 推送即時更新
```

---

## 4. Index 策略

所有複合索引以 `clinic_id` 開頭（Multi-tenant 查詢最佳化）。

### 關鍵 Index 列表

| 表 | Index 欄位 | 類型 | 用途 |
|----|-----------|------|------|
| clinics | slug | UNIQUE | URL 路由 |
| users | (clinic_id, email) | UNIQUE | 登入 |
| patients | (clinic_id, phone) | INDEX | Identity Resolution |
| departments | (clinic_id, code) | UNIQUE | 代碼查詢 |
| medications | (clinic_id, code) | UNIQUE | 代碼查詢 |
| appointments | qr_code_token | UNIQUE | QR 報到 |
| appointments | (clinic_id, appointment_date) | INDEX | 日期查詢 |
| workflow_steps | (workflow_definition_id, step_order) | UNIQUE | 步驟順序 |
| visits | (clinic_id, status) | INDEX | **主查詢** |
| visits | (clinic_id, checked_in_at) | INDEX | 報到排序 |
| visit_events | (clinic_id, visit_id, created_at) | INDEX | 事件軸 |
| queue_entries | (clinic_id, queue_type, status) | INDEX | **佇列主查詢** |
| prescriptions | (clinic_id, status) | INDEX | 處方查詢 |
| audit_logs | (clinic_id, entity_type, entity_id) | INDEX | 稽核查詢 |

---

## 5. 資料表數量與複雜度評估

| 分類 | 表數 | 複雜度 |
|------|------|--------|
| 租戶與認證 | 4 | 低 |
| 病患與裝置 | 3 | 中 |
| 門診基礎設定 | 4 | 低 |
| Workflow 引擎 | 3 | **高** |
| 門診核心流程 | 5 | **高** |
| 叫號佇列 | 2 | 中 |
| 通知 | 2 | 低 |
| 稽核 | 1 | 低 |
| **合計** | **24** | |

### 資料量預估（單一中型診所，日均 100 病患）
| 表 | 年累積 |
|----|--------|
| visits | 36,000 |
| visit_events | 324,000 |
| notification_logs | 180,000 |
| audit_logs | 360,000 |

→ 年度約 100 萬筆寫入，MySQL 8.0 輕鬆應對。

---

## 6. 前後端資料流說明

### 病患端 PWA
| API | 說明 |
|-----|------|
| `POST /api/checkin/otp/send` | 發送 OTP |
| `POST /api/checkin/otp/verify` | 驗證 OTP 完成報到 |
| `POST /api/checkin/qrcode` | QR Code 報到 |
| `GET /api/visits/{id}/status` | 查詢流程狀態 |
| `GET /api/queue/{clinicId}/position` | 候診位置（Redis） |
| `POST /api/devices/webpush/subscribe` | 註冊 Web Push |
| **WS** `/hubs/visit` | SignalR 即時更新 |

### 護理師端
| API | 說明 |
|-----|------|
| `POST /api/checkin/manual` | 手動報到 |
| `GET /api/queue/{clinicId}` | 候診佇列 |
| `POST /api/queue/call-next` | 叫下一號 |
| `POST /api/queue/call-pickup/{visitId}` | 叫領藥號 |
| `POST /api/visits/{id}/skip` | 過號 |

### 醫師端
| API | 說明 |
|-----|------|
| `GET /api/doctor/queue` | 待看診列表 |
| `POST /api/visits/{id}/start-consult` | 開始看診 |
| `POST /api/visits/{id}/prescriptions` | 開立處方 |
| `POST /api/visits/{id}/complete-consult` | 完成看診 |

### 藥劑師端
| API | 說明 |
|-----|------|
| `GET /api/pharmacy/queue` | 待配藥列表 |
| `POST /api/prescriptions/{id}/start-dispense` | 開始配藥 |
| `POST /api/prescriptions/{id}/complete-dispense` | 配藥完成 |

### 管理員端
| API | 說明 |
|-----|------|
| `CRUD /api/admin/workflows` | 流程管理 |
| `CRUD /api/admin/queue-configs` | 叫號規則 |
| `CRUD /api/admin/rooms` | 診間 |
| `CRUD /api/admin/departments` | 科別 |
| `CRUD /api/admin/medications` | 藥品 |
| `CRUD /api/admin/schedules` | 排班 |
| `PUT /api/admin/clinic/settings` | 診所設定 |
| `GET /api/admin/audit-logs` | 稽核紀錄 |

### 即時通訊架構
```
Visit 狀態變更
    ├─→ VisitEvent 寫入 DB
    ├─→ SignalR Hub 推送（在線即時）
    └─→ Web Push 推播（離線/背景）
```
