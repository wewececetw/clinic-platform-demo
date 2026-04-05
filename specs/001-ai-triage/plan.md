# Implementation Plan: AI 智慧症狀分流

**Branch**: `001-ai-triage` | **Date**: 2026-04-05 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-ai-triage/spec.md`

**Note**: 本文檔為反向補寫（Reverse-engineered），對應已完成的實作（commits dc7f0a3, ab21fc6, 3cdc866）。

## Summary

建立 LLM 抽象層（`ILlmClient`）以 OpenAI 相容格式統一 OMLX（本地 Ollama）與 Groq（雲端）兩個 provider，由 `AiService` 依設定檔優先序嘗試，失敗自動 fallback。`TriagePromptBuilder` 動態組合該院所的科別清單產生 system prompt，強制模型回傳單行 JSON。解析器 `ParseTriageResponse` 使用含 `department` 欄位關鍵字的 JSON 擷取邏輯，以容忍 thinking process 與 markdown 包裝。

## Technical Context

**Language/Version**：C# 13 / .NET 10
**Primary Dependencies**：ASP.NET Core 10、EF Core 10、`Microsoft.Extensions.Http`、`System.Text.Json`
**Storage**：MySQL 8（科別清單查詢，無 AI 呼叫日誌落地）
**Testing**：xUnit（單元測試目標：`ParseTriageResponse`、`ExtractJson`）
**Target Platform**：Linux server（docker-compose 佈署）
**Project Type**：Web service（Clean Architecture，AI 歸屬 Infrastructure 層）
**Performance Goals**：P95 回應時間 ≤ 3 秒（OMLX 本地）、fallback 切換 ≤ 10 秒
**Constraints**：病患症狀資料優先留在本地（OMLX 預設）、Groq 僅作備援；`max_tokens=256`、`temperature=0.2`
**Scale/Scope**：單院所科別數 5-20 個、單次請求 symptom 字串 ≤ 500 字

## Constitution Check

（專案 constitution 尚未建立，依 CLAUDE.md 原則檢核）

- ✅ **繁體中文**：所有 log、錯誤訊息、reasoning 皆為繁中
- ✅ **Clean Architecture**：`IAiService` 定義於 Application 層、`AiService` 實作於 Infrastructure 層
- ✅ **Provider 抽象**：`ILlmClient` 介面隔離具體 LLM 實作，符合 Open/Closed 原則
- ✅ **錯誤不中斷**：使用 `Result<T>` 回傳，失敗不拋例外至 Controller

## Project Structure

### Documentation

```text
specs/001-ai-triage/
├── spec.md              # 功能規格（WHAT & WHY）
├── plan.md              # 本檔案（HOW）
└── tasks.md             # 任務清單（全部標記完成）
```

### Source Code

```text
backend/src/
├── ClinicPlatform.Application/
│   └── Features/AI/
│       ├── IAiService.cs           # 服務介面
│       ├── TriageRequest.cs        # 請求 DTO
│       └── TriageResponse.cs       # 回應 DTO
│
├── ClinicPlatform.Infrastructure/
│   └── Services/AI/
│       ├── AiService.cs            # 主要業務邏輯 + provider 協調
│       ├── ILlmClient.cs           # LLM 抽象介面
│       ├── LlmMessage.cs           # LLM 請求/回應 record
│       ├── OmlxLlmClient.cs        # 本地 Ollama/OMLX 實作
│       ├── GroqLlmClient.cs        # Groq 雲端實作
│       └── TriagePromptBuilder.cs  # 分流 prompt 組裝
│
└── ClinicPlatform.WebAPI/
    └── Controllers/
        └── AiController.cs         # POST /api/ai/triage
```

## Architecture

### 呼叫流程

```
Client (Vue 前端)
   │ POST /api/ai/triage { clinicId, symptoms }
   ▼
AiController
   │ 呼叫 _aiService.TriageAsync(request)
   ▼
AiService
   │ 1. 查詢 DB 取得 ClinicId 的科別清單
   │ 2. TriagePromptBuilder 組 system + user prompt
   │ 3. 依 AI:Provider 設定排序 llmClients
   │ 4. foreach client: try → parse → return
   ▼
ILlmClient.ChatAsync  ← OmlxLlmClient / GroqLlmClient
   │ 送出 OpenAI 相容格式 HTTP POST
   ▼
回傳 LlmResponse (Content, PromptTokens, CompletionTokens)
   │
AiService.ParseTriageResponse
   │ ExtractJson (關鍵字 "department")
   │ JsonSerializer.Deserialize → TriageResponse
   ▼
Result<TriageResponse>  →  Controller  →  Client
```

### 關鍵設計決策

| 決策 | 理由 |
|------|------|
| 使用 OpenAI 相容格式 | OMLX/Ollama/Groq 皆支援，可共用 `OpenAiChatResponse` 反序列化模型 |
| 採 `IEnumerable<ILlmClient>` DI 注入 | 支援任意數量 provider，加新 provider 僅需 `services.AddHttpClient<ILlmClient, XxxLlmClient>()` |
| 依 `ProviderName` 字串排序，不用 enum | 設定檔 `AI:Provider` 保持彈性，增 provider 不需改程式碼 |
| `ExtractJson` 從後往前掃描 | 非思考模型的 JSON 通常在最後；跳過 `<think>` 區塊 |
| `requiredKey` 參數化（"department" / "action"）| 同一套擷取邏輯可複用於分流與指令兩個功能 |
| 溫度 0.2 / max_tokens 256 | 壓低發散性、限制輸出長度，拉高解析成功率 |
| 使用 Qwen2.5-7B **非思考**模型 | 思考模型會輸出 `<think>` 拖慢響應（20 秒 → 2-3 秒） |

### 容錯策略

1. **HTTP 層**：`HttpClient.SendAsync` 失敗 → catch Exception，log warning，嘗試下一個 provider
2. **解析層**：JSON 無法解析或缺 `department` 欄位 → `ParseTriageResponse` 回 null，視同該 provider 失敗
3. **全部失敗**：回傳 `Result.Fail("AI 分流服務暫時無法使用，請手動選擇科別")`，Controller 不拋 500

## Configuration

```jsonc
// appsettings.json
{
  "AI": {
    "Provider": "Omlx",               // 預設優先 provider
    "Omlx": {
      "Endpoint": "http://localhost:9000/v1/chat/completions",
      "ApiKey": "",                    // 可選
      "Model": "Qwen2.5-7B-Instruct"
    },
    "Groq": {
      "ApiKey": "gsk_...",             // 從環境變數或 user-secrets 讀取
      "Model": "llama-3.3-70b-versatile"
    }
  }
}
```

## DI Registration

```csharp
// Program.cs / ServiceCollectionExtensions
services.AddHttpClient<ILlmClient, OmlxLlmClient>();
services.AddHttpClient<ILlmClient, GroqLlmClient>();
services.AddScoped<IAiService, AiService>();
```

## Out of Scope（本 Phase 不做）

- AI 呼叫日誌落地 DB（目前僅 `ILogger` 輸出）
- Token 用量統計 / 成本分析 dashboard
- Prompt caching / response caching
- 多租戶獨立 provider 設定（目前全系統共用同組設定）
