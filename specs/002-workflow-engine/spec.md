# Feature Specification: 條件式 Workflow Engine

**Feature Branch**: `002-workflow-engine`
**Created**: 2026-04-05
**Status**: Completed（反向補文檔）
**Input**: 門診流程（報到 → 叫號 → 看診 → 繳費/領藥 → 完成）以有向圖建模，允許每個院所自訂步驟與轉移條件。系統根據 Visit 當前狀態與轉移條件自動推進至下一步，支援「不需拿藥則跳過藥劑環節」等條件分支。

## User Scenarios & Testing

### User Story 1 - 依固定路徑推進門診流程（Priority: P1）

當護理師或醫師完成一個步驟（例如叫號、看診完成），系統自動將 Visit 推進到下一個步驟，並記錄 VisitEvent 事件軌跡。

**Why this priority**：這是整個平台的核心業務流程。沒有這個引擎，所有 Service 都要各自寫一堆 if/else 判斷當前狀態，重複且易錯。

**Independent Test**：建立 Visit，呼叫 `WorkflowEngine.AdvanceAsync(clinicId, visitId, userId)`，驗證 `Visit.CurrentStepId` 已更新為下一步、`VisitEvents` 多一筆紀錄。

**Acceptance Scenarios**：

1. **Given** Visit 當前步驟為 `checked_in`、workflow 定義 `checked_in → called` 的 transition，**When** 呼叫 `AdvanceAsync`，**Then** `CurrentStepId` 變為 `called`，新增 VisitEvent。
2. **Given** Visit 目前在最終步驟（無後續 transition），**When** 呼叫 `AdvanceAsync`，**Then** 回傳 `Result.Fail("目前步驟無可用轉移路線")`。

---

### User Story 2 - 條件分支（不需拿藥跳過藥劑）（Priority: P1）

醫師看診完成時標記「不需拿藥」，Engine 根據 transition 上的條件 JSON 判斷，從「完診」直接跳到「完成」，跳過「藥劑調劑」步驟。

**Why this priority**：條件式路由是這個 Engine 的差異化能力，單純線性流程用 enum 就夠了。

**Independent Test**：建立兩條從 `consult_done` 出發的 transition，一條條件 `visit.needs_medication=false` 直達 `completed`，另一條無條件至 `pharmacy`；設定 Visit `NeedsMedication=false`，呼叫 `AdvanceAsync`，驗證直接跳至 `completed`。

**Acceptance Scenarios**：

1. **Given** transition 條件 `{"skip_when":{"field":"visit.needs_medication","operator":"eq","value":false}}`、Visit 的 `NeedsMedication=false`，**When** 推進，**Then** 走該條件 transition。
2. **Given** 條件不符且存在無條件 fallback transition，**When** 推進，**Then** 走 fallback transition。
3. **Given** 多條有條件 transition 皆不符，**When** 推進，**Then** 回傳 `Result.Fail("無符合條件的轉移路線")`。

---

### User Story 3 - Priority 排序與自動遞迴推進（Priority: P2）

Transition 依 `Priority` 降序評估，優先度高的先被嘗試。若推進後的新步驟標記 `AutoAdvance=true`，Engine 自動繼續推進，直到遇到需人工介入的步驟為止。

**Why this priority**：實務上有些步驟（如「寫入 queue」）純系統操作無需等人介入，需要連續推進。

**Independent Test**：建立 `queued → auto_assign_room → called` 的流程，`auto_assign_room.AutoAdvance=true`，從 `queued` 呼叫 `AdvanceAsync` 一次，驗證最終到達 `called`。

**Acceptance Scenarios**：

1. **Given** 兩條 transition priority 分別為 10 和 5，**When** 推進，**Then** priority=10 的先被評估。
2. **Given** `ToStep.AutoAdvance=true`，**When** 推進至該步驟，**Then** Engine 遞迴呼叫 `AdvanceAsync` 繼續推進。

---

### User Story 4 - 事件軌跡與觸發來源記錄（Priority: P2）

每次狀態轉移都新增一筆 VisitEvent，記錄 `FromStepId`、`ToStepId`、`TriggeredByUserId`、`TriggerType`（Manual/System）、`CreatedAt`，供後續查詢病患動線與稽核。

**Why this priority**：醫療場景需稽核軌跡，但主流程不依賴事件存在也能運作。

**Acceptance Scenarios**：

1. **Given** 護理師叫號觸發推進，**When** Engine 執行，**Then** VisitEvent 的 `TriggerType=Manual`、`TriggeredByUserId` 為該護理師。
2. **Given** `AutoAdvance` 觸發的推進，**When** Engine 執行，**Then** VisitEvent 的 `TriggerType=System`。

---

## Requirements

### Functional Requirements

- **FR-001**：系統必須以有向圖結構儲存 workflow：`WorkflowDefinitions` → `WorkflowSteps` → `WorkflowTransitions`。
- **FR-002**：每個 `WorkflowTransition` 必須含 `FromStepId`、`ToStepId`、`Priority`、`ConditionJson`（可為 null）。
- **FR-003**：Engine 必須依 `Priority` 降序評估 transition。
- **FR-004**：`ConditionJson` 為空時，該 transition 視為無條件 fallback，在所有有條件 transition 都不符時才使用。
- **FR-005**：條件 JSON 格式為 `{"skip_when":{"field":"<欄位路徑>","operator":"eq|neq","value":<值>}}`。
- **FR-006**：Engine 必須支援 `visit.needs_medication`、`visit.status` 兩個欄位路徑的條件評估。
- **FR-007**：推進後若新步驟的 `AutoAdvance=true`，Engine 必須遞迴呼叫自己繼續推進。
- **FR-008**：推進至 `StepCode="completed"` 的步驟時，必須同時更新 `Visit.Status=Completed` 與 `Visit.CompletedAt`。
- **FR-009**：每次推進必須新增一筆 `VisitEvent` 記錄來源與時間。
- **FR-010**：`TriggeredByUserId` 為 null 時 `TriggerType` 為 `System`，否則為 `Manual`。
- **FR-011**：條件 JSON 解析失敗必須回傳 false（視為條件不符），不得中斷流程。

### Non-Functional Requirements

- **NFR-001**：單次 `AdvanceAsync` 的資料庫查詢次數須 ≤ 3（Visit 查詢、Transitions 查詢、SaveChanges）。
- **NFR-002**：條件評估為純記憶體運算，不得額外查詢資料庫。
- **NFR-003**：遞迴推進須有防無限迴圈機制（此版本依賴 workflow 設計者避免環路）。

### Key Entities

- **WorkflowDefinition**：流程定義，屬於特定 `ClinicId`，可有多版本。
- **WorkflowStep**：流程步驟節點，有 `StepCode`（如 `checked_in`, `called`, `consulting`, `completed`）、`AutoAdvance` 旗標。
- **WorkflowTransition**：步驟間的有向邊，有 `FromStepId`、`ToStepId`、`Priority`、`ConditionJson`。
- **Visit**：病患就診紀錄，帶 `WorkflowDefinitionId`、`CurrentStepId`、業務旗標（如 `NeedsMedication`）。
- **VisitEvent**：狀態轉移事件軌跡。

## Success Criteria

- **SC-001**：標準門診流程（6 步驟）從報到到完成可在 5 次推進呼叫內完成。
- **SC-002**：單次推進 P95 延遲 ≤ 50ms（不含跨服務呼叫）。
- **SC-003**：每個 transition 評估結果可從 VisitEvent 推算，稽核可 100% 重建病患動線。

## Out of Scope

- **不做**視覺化 workflow 編輯器（定義透過 seed data / admin API 維護）。
- **不做**複合條件（AND/OR/NOT 組合），目前僅支援單一條件 eq/neq。
- **不做**欄位路徑動態表達式引擎（欄位白名單 hardcode 在 `GetFieldValue`）。
- **不做**環路偵測（依賴 workflow 設計者自律）。
- **不做**並行分支（一個 Visit 同一時間只在一個步驟）。
