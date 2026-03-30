# AI 智慧分流系統 — Plan

## 實作計畫

### Step 1：Application 層 — 介面與 DTO
- `Features/AI/IAiService.cs` — 介面定義
- `Features/AI/AiDtos.cs` — Request/Response DTO

### Step 2：Infrastructure 層 — LLM Client 抽象
- `Services/AI/ILlmClient.cs` — LLM 呼叫介面
- `Services/AI/LlmMessage.cs` — 共用訊息模型
- `Services/AI/OmlxLlmClient.cs` — 本地 Ollama 實作
- `Services/AI/GroqLlmClient.cs` — Groq API 實作

### Step 3：Infrastructure 層 — AI Service 實作
- `Services/AI/AiService.cs` — 實作 IAiService
  - TriageAsync：症狀分流（組裝 prompt → 呼叫 LLM → 解析 JSON 回應）
  - CommandAsync：自然語言指令（Phase 2）
- `Services/AI/TriagePromptBuilder.cs` — 分流 Prompt 模板

### Step 4：DI 註冊與設定
- `DependencyInjection.cs` — 註冊 AI 相關服務
- `appsettings.json` — AI 設定區段（provider、model、endpoint）

### Step 5：WebAPI 層 — Controller
- `Controllers/AiController.cs` — POST /api/ai/triage, POST /api/ai/command

### Step 6：前端 — 報到頁 AI 分流 Tab
- `api/client.ts` — 新增 aiTriage API 呼叫
- `views/patient/CheckIn.vue` — 新增「AI 分流」tab
  - 症狀輸入框
  - AI 建議卡片（科別、優先度、原因）
  - 一鍵報到按鈕

### Step 7：設定檔
- `appsettings.json` 加入 AI 設定
- `.env.example` 加入 GROQ_API_KEY

## 檔案清單

```
backend/src/ClinicPlatform.Application/Features/AI/
├── IAiService.cs
└── AiDtos.cs

backend/src/ClinicPlatform.Infrastructure/Services/AI/
├── ILlmClient.cs
├── LlmMessage.cs
├── OmlxLlmClient.cs
├── GroqLlmClient.cs
├── AiService.cs
└── TriagePromptBuilder.cs

backend/src/ClinicPlatform.WebAPI/Controllers/
└── AiController.cs

frontend/src/
├── api/client.ts （修改）
└── views/patient/CheckIn.vue （修改）
```
