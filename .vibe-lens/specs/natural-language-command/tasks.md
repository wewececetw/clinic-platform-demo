# Tasks: natural-language-command

> Methodology: Spec-Driven Development | Tool: Vibe Lens
> Inspired by [GitHub Spec Kit](https://github.com/github/spec-kit)

**Created**: 2026-03-31
**Plan**: specs/natural-language-command/plan.md

---

## Phase 1: Domain & Application 基礎

- [x] [T001] CommandAction enum 定義 6 種 action + Unknown
- [x] [T002] CommandPromptBuilder 依角色建構 system prompt
- [x] [T003] AiService.CommandAsync 串接 LLM + JSON 解析

**Checkpoint**: LLM 解析層可獨立運作，給定自然語言回傳結構化 JSON。

## Phase 2: Action Router & Executor 介面

- [x] [T004] 建立 ICommandExecutor 介面（CanHandle, ExecuteAsync）
- [x] [T005] 建立 CommandRouter（注入 IEnumerable<ICommandExecutor>，依 action 路由 + 角色二次驗證）
- [x] [T006] DI 註冊 CommandRouter 與所有 Executor

**Checkpoint**: Router 骨架就位，可接收 action 並路由到 executor。 ✅

## Phase 3: US1 — 護理師叫號

- [x] [T007] [US1] 實作 CallNextExecutor（對接 QueueService）
- [x] [T008] [US1] WebAPI POST /api/ai/command/execute 端點
- [x] [T009] [US1] [P] 前端 CommandInput.vue 元件（輸入框 + 確認對話框）
- [x] [T010] [US1] [P] 前端 api/ai.ts（commandAsync, executeCommandAsync）
- [x] [T011] [US1] NurseDashboard 嵌入 CommandInput

**Checkpoint**: 護理師可用「叫下一位」完成端到端叫號流程。 ✅

## Phase 4: US2 — 護理師過號

- [x] [T012] [US2] 實作 SkipExecutor（對接 QueueService）
- [x] [T013] [US2] 前端處理 skip action 的確認流程（CommandInput 統一處理）

**Checkpoint**: 護理師可用「3號過號」完成過號。 ✅

## Phase 5: US3 — 查詢候診

- [x] [T014] [US3] [P] 實作 QueryQueueExecutor（對接 QueueService）
- [x] [T015] [US3] [P] 前端處理查詢類結果直接顯示（CommandInput 依 needsConfirm 判斷）

**Checkpoint**: 護理師可用「還有幾位候診」查詢即時狀態。 ✅

## Phase 6: US5 — 醫師完成看診

- [x] [T016] [US5] 實作 CompleteConsultExecutor（對接 VisitService）
- [x] [T017] [US5] DoctorConsult 嵌入 CommandInput

**Checkpoint**: 醫師可用「完成看診，需要拿藥」推進工作流程。 ✅

## Phase 7: US6 — 管理員統計查詢

- [x] [T018] [US6] [P] 實作 QueryStatsExecutor
- [x] [T019] [US6] [P] AdminDashboard 嵌入 CommandInput

**Checkpoint**: 管理員可用自然語言查詢今日統計。 ✅

## Phase 8: US4 — 醫師開處方（依賴藥品資料表）

- [ ] [T020] [US4] 確認藥品資料表結構（Drug entity）
- [ ] [T021] [US4] 實作 CreatePrescriptionExecutor（藥品名稱模糊匹配 + 處方建立）
- [ ] [T022] [US4] CommandPromptBuilder 加入藥品相關 params 說明

**Checkpoint**: 醫師可用「開普拿疼 500mg TID 三天份」建立處方。

## Phase 9: Polish

- [ ] [T023] 整合測試：各角色端到端指令流程
- [ ] [T024] 邊界案例處理（無意義輸入、無權限 action、LLM 回傳格式錯誤）
- [ ] [T025] CommandAction enum 補上 CreatePrescription

---

**Legend**:
- `[T###]` — Task ID (sequential)
- `[P]` — Can run in parallel with adjacent tasks
- `[US#]` — User story reference
- `[x]` — 已完成（對應現有程式碼）
