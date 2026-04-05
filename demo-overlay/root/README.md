# 醫療院所門診流程管理平台 (Demo)

Multi-tenant SaaS 門診流程管理系統，支援報到、候診、叫號、看診、處方、配藥完整流程。

## 技術棧

### 後端
- **ASP.NET Core 10** + Entity Framework Core + MySQL 8.0
- **Clean Architecture**（Domain / Application / Infrastructure / WebAPI）
- **SignalR** 即時通訊（叫號推播、狀態同步）
- **Redis** 候診佇列快取

### 前端
- **Vue 3** + TypeScript + Vite
- **Pinia** 狀態管理
- **PWA** 支援（離線 + Web Push）

### 基礎設施
- Docker Compose（MySQL + Redis 一鍵啟動）
- 24 張資料表，Workflow Engine 驅動流程

## 系統架構

```
病患 PWA ─┐
護理師端 ──┤                    ┌── MySQL (24 tables)
醫師端 ────┼── ASP.NET Core API ┤
藥劑師端 ──┤    + SignalR Hub   └── Redis (Queue Cache)
管理員端 ──┘
```

## 核心功能

### 門診流程（Workflow Engine 驅動）
```
報到 → 候診 → 叫號 → 看診 → 開處方 → 傳藥局 → 配藥 → 叫領藥 → 離院
```
- 流程步驟可由管理員自訂（資料驅動，非 hardcode）
- 支援條件跳轉（如免藥直接離院）

### AI 智慧分流
- 病患輸入症狀描述，AI 即時建議科別、優先度、預估等候
- 支援本地 OMLX（Ollama）和 Groq 雲端模型，自動 fallback
- ILlmClient 抽象層，provider 可插拔擴充

### 多元報到
- AI 智慧分流報到（描述症狀 → AI 建議科別）
- 手機 OTP 驗證報到
- QR Code 掃碼報到
- 護理師手動報到

### 即時叫號
- SignalR WebSocket 即時推播
- 病患 PWA 背景通知
- 優先號插隊邏輯

### 角色分端
| 角色 | 功能 |
|------|------|
| 病患 | 報到、查看候診進度、接收叫號通知 |
| 護理師 | 管理報到、操作叫號、手動報到 |
| 醫師 | 看診、開立處方 |
| 藥劑師 | 配藥佇列管理 |
| 管理員 | 流程設定、叫號規則、診所管理 |

## 快速啟動

### 前置需求
- .NET 10 SDK
- Node.js 22+
- Docker & Docker Compose

### 1. 啟動資料庫

```bash
docker compose up -d
```

### 2. 啟動後端

```bash
cd backend
dotnet ef database update \
  --project src/ClinicPlatform.Infrastructure/ClinicPlatform.Infrastructure.csproj \
  --startup-project src/ClinicPlatform.WebAPI/ClinicPlatform.WebAPI.csproj
dotnet run --project src/ClinicPlatform.WebAPI
```

### 3. 啟動前端

```bash
cd frontend
npm install
npm run dev
```

開啟 http://localhost:5173

## 資料庫設計

24 張表，分為 8 層：

| 層 | 表數 | 說明 |
|----|------|------|
| 租戶認證 | 4 | clinics, users, roles, user_roles |
| 病患裝置 | 3 | patients, patient_devices, otp_verifications |
| 門診設定 | 4 | departments, rooms, schedules, medications |
| Workflow | 3 | workflow_definitions, workflow_steps, workflow_transitions |
| 核心流程 | 5 | appointments, visits, visit_events, prescriptions, prescription_items |
| 叫號佇列 | 2 | queue_configs, queue_entries |
| 通知 | 2 | notification_templates, notification_logs |
| 稽核 | 1 | audit_logs |

## API 端點

| 端點 | 說明 |
|------|------|
| `POST /api/checkin/otp/send` | 發送 OTP |
| `POST /api/checkin/otp/verify` | OTP 報到 |
| `POST /api/checkin/qrcode` | QR Code 報到 |
| `POST /api/checkin/manual` | 手動報到 |
| `GET /api/queue/{clinicId}` | 候診佇列 |
| `POST /api/queue/call-next` | 叫下一號 |
| `POST /api/visits/{id}/start-consult` | 開始看診 |
| `POST /api/visits/{id}/complete-consult` | 完成看診 |
| `GET /api/pharmacy/queue` | 待配藥列表 |
| `POST /api/ai/triage` | AI 智慧症狀分流 |
| `WS /hubs/visit` | SignalR 即時通訊 |

## 授權

本專案為展示用途。
