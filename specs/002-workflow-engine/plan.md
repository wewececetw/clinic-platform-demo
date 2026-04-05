# Implementation Plan: 條件式 Workflow Engine

**Branch**: `002-workflow-engine` | **Date**: 2026-04-05 | **Spec**: [spec.md](./spec.md)

**Note**: 反向補文檔，對應 commits 72a1c3a（初始化）、e2d79df（Service 層）。

## Summary

以有向圖三張表（definitions/steps/transitions）建模門診流程，`WorkflowEngine.AdvanceAsync` 從當前步驟找出所有 outgoing transitions，依 Priority 降序評估 `ConditionJson`，命中則切換 `Visit.CurrentStepId` 並記錄 `VisitEvent`。新步驟的 `AutoAdvance=true` 觸發遞迴推進。條件 JSON 解析走 `System.Text.Json` 純記憶體運算。

## Technical Context

**Language/Version**：C# 13 / .NET 10
**Primary Dependencies**：EF Core 10、`System.Text.Json`
**Storage**：MySQL 8（`workflow_definitions` / `workflow_steps` / `workflow_transitions` / `visits` / `visit_events`）
**Testing**：xUnit（`EvaluateCondition`、`GetFieldValue`、`ValuesEqual` 純函式單元測試）
**Project Type**：Web service（Clean Architecture）
**Performance Goals**：單次推進 P95 ≤ 50ms
**Constraints**：條件評估不可額外查 DB；無環路偵測（依賴設計者）

## Constitution Check

- ✅ **繁體中文**：錯誤訊息全繁中
- ✅ **Clean Architecture**：`IWorkflowEngine` 在 Application 層、實作在 Infrastructure 層
- ✅ **資料庫事務**：Visit 更新 + VisitEvent 新增在同一 `SaveChangesAsync`

## Project Structure

```text
backend/src/
├── ClinicPlatform.Domain/Entities/
│   ├── WorkflowDefinition.cs
│   ├── WorkflowStep.cs
│   ├── WorkflowTransition.cs
│   ├── Visit.cs
│   └── VisitEvent.cs
├── ClinicPlatform.Application/Features/Workflow/
│   └── IWorkflowEngine.cs
└── ClinicPlatform.Infrastructure/Services/
    └── WorkflowEngine.cs
```

## Data Model

### workflow_definitions
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | Guid | PK |
| ClinicId | Guid | FK（multi-tenant）|
| Name | string | 流程名稱 |
| Version | int | 版本 |
| IsActive | bool | 啟用中 |

### workflow_steps
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | Guid | PK |
| WorkflowDefinitionId | Guid | FK |
| StepCode | string | 機器可讀代碼（checked_in / called / consulting / pharmacy / completed）|
| DisplayName | string | 繁中顯示名稱 |
| AutoAdvance | bool | 到達後是否自動推進 |

### workflow_transitions
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | Guid | PK |
| WorkflowDefinitionId | Guid | FK |
| FromStepId | Guid | FK |
| ToStepId | Guid | FK |
| Priority | int | 評估順序（降序）|
| ConditionJson | string? | 條件 JSON，可為 null |

## 條件 JSON 格式

```jsonc
// skip_when：條件成立則「走這條」
{
  "skip_when": {
    "field": "visit.needs_medication",
    "operator": "eq",
    "value": false
  }
}
```

**支援欄位**（hardcode in `GetFieldValue`）：
- `visit.needs_medication` → `Visit.NeedsMedication` (bool)
- `visit.status` → `Visit.Status.ToString()` (enum string)

**支援 operator**：`eq`, `neq`

## 核心演算法

```csharp
// 偽碼
foreach (transition in transitions.OrderByDescending(Priority)) {
    if (transition.ConditionJson is null) {
        matched ??= transition;  // 無條件作為 fallback
        continue;
    }
    if (EvaluateCondition(transition.ConditionJson, visit)) {
        matched = transition;
        break;  // 命中即停
    }
}
if (matched is null) return Fail("無符合條件的轉移路線");

// 更新 Visit + 寫 VisitEvent + SaveChanges
// 若 ToStep.AutoAdvance 則遞迴 AdvanceAsync
```

## 關鍵設計決策

| 決策 | 理由 |
|------|------|
| 用 JSON 儲存條件而非 C# code | 免程式碼變更即可調整流程、支援 runtime 動態定義 |
| `skip_when` 語意 = 「走這條」 | 對應醫療場景「XX 條件成立則跳過後續」的直覺 |
| 欄位路徑白名單 hardcode | 避免反射的效能與安全風險；新欄位顯式 review |
| 無條件 transition 作 fallback 而非預設 | 強制設計者明確定義「else 分支」 |
| 遞迴推進而非迴圈 | 程式碼簡潔、stack 深度 ≤ workflow 步驟數 |
| `SaveChanges` 一次寫入 Visit + VisitEvent | 保證事件軌跡與狀態一致性 |

## Out of Scope（本 Phase 不做）

- 複合條件運算子（AND/OR/NOT）
- 欄位路徑動態表達式引擎（如 CEL / JSONPath）
- 環路偵測與最大推進步數限制
- 並行分支（split / join）
- Workflow 版本遷移工具
