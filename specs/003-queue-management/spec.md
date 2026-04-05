# Feature Specification: 候診佇列管理

**Feature Branch**: `003-queue-management`
**Created**: 2026-04-05
**Status**: Completed（反向補文檔）
**Input**: 管理病患候診佇列：報到後加入佇列、護理師叫號、查詢候診位置、跳過（過號）。佇列採優先度排序（緊急 > 優先 > 一般），同優先度內依報到時間排序。

## User Scenarios & Testing

### User Story 1 - 查詢目前候診佇列（Priority: P1）

護理師在候診管理介面查看某佇列類型（waiting/pharmacy）的所有等待病患，依優先度降序、報到時間升序顯示，含病患姓名、號碼牌、報到時間。

**Why this priority**：候診管理的基本能力，沒這個無法進行任何叫號操作。

**Independent Test**：插入多筆不同優先度的 QueueEntry，呼叫 `GET /api/queue/{clinicId}/{queueType}`，驗證回傳順序正確。

**Acceptance Scenarios**：

1. **Given** 佇列有 3 筆 priority=0、2 筆 priority=1 的 waiting entries，**When** 查詢，**Then** priority=1 的 2 筆排在前面。
2. **Given** 兩筆相同 priority=0，**When** 查詢，**Then** 先報到的排前面。
3. **Given** 佇列類型字串為 `"Pharmacy"` 或 `"pharmacy"`，**When** 查詢，**Then** 皆能正確解析為 enum。
4. **Given** 佇列類型字串為 `"invalid"`，**When** 查詢，**Then** 回 `Result.Fail("無效的佇列類型")`。

---

### User Story 2 - 叫下一位病患（Priority: P1）

護理師按下「叫下一位」，系統取出佇列中優先度最高、報到最早的病患，將 QueueEntry 狀態改為 `Called`、更新 Visit 的 `CurrentStepId` 至 `called` 步驟，可選擇指定診間 `RoomId`。同時記錄 VisitEvent。

**Why this priority**：整個候診流程的核心動作。

**Independent Test**：佇列中有 3 筆 waiting，呼叫 `POST /api/queue/call-next`，驗證優先度最高那筆的 `Status=Called`、`CalledAt` 有值、Visit 的 `CurrentStepId` 更新、VisitEvent 新增一筆。

**Acceptance Scenarios**：

1. **Given** 佇列空，**When** 呼叫 `CallNextAsync`，**Then** 回 `Result.Fail("目前佇列中沒有等待的病患")`。
2. **Given** 佇列有等待病患且指定 `RoomId`，**When** 叫號，**Then** Visit 的 `RoomId` 也同步更新。
3. **Given** 無 `called` 步驟（workflow 未定義），**When** 叫號，**Then** 不更新 Visit.CurrentStepId 但佇列狀態仍更新。

---

### User Story 3 - 查詢候診位置（Priority: P2）

病患在前端輸入號碼牌查詢自己目前在佇列中的位置（第幾位）與總等待人數，用於顯示「前面還有 X 人」的提示。

**Why this priority**：病患體驗加分項，不影響核心流程運作。

**Independent Test**：插入 5 筆 waiting，呼叫 `GetPositionAsync` 查詢中間那筆，驗證 `Position=3`。

**Acceptance Scenarios**：

1. **Given** 該 visit 前有 2 筆更高優先度 + 1 筆同優先度但更早報到，**When** 查詢位置，**Then** `Position=4`（含自己）。
2. **Given** visitId 不存在於佇列，**When** 查詢，**Then** 回 `Result.Fail("找不到該候診紀錄")`。

---

### User Story 4 - 過號（Skip）（Priority: P2）

病患未到場時，護理師可標記過號，該筆 QueueEntry 狀態從 `Waiting` 改為 `Skipped`，從候診清單移除。

**Why this priority**：處理缺席情境必要但非每次都用。

**Acceptance Scenarios**：

1. **Given** QueueEntry.Status=Waiting，**When** 呼叫 `SkipAsync`，**Then** Status=Skipped、SkippedAt 有值。
2. **Given** QueueEntry.Status=Called（已叫號），**When** 呼叫 `SkipAsync`，**Then** 回 `Result.Fail("只能跳過等待中的候診紀錄")`。

---

## Requirements

### Functional Requirements

- **FR-001**：QueueEntry 必須以 `(ClinicId, QueueType, Priority DESC, CreatedAt ASC)` 排序。
- **FR-002**：佇列支援至少兩種類型：`Waiting`（看診候診）、`Pharmacy`（領藥候診）。
- **FR-003**：`QueueType` 字串解析必須忽略大小寫。
- **FR-004**：優先度定義：0=一般、1=優先、2=緊急，緊急優先叫號。
- **FR-005**：`CallNextAsync` 必須自動更新對應 Visit 的 `CurrentStepId` 至 workflow 中 `StepCode="called"` 的步驟（若存在）。
- **FR-006**：`CallNextAsync` 可選擇性指定 `RoomId`，有指定則更新 Visit.RoomId。
- **FR-007**：叫號操作必須同步寫入 VisitEvent（Notes="叫號"、TriggerType=Manual）。
- **FR-008**：候診位置計算必須排除非 Waiting 狀態的 entry。
- **FR-009**：`SkipAsync` 僅允許對 `Status=Waiting` 的 entry 操作。

### Non-Functional Requirements

- **NFR-001**：查詢佇列 P95 延遲 ≤ 100ms（單院所典型候診數 10-50 人）。
- **NFR-002**：叫號操作須保證 QueueEntry + Visit + VisitEvent 三者原子更新（同一 `SaveChangesAsync`）。
- **NFR-003**：多個護理師同時叫號時，使用資料庫樂觀鎖避免重複叫同一人（未來優化項）。

### Key Entities

- **QueueEntry**：候診條目，含 `VisitId`、`QueueType`、`QueueNumber`、`Priority`、`Status`、`CreatedAt/CalledAt/SkippedAt`。
- **QueueEntryStatus**：enum（`Waiting`, `Called`, `Skipped`, `Completed`）。
- **QueueType**：enum（`Waiting`, `Pharmacy`）。
- **QueueEntryDto** / **QueuePositionDto** / **CallNextRequest**：API 傳輸物件。

## Success Criteria

- **SC-001**：100 筆候診佇列查詢 P95 ≤ 100ms。
- **SC-002**：叫號併發衝突率 0（同一人不得被叫兩次，依賴 DB transaction）。
- **SC-003**：位置計算準確率 100%（位置數 = 真實在前的人數 + 1）。

## Out of Scope

- **不做** Redis 快取層（原規劃但本 Phase 僅以 MySQL 實作；Redis 為未來優化項）。
- **不做** SignalR 即時推播號碼變化（另屬功能）。
- **不做** 跨院所合併佇列。
- **不做** 動態優先度調整（報到後 priority 不變，除非手動改）。
- **不做** 叫號重試機制（3 次未到自動過號等規則）。
