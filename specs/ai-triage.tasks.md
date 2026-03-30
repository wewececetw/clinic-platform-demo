# AI 智慧分流系統 — Tasks

## Phase 1：智慧症狀分流

### Task 1: Application 層 DTO 與介面
- [x] 建立 `Features/AI/AiDtos.cs`
- [x] 建立 `Features/AI/IAiService.cs`

### Task 2: LLM Client 抽象層
- [x] 建立 `Services/AI/LlmMessage.cs` — 共用模型
- [x] 建立 `Services/AI/ILlmClient.cs` — 介面
- [x] 建立 `Services/AI/OmlxLlmClient.cs` — Ollama 實作
- [x] 建立 `Services/AI/GroqLlmClient.cs` — Groq 實作

### Task 3: AI Service 與 Prompt
- [x] 建立 `Services/AI/TriagePromptBuilder.cs` — 分流 Prompt
- [x] 建立 `Services/AI/AiService.cs` — 實作 IAiService

### Task 4: DI 與設定
- [x] 修改 `DependencyInjection.cs` 註冊 AI 服務
- [x] 修改 `appsettings.json` 加 AI 設定
- [x] 加入 `Microsoft.Extensions.Http` 套件

### Task 5: WebAPI Controller
- [x] 建立 `Controllers/AiController.cs`

### Task 6: 前端整合
- [x] 修改 `api/client.ts` 加 aiTriage 呼叫
- [x] 修改 `views/patient/CheckIn.vue` 加 AI 分流 tab

### 驗證
- [x] `dotnet build` 編譯通過

## Phase 2：自然語言指令（後續）
- [ ] CommandPromptBuilder
- [ ] AiService.CommandAsync 實作
- [ ] 醫護端指令輸入框
