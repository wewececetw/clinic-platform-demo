# AI 功能發展藍圖（商業版 — main）

## 願景
透過 AI 將傳統門診流程從「人工驅動」升級為「智慧輔助」，降低人力成本、提升病患體驗、減少醫療疏失。

---

## Phase 1：智慧症狀分流 ✅
**目標**：病患自助報到時，AI 自動建議科別與優先度

| 項目 | 說明 |
|------|------|
| 輸入 | 自然語言症狀描述 |
| 輸出 | 建議科別、優先度、預估等候、推理說明 |
| 模型 | OMLX（本地）優先、Groq（雲端）備援 |
| 架構 | ILlmClient 抽象層 → AiService → AiController |

**已完成**：DTO、LLM Client（OMLX + Groq）、AiService、Prompt Builder、前端 AI Tab

---

## Phase 2：自然語言指令
**目標**：醫護人員用自然語言操作系統，取代手動點按

### 護理師指令
| 指令範例 | 對應 Action | 呼叫的 Service |
|----------|-------------|----------------|
| 「叫下一位」 | call_next | QueueService.CallNextAsync |
| 「3 號過號」 | skip_visit | QueueService.SkipAsync |
| 「幫王先生手動報到」 | manual_checkin | CheckInService.ManualCheckInAsync |

### 醫師指令
| 指令範例 | 對應 Action | 呼叫的 Service |
|----------|-------------|----------------|
| 「開始看診」 | start_consult | VisitService.StartConsultAsync |
| 「完成看診，需要領藥」 | complete_consult | VisitService.CompleteConsultAsync |
| 「開普拿疼 500mg TID 三天份」 | create_prescription | PrescriptionService.CreateAsync |

### 技術方案
- **CommandPromptBuilder**：給定角色 + 可用 action 清單，要求 JSON 回傳 `{action, params}`
- **ActionRouter**：根據 action 分派到對應 Service 方法
- **安全機制**：角色權限檢查（護理師不能開處方、醫師不能操作叫號）
- **確認機制**：危險操作（如過號）需二次確認

---

## Phase 3：AI 輔助看診（CDSS）
**目標**：臨床決策支援系統，輔助醫師診斷與用藥

### 功能
- 根據症狀 + 病歷摘要，建議可能的鑑別診斷
- 藥品交互作用檢查
- 用藥劑量建議（依年齡、體重）
- ICD-10 診斷碼自動建議

### 技術方案
- **RAG 架構**：醫療知識庫 → 向量化 → 語意檢索 → LLM 生成建議
- **向量資料庫**：Qdrant（自架）或 ChromaDB（輕量）
- **知識來源**：藥品仿單、診療指引、NHI 給付規範
- **嵌入模型**：BGE-M3（多語言，支援中文）

### 合規要求
- 明確標示「AI 建議僅供參考」
- 所有 AI 建議需記錄到 audit_logs
- 醫師有最終決定權，AI 不可自動執行

---

## Phase 4：智慧排程優化
**目標**：預測候診時間、動態優化叫號順序

### 功能
- 預測每位病患看診時長（依科別、症狀複雜度、醫師習慣）
- 動態調整佇列順序（最小化整體等候時間）
- 預測尖峰時段，建議加診或分流

### 技術方案
- **特徵工程**：歷史看診時長、科別、時段、醫師、症狀分類
- **模型**：LightGBM 回歸 或 ML.NET 時序預測
- **訓練數據**：visit_events 表的 created_at 時間差
- **推理**：模型輕量化，可內嵌 .NET 或用 Python sidecar

---

## Phase 5：多模態 AI
**目標**：語音、影像等多模態輸入

### 語音指令
- Web Speech API（瀏覽器端語音轉文字）
- Whisper 模型（本地或 API，支援中文醫療術語）
- 語音 → 文字 → Phase 2 自然語言指令管道

### 影像辨識
- 傷口照片初步分類（燒燙傷分級、外傷類型）
- 處方箋 OCR（紙本處方數位化）
- Vision 模型：LLaVA / GPT-4o / Claude Vision

---

## 技術架構演進

```
Phase 1-2（現在）          Phase 3-4（中期）           Phase 5（遠期）
┌─────────────┐         ┌─────────────────┐       ┌──────────────────┐
│  LLM Client │         │  LLM + RAG      │       │  LLM + RAG       │
│  (OMLX/Groq)│         │  + 向量 DB      │       │  + 向量 DB       │
│             │         │  + ML.NET       │       │  + ML.NET        │
│  純文字 I/O │         │  知識庫檢索     │       │  + Whisper       │
│             │         │  預測模型       │       │  + Vision        │
└─────────────┘         └─────────────────┘       └──────────────────┘
```

## 成本估算

| Phase | 模型成本 | 基礎設施 | 開發時程 |
|-------|----------|----------|----------|
| 1 | 免費（本地 + Groq free） | 現有 | ✅ 已完成 |
| 2 | 免費 | 現有 | 1-2 週 |
| 3 | 嵌入模型免費（BGE-M3），LLM 同上 | +向量 DB（1 vCPU） | 3-4 週 |
| 4 | ML.NET 免費 | 現有 | 2-3 週 |
| 5 | Whisper 免費（本地）| +GPU（語音） | 4-6 週 |
