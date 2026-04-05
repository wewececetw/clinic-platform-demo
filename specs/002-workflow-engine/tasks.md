# Tasks: 條件式 Workflow Engine

**Feature**: 002-workflow-engine | **Status**: ✅ 全部完成（反向補文檔）
**Source Commits**: 72a1c3a, e2d79df

## Phase 1: Domain Model

- [x] [T001] [P] 建立 `WorkflowDefinition` entity（Id, ClinicId, Name, Version, IsActive）
- [x] [T002] [P] 建立 `WorkflowStep` entity（Id, StepCode, DisplayName, AutoAdvance）
- [x] [T003] [P] 建立 `WorkflowTransition` entity（FromStepId, ToStepId, Priority, ConditionJson）
- [x] [T004] [P] 建立 `Visit` entity（含 `CurrentStepId`, `NeedsMedication`, `Status`）
- [x] [T005] [P] 建立 `VisitEvent` entity（FromStepId, ToStepId, TriggerType, TriggeredByUserId）
- [x] [T006] [P] 定義 `VisitStatus` / `TriggerType` enum

## Phase 2: Persistence

- [x] [T007] EF Core 設定各實體 PK/FK/索引（`ClinicId` + `VisitId` 複合索引）
- [x] [T008] Migration 建立四張相關資料表
- [x] [T009] Seed Data：建立範例 workflow（6 步驟：checked_in → called → consulting → consult_done → pharmacy → completed）

## Phase 3: Engine 核心（US1 + US2）

- [x] [T010] [US1] 定義 `IWorkflowEngine.AdvanceAsync(clinicId, visitId, triggeredByUserId)` 介面
- [x] [T011] [US1] 實作查詢當前 Visit + `Include(v => v.CurrentStep)`
- [x] [T012] [US1] 實作查詢 outgoing transitions `OrderByDescending(Priority)`
- [x] [T013] [US2] 實作 `EvaluateCondition(conditionJson, visit)`：解析 `skip_when.field/operator/value`
- [x] [T014] [US2] 實作 `GetFieldValue(field, visit)`：hardcode `visit.needs_medication` / `visit.status` 欄位對應
- [x] [T015] [US2] 實作 `ValuesEqual(actual, expected)`：處理 bool/string/number 三種 JsonValueKind
- [x] [T016] [US2] 實作 transition 匹配邏輯：有條件優先、無條件作 fallback
- [x] [T017] [US2] 條件解析失敗時 catch 並回 false

## Phase 4: 遞迴推進與完成處理（US3）

- [x] [T018] [US3] 檢查 `matched.ToStep.AutoAdvance` 並遞迴呼叫 `AdvanceAsync`
- [x] [T019] [US3] 推進至 `StepCode="completed"` 時同步設 `Visit.Status=Completed` 與 `Visit.CompletedAt`

## Phase 5: 事件軌跡（US4）

- [x] [T020] [US4] 新增 `VisitEvent` 記錄 FromStepId/ToStepId/TriggeredByUserId
- [x] [T021] [US4] 依 `triggeredByUserId` 是否為 null 決定 `TriggerType`（Manual/System）
- [x] [T022] [US4] Visit 更新 + VisitEvent 新增在同一 `SaveChangesAsync`

## Phase 6: 整合

- [x] [T023] DI 註冊 `IWorkflowEngine → WorkflowEngine`（Scoped）
- [x] [T024] 在 `QueueService.CallNextAsync` 等呼叫點整合（叫號時手動更新 Visit.CurrentStepId）
- [x] [T025] 在 `VisitService.CompleteConsultAsync` 觸發 `AdvanceAsync`

---

## 依賴圖

```
T001-T006 (Entities) [P]
    ↓
T007, T008 (Persistence)
    ↓
T009 (Seed)
    ↓
T010 → T011 → T012 (查詢)
            ↓
T013 → T014 → T015 → T016 → T017 (條件評估)
                  ↓
T018, T019 (遞迴/完成)
    ↓
T020 → T021 → T022 (事件)
    ↓
T023 → T024, T025 (整合)
```

## 未來改善（非本 Phase）

- [ ] 單元測試：`EvaluateCondition` 各種 operator/value type 組合
- [ ] 單元測試：fallback transition 選擇邏輯
- [ ] 整合測試：完整 6 步驟推進流程
- [ ] 環路偵測（最大遞迴深度限制）
- [ ] 複合條件（AND/OR）支援
- [ ] Workflow 定義匯入/匯出工具
