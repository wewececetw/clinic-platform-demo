# 醫療院所門診流程管理平台 - 資料庫 Schema 設計規格

## 系統定位
Multi-tenant SaaS 平台，每間診所獨立設定。支援家醫科、醫美、牙科等不同型態診所，流程步驟與報到方式可依診所客製化設定。

## 核心角色
| 角色 | 說明 |
|------|------|
| Patient | 病患，透過 PWA 報到、候診、領藥 |
| Nurse | 護理師，管理報到、叫號、領藥通知 |
| Doctor | 醫師，看診、開立處方 |
| Pharmacist | 藥劑師，配藥、確認出藥 |
| Admin | 診所管理員，設定流程、叫號規則、報到方式 |

## 完整門診流程
```
報到(CheckIn) → 候診(Waiting) → 叫號進診間(Called) → 醫師看診(Consulting)
→ 開立處方(Prescribed) → 處方傳藥局(SentToPharmacy) → 藥劑師配藥(Dispensing)
→ 護理師叫領藥(ReadyForPickup) → 離院(Completed)
```

## 報到方式（診所可選設定）
1. **手機號碼 + OTP 驗證** — 病患輸入手機號，收 OTP 後完成報到
2. **QR Code 掃碼** — 掃描預約單 QR Code 自動報到
3. **護理師手動報到** — 護理師在後台手動為病患報到

## 關鍵設計需求

### Multi-tenant
- 所有資料表含 `clinic_id` 欄位
- 診所間資料完全隔離
- 查詢層一律帶 clinic_id 過濾

### Pluggable Workflow
- 流程步驟由管理員設定，非 hardcode
- 每間診所可自訂流程步驟順序、名稱、跳過邏輯
- 支援條件式跳轉（例：免藥直接離院）

### 病患 Identity Resolution
- 病患資料允許不完整（依報到方式而異）
- 同手機號碼自動合併為同一病患
- 支援匿名報到（護理師手動），後續可補資料合併

### 即時通知
- 每個流程步驟狀態變更時觸發通知
- SignalR 即時推播（在線用戶）
- Web Push 背景推播（PWA 離線/背景）

### 叫號規則
- 一般號：依報到順序
- 優先號：可插隊（VIP、急診、老幼等）
- 叫號規則可由管理員設定

## 技術環境
- 後端：ASP.NET Core + EF Core
- 資料庫：MySQL
- 快取：Redis（候診佇列）
- 即時通訊：SignalR
- 前端：Vue 3 + Vite（PWA）
- 病患端：手機瀏覽器 PWA + Web Push
- 管理後台：Vue 3 SPA

## 預期產出
1. 完整 ER Diagram（文字描述表、欄位、型別、關聯）
2. 每張表設計說明
3. Workflow Engine Schema 設計思路
4. Index 策略
5. 資料表數量與複雜度評估
6. 前後端資料流與 API 端點對應
