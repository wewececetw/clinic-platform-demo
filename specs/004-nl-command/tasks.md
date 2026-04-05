# Tasks: AI 自然語言指令

**Feature**: 004-nl-command | **Status**: ✅ 全部完成（反向補文檔）
**Source Commits**: 25a23ee
**Depends On**: Feature 001 (AI 智慧症狀分流) — 複用 `ILlmClient` 與 fallback 機制

## Phase 1: Application 層介面

- [x] [T001] [P] 定義 `CommandRequest`（Role, Command）
- [x] [T002] [P] 定義 `CommandResponse`（Action, Params, Result, Message）
- [x] [T003] [P] 定義 `CommandContext`（Role, Action, Params, UserId, ClinicId）
- [x] [T004] [P] 定義 `ICommandExecutor`（Action, AllowedRoles[], ExecuteAsync）
- [x] [T005] [P] 定義 `CommandExecutionResult`（Success, Message, Data）
- [x] [T006] 擴充 `IAiService` 加入 `CommandAsync(CommandRequest)`

## Phase 2: Prompt 工程（US3）

- [x] [T007] [US3] 建立 `CommandPromptBuilder` 與 `RolePermissions` dictionary
- [x] [T008] [US3] 實作 `GetActionDescription(action)` 提供每個 action 的繁中說明
- [x] [T009] [US3] `BuildSystemPrompt(role)`：注入角色名、可執行 action 清單、params 格式文件
- [x] [T010] [US3] 強制 LLM 回應為單一 JSON（action, params, message, needsConfirm）

## Phase 3: LLM 呼叫與解析（US1 + US2）

- [x] [T011] [US1] 實作 `AiService.CommandAsync`：組 prompt、依 `AI:Provider` 排序 clients、fallback 迴圈
- [x] [T012] [US1] 實作 `ParseCommandResponse(content)`：複用 `ExtractJson(content, "action")`
- [x] [T013] [US1] 解析 `params` 時處理 JsonValueKind（String/Number/True/False）
- [x] [T014] [US1] 映射 `needsConfirm` → `result`（"confirm" / "done"）
- [x] [T015] [US1] 全部 provider 失敗時回 `Result.Fail("指令解析失敗，請換個說法再試一次")`
- [x] [T016] [US2] 驗證處方指令的 params 擷取（drugName, dosage, frequency, days）

## Phase 4: Command Router（US3 + US4）

- [x] [T017] [US4] 建立 `CommandRouter` class 注入 `IEnumerable<ICommandExecutor>`
- [x] [T018] [US4] ctor 建立 `_executorMap = executors.ToDictionary(e => e.Action)`
- [x] [T019] [US3] 未知 action 時回 `"不支援的指令：{action}"` 並 log warning
- [x] [T020] [US3] 驗證 `executor.AllowedRoles.Contains(role)`，否則回 `"您沒有權限執行此操作"`
- [x] [T021] [US4] try/catch executor 執行例外，回 `"指令執行失敗，請稍後再試"` 並 log error

## Phase 5: `needsConfirm` 二階段（US5）

- [x] [T022] [US5] 操作類 action（call_next, skip, create_prescription, complete_consult）標示 `needsConfirm=true`
- [x] [T023] [US5] 查詢類 action（query_queue, query_stats）標示 `needsConfirm=false`
- [x] [T024] [US5] 前端依 `result` 決定是否顯示 confirm 對話框

## Phase 6: API 端點

- [x] [T025] 於 `AiController` 新增 `POST /api/ai/command` 端點

---

## 依賴圖

```
Feature 001 (AI 分流) ← 已存在 ILlmClient + fallback
    ↓
T001-T006 (介面/DTOs) [P]
    ↓
T007-T010 (Prompt)
    ↓
T011-T016 (LLM 解析)
    ↓
T017-T021 (Router 路由 + 權限)
    ↓
T022-T024 (二階段確認)
    ↓
T025 (API)
```

## 未來改善（非本 Phase）

- [ ] 實作具體的 6 個 `ICommandExecutor`（CallNextExecutor, SkipExecutor, ... 目前 CommandRouter 架構已就緒但 executor 尚未全部串接）
- [ ] 單元測試：`CommandPromptBuilder` 對各角色的輸出
- [ ] 單元測試：`CommandRouter` 的權限驗證與未知 action 處理
- [ ] 整合測試：端到端自然語言 → 執行流程
- [ ] 指令執行歷史記錄（新 table `command_logs`）
- [ ] Admin UI 可檢視各角色權限矩陣
- [ ] 多輪對話上下文支援（延續前一輪 Context）
