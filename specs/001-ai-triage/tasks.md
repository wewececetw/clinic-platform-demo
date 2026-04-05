# Tasks: AI 智慧症狀分流

**Feature**: 001-ai-triage | **Status**: ✅ 全部完成（反向補文檔）
**Source Commits**: dc7f0a3, ab21fc6, 3cdc866, b11c865

## Phase 1: Foundation（LLM 抽象層）

- [x] [T001] 定義 `ILlmClient` 介面與 `LlmMessage`/`LlmRequest`/`LlmResponse` record（`backend/src/ClinicPlatform.Infrastructure/Services/AI/ILlmClient.cs`, `LlmMessage.cs`）
- [x] [T002] 定義 OpenAI 相容回應模型 `OpenAiChatResponse` / `OpenAiChoice` / `OpenAiMessage` / `OpenAiUsage`（共用於 OMLX 與 Groq）

## Phase 2: Provider 實作

- [x] [T003] [P] 實作 `OmlxLlmClient`：從 `IConfiguration` 讀取 endpoint/apiKey/model，送 OpenAI 相容 POST 請求
- [x] [T004] [P] 實作 `GroqLlmClient`：hardcode Groq endpoint、從 config 讀 apiKey/model、Bearer token 認證
- [x] [T005] DI 註冊兩個 provider 為 `IEnumerable<ILlmClient>`（Program.cs / DependencyInjection）

## Phase 3: 分流業務邏輯（US1 + US2）

- [x] [T006] [US1] 建立 `TriageRequest` / `TriageResponse` DTO 於 Application 層 `Features/AI/`
- [x] [T007] [US1] 建立 `IAiService` 介面含 `TriageAsync(TriageRequest)`
- [x] [T008] [US1] 實作 `TriagePromptBuilder.BuildSystemPrompt(departments)`：動態注入科別清單、定義 JSON 回應格式、標示「不做醫療診斷」
- [x] [T009] [US1] 實作 `AiService.TriageAsync`：查詢 DB 科別清單、組 prompt、依 `AI:Provider` 排序 clients
- [x] [T010] [US2] 實作 fallback 迴圈：`foreach (var client in orderedClients)` 嘗試呼叫，失敗則繼續
- [x] [T011] [US2] 全部失敗時回傳 `Result.Fail("AI 分流服務暫時無法使用，請手動選擇科別")`
- [x] [T012] [US2] 加上完整 log：嘗試使用哪個 provider、解析失敗、呼叫失敗

## Phase 4: 解析容錯（US3）

- [x] [T013] [US3] 實作 `ExtractJson(content, requiredKey)`：從後往前掃描、跳過 `<think>` 區塊、找含指定 key 的 JSON 物件
- [x] [T014] [US3] 實作 `ParseTriageResponse`：解析 JSON、匹配 `departmentId`、fallback 以科別名稱比對

## Phase 5: API 端點

- [x] [T015] 建立 `AiController` 與 `POST /api/ai/triage` 端點
- [x] [T016] 將 `Result<TriageResponse>` 轉為 HTTP 回應（成功 200 + data / 失敗 200 + error message）

## Phase 6: 效能優化

- [x] [T017] 從 Qwen3.5-9B 思考模型切換至 **Qwen2.5-7B-Instruct 非思考模型**（commit 3cdc866），回應從 20 秒降至 2-3 秒
- [x] [T018] 調整 `max_tokens=256`、`temperature=0.2`，壓低發散性

---

## 任務依賴圖

```
T001, T002 (Foundation)
   ↓
T003, T004 [P] (Providers)
   ↓
T005 (DI)
   ↓
T006, T007 → T008 → T009 → T010 → T011 → T012 (Triage 主流程)
                              ↓
                         T013 → T014 (解析容錯)
                              ↓
                         T015 → T016 (API)
                              ↓
                         T017, T018 (效能)
```

## 未來改善（非本 Phase）

- [ ] 單元測試：`ExtractJson` 各種混亂輸入情境
- [ ] 單元測試：`AiService.TriageAsync` 的 provider fallback 流程（用 mock `ILlmClient`）
- [ ] 整合測試：實際打 OMLX endpoint 驗證端到端
- [ ] AI 呼叫日誌落地（新 table `ai_request_logs`）
- [ ] Token 用量統計 dashboard
