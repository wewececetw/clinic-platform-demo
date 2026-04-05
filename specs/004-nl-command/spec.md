# Feature Specification: AI 自然語言指令

**Feature Branch**: `004-nl-command`
**Created**: 2026-04-05
**Status**: Completed（反向補文檔）
**Input**: 護理師、醫師、管理員可用自然語言（如「叫下一位」「開普拿疼 500mg TID 三天份」）下指令給系統，由 LLM 解析為結構化 action + params，經角色權限驗證後路由到對應的 CommandExecutor 執行。

## User Scenarios & Testing

### User Story 1 - 護理師語音／文字叫號（Priority: P1）

護理師在叫號介面輸入「叫下一位」或「叫下一個病人」，系統解析為 `action=call_next`、`needsConfirm=true`，護理師確認後執行叫號並回傳結果訊息。

**Why this priority**：這是 Phase 2 的核心價值——讓醫護人員用自然語言操作系統，降低介面學習成本。護理師叫號是最高頻動作。

**Independent Test**：呼叫 `POST /api/ai/command` 帶 `{role:"Nurse", command:"叫下一位"}`，驗證回應含 `action="call_next"`、`needsConfirm=true`、`message` 有繁中說明。

**Acceptance Scenarios**：

1. **Given** 護理師角色，**When** 輸入「叫下一位」，**Then** 解析為 `{action:"call_next", params:{queueType:"waiting"}, result:"confirm"}`。
2. **Given** 護理師角色，**When** 輸入「3 號過號」，**Then** 解析為 `{action:"skip", params:{queueNumber:3}, result:"confirm"}`。
3. **Given** 護理師角色，**When** 輸入「目前有幾個在等」，**Then** 解析為 `{action:"query_queue", result:"done"}`（查詢類不需確認）。

---

### User Story 2 - 醫師開立處方（Priority: P1）

醫師在看診介面說「開普拿疼 500mg TID 三天份」，系統解析為 `create_prescription` action 並擷取 `drugName`、`dosage`、`frequency`、`days` 四個參數，確認後建立處方紀錄。

**Why this priority**：處方是醫師最常用功能之一，且參數結構化最有價值。

**Independent Test**：呼叫 `POST /api/ai/command` 帶 `{role:"Doctor", command:"開普拿疼 500mg TID 三天份"}`，驗證回應的 `params` 含所有四個欄位且數值正確。

**Acceptance Scenarios**：

1. **Given** 醫師輸入「開普拿疼 500mg TID 三天份」，**When** 解析，**Then** `params={drugName:"普拿疼", dosage:"500mg", frequency:"TID", days:3}`。
2. **Given** 醫師輸入「完診，需要拿藥」，**When** 解析，**Then** `action="complete_consult"`、`params={needsMedication:true}`。

---

### User Story 3 - 角色權限過濾（Priority: P1）

system prompt 根據使用者角色（Nurse/Doctor/Admin）**只暴露該角色可執行的 action 清單**給 LLM；同時 `CommandRouter` 在實際執行前再次檢查 `executor.AllowedRoles`，實作雙層防護。

**Why this priority**：安全性不可妥協。LLM 雖有 prompt 限制但不保證，Router 層必須再驗證。

**Independent Test**：用 `role="Nurse"` 呼叫 command 並傳「開處方」類指令，驗證 Router 回 `"您沒有權限執行此操作"`，即使 LLM 解析出了該 action。

**Acceptance Scenarios**：

1. **Given** `role="Nurse"`，**When** system prompt 建立，**Then** `action 清單` 僅含 `call_next, skip, query_queue, query_stats`。
2. **Given** LLM 回傳 `action="create_prescription"` 但 caller 是 Nurse，**When** `CommandRouter.RouteAsync`，**Then** 拒絕並回 `"您沒有權限執行此操作"`。
3. **Given** 未知 action（如 `action="delete_all"`），**When** Router 查不到對應 executor，**Then** 回 `"不支援的指令：delete_all"`。

---

### User Story 4 - Executor 動態註冊與 Action Routing（Priority: P2）

系統以 `ICommandExecutor` 介面定義 action 執行者，DI 註冊多個 executor（`CallNextExecutor`, `SkipExecutor`, `CreatePrescriptionExecutor`...），`CommandRouter` 啟動時以 `executor.Action` 為 key 建立 dictionary 路由。

**Why this priority**：擴充性保證，未來新增 action 只需新建 executor class 並註冊。

**Acceptance Scenarios**：

1. **Given** 新增 `QueryStatsExecutor` 並 DI 註冊，**When** 系統啟動，**Then** `_executorMap["query_stats"]` 自動包含該 executor。
2. **Given** executor 執行中拋例外，**When** Router catch，**Then** 回 `"指令執行失敗，請稍後再試"` 並 log error。

---

### User Story 5 - `needsConfirm` 二階段執行（Priority: P2）

會改變系統狀態的操作（call_next, skip, create_prescription）`needsConfirm=true`，前端先顯示確認對話框；查詢類（query_queue, query_stats）`needsConfirm=false` 直接執行。

**Why this priority**：避免 LLM 誤解導致錯誤操作。

**Acceptance Scenarios**：

1. **Given** action 為 `query_queue`，**When** AiService 包裝回應，**Then** `result="done"`。
2. **Given** action 為 `create_prescription`，**When** AiService 包裝回應，**Then** `result="confirm"`。

---

## Requirements

### Functional Requirements

- **FR-001**：系統必須提供 `POST /api/ai/command` API，接收 `{role:string, command:string}`。
- **FR-002**：`CommandPromptBuilder.BuildSystemPrompt(role)` 必須根據角色動態產生該角色可執行的 action 清單。
- **FR-003**：system prompt 必須含每個 action 的 `params` JSON 格式說明。
- **FR-004**：LLM 回應必須為單一 JSON 物件，含 `action`, `params`, `message`, `needsConfirm` 四欄位。
- **FR-005**：AiService 必須將 `needsConfirm=true` 映射為 `result="confirm"`、false 映射為 `result="done"`。
- **FR-006**：`CommandRouter` 必須以 `executor.Action` 為 key 建立路由 dictionary。
- **FR-007**：執行前必須驗證 `context.Role in executor.AllowedRoles`，否則拒絕。
- **FR-008**：未知 action 必須回 `"不支援的指令：{action}"`。
- **FR-009**：executor 執行例外必須 catch 並回 `"指令執行失敗，請稍後再試"`，不得暴露 stack trace 給前端。
- **FR-010**：所有權限拒絕與未知 action 必須 log warning；例外必須 log error。
- **FR-011**：指令解析必須複用 Feature 001 的 `ILlmClient` 與 fallback 機制。

### Non-Functional Requirements

- **NFR-001**：指令解析 P95 ≤ 3 秒（OMLX 本地）。
- **NFR-002**：temperature=0.1（較分流更低），確保 action 解析穩定。
- **NFR-003**：max_tokens=256，強制精簡回應。
- **NFR-004**：角色權限 dictionary 為 hardcode，避免 runtime 配置篡改風險。

### Key Entities

- **CommandRequest**：`{Role: string, Command: string}`。
- **CommandResponse**：`{Action: string, Params: Dictionary<string,object>?, Result: string, Message: string}`。
- **CommandContext**：Router 接收的上下文（Role, Action, Params, UserId, ClinicId）。
- **ICommandExecutor**：`Action`（string）、`AllowedRoles`（string[]）、`ExecuteAsync(context)`。
- **CommandExecutionResult**：`{Success, Message, Data?}`。

## Success Criteria

- **SC-001**：20 組常見自然語言指令解析 action 正確率 ≥ 90%。
- **SC-002**：角色越權嘗試 100% 被 Router 阻擋（即使 LLM 解析出了該 action）。
- **SC-003**：指令解析 P95 ≤ 3 秒（本地 OMLX）。
- **SC-004**：新增 action 僅需新增 executor class + DI 註冊，不需改 Router 程式碼。

## Out of Scope

- **不做** 語音輸入（Web Speech API 為 Phase 5）。
- **不做** 多輪對話上下文（每次 command 獨立解析）。
- **不做** 指令執行歷史記錄（僅 log，不落 DB）。
- **不做** LLM Function Calling 原生格式（採自訂 JSON 格式以相容 OMLX/Groq）。
- **不做** 指令建議 / 自動完成。
