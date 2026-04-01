# Implementation Plan: natural-language-command

> Methodology: Spec-Driven Development | Tool: Vibe Lens
> Inspired by [GitHub Spec Kit](https://github.com/github/spec-kit)

**Created**: 2026-03-31
**Spec**: specs/natural-language-command/spec.md

---

## Summary

實作自然語言指令系統，讓醫護人員用口語化文字操作門診系統。後端以 Clean Architecture 分層，LLM 解析意圖後由 Action Router 路由到對應 Executor 執行。

## Technical Context

| Aspect | Decision |
|--------|----------|
| Language/Version | C# / .NET 10, TypeScript |
| Primary Dependencies | ASP.NET Core 10, EF Core, MySQL, Vue 3 + Vite, ILlmClient (OMLX/Groq) |
| Storage | MySQL (持久化), Redis (佇列快取) |
| Testing Framework | xUnit (後端), Vitest (前端) |
| Target Platform | Linux container (Docker) |
| Performance Goals | LLM 回應 < 3 秒 (P95) |

## Architecture

### 後端 — Clean Architecture 四層

```
Domain/
  Enums/CommandAction.cs          ← 6 種 action + Unknown

Application/
  Features/AI/
    CommandRequest.cs             ← DTO: ClinicId, Role, Command
    CommandResponse.cs            ← DTO: Action, Parameters, Result, Message
    ICommandExecutor.cs           ← 介面: CanHandle(action), ExecuteAsync(context)
    Executors/
      CallNextExecutor.cs         ← 對接 QueueService.CallNextAsync
      SkipExecutor.cs             ← 對接 QueueService.SkipAsync
      QueryQueueExecutor.cs       ← 對接 QueueService.GetQueueAsync
      CreatePrescriptionExecutor.cs ← 對接 PrescriptionService (Phase 2+)
      CompleteConsultExecutor.cs   ← 對接 ConsultService.CompleteAsync
      QueryStatsExecutor.cs       ← 對接 StatsService.GetTodayAsync

Infrastructure/
  Services/AI/
    CommandPromptBuilder.cs       ← 依角色建構 system prompt (已完成)
    AiService.cs                  ← CommandAsync: LLM 呼叫 + JSON 解析 (已完成)
    CommandRouter.cs              ← 依 action string 路由到對應 ICommandExecutor

WebAPI/
  Controllers/AiController.cs
    POST /api/ai/command          ← 解析指令 (回傳 action + needsConfirm)
    POST /api/ai/command/execute  ← 確認後執行操作類指令
```

### 前端 — Vue 3

```
src/
  components/
    CommandInput.vue              ← 共用指令輸入框 + 確認對話框
  views/
    NurseDashboard.vue            ← 嵌入 CommandInput
    DoctorDashboard.vue           ← 嵌入 CommandInput
    AdminDashboard.vue            ← 嵌入 CommandInput
  api/
    ai.ts                         ← commandAsync(), executeCommandAsync()
```

### 指令處理流程

```
使用者輸入自然語言
  → POST /api/ai/command {role, command}
  → CommandPromptBuilder 建構 prompt（依角色限制 action）
  → ILlmClient.ChatAsync（OMLX → Groq fallback）
  → ParseCommandResponse 解析 JSON
  → 回傳 CommandResponse {action, params, needsConfirm, message}

前端收到回應：
  查詢類 (needsConfirm=false) → 直接顯示結果
  操作類 (needsConfirm=true)  → 顯示確認對話框
    → 使用者確認 → POST /api/ai/command/execute {action, params}
    → CommandRouter → ICommandExecutor.ExecuteAsync
    → 回傳執行結果
```

## API Contracts

### POST /api/ai/command — 解析指令

Request:
```json
{
  "clinicId": "guid",
  "role": "Nurse|Doctor|Admin",
  "command": "叫下一位"
}
```

Response:
```json
{
  "action": "call_next",
  "parameters": {},
  "result": "confirm",
  "message": "確認要叫下一位候診病患嗎？"
}
```

### POST /api/ai/command/execute — 執行指令

Request:
```json
{
  "clinicId": "guid",
  "role": "Nurse",
  "action": "call_next",
  "parameters": {}
}
```

Response:
```json
{
  "success": true,
  "message": "已叫號：4 號 王小明",
  "data": { "queueNumber": 4, "patientName": "王小明" }
}
```

## Key Design Decisions

1. **兩階段 API（解析 + 執行）**：操作類指令需使用者確認，避免 LLM 誤判導致錯誤操作
2. **ICommandExecutor 介面**：每個 action 獨立一個 executor，符合 OCP，新增 action 不修改現有程式碼
3. **CommandRouter DI 注入**：透過 IEnumerable<ICommandExecutor> 自動發現所有 executor，無需手動註冊
4. **角色權限雙重檢查**：Prompt 層限制 + Router 層驗證，防止 LLM 幻覺產生無權限 action

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| LLM 回傳非預期 action | 執行錯誤操作 | Router 做 action 白名單 + 角色二次驗證 |
| LLM 參數解析錯誤（如號碼錯誤） | 過號/開藥錯誤 | 操作類一律需確認，前端顯示完整參數讓使用者核對 |
| 處方藥品名稱 LLM 無法正確對應 | 建立錯誤處方 | Phase 2+ 加入藥品名稱模糊匹配 + 確認機制 |
| OMLX 本地模型理解力不足 | 解析成功率低 | Groq fallback + 持續優化 prompt |
