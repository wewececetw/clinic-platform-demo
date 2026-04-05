# Implementation Plan: AI 自然語言指令

**Branch**: `004-nl-command` | **Date**: 2026-04-05 | **Spec**: [spec.md](./spec.md)

**Note**: 反向補文檔，對應 commit 25a23ee。依賴 Feature 001 的 `ILlmClient` 與 fallback 機制。

## Summary

複用 Feature 001 的 LLM 抽象層，新增 `CommandPromptBuilder` 根據角色動態組出 system prompt（暴露該角色可執行的 action 清單與 params 格式）。`AiService.CommandAsync` 呼叫 LLM 取得結構化 JSON 後，由 `CommandRouter` 依 action 名稱路由到對應 `ICommandExecutor`，路由前再次檢查角色權限（雙層防護）。

## Technical Context

**Language/Version**：C# 13 / .NET 10
**Primary Dependencies**：繼承 Feature 001 的 `ILlmClient`、`ICommandExecutor` 抽象
**Storage**：不落 DB（指令不記錄歷史）
**Testing**：xUnit（`CommandPromptBuilder` 對各角色的輸出、`CommandRouter` 的權限檢查）
**Project Type**：Web service
**Performance Goals**：解析 P95 ≤ 3 秒（OMLX 本地）
**Constraints**：`temperature=0.1`、`max_tokens=256`、`needsConfirm` 決定前端是否二次確認

## Constitution Check

- ✅ **繁體中文**：message、action descriptions、error 全繁中
- ✅ **雙層權限**：Prompt 層過濾 + Router 層驗證
- ✅ **不暴露內部錯誤**：executor 例外統一回 `"指令執行失敗"`
- ✅ **Clean Architecture**：`ICommandExecutor` 介面在 Application 層

## Project Structure

```text
backend/src/
├── ClinicPlatform.Application/Features/AI/
│   ├── IAiService.cs                # 擴充 CommandAsync 方法
│   ├── CommandRequest.cs
│   ├── CommandResponse.cs
│   ├── CommandContext.cs
│   ├── ICommandExecutor.cs
│   └── CommandExecutionResult.cs
├── ClinicPlatform.Infrastructure/Services/AI/
│   ├── AiService.cs                 # CommandAsync + ParseCommandResponse
│   ├── CommandPromptBuilder.cs      # 角色權限 + params 格式
│   ├── CommandRouter.cs             # Action → Executor 路由
│   └── Executors/
│       ├── CallNextExecutor.cs
│       ├── SkipExecutor.cs
│       ├── QueryQueueExecutor.cs
│       ├── CompleteConsultExecutor.cs
│       ├── CreatePrescriptionExecutor.cs
│       └── QueryStatsExecutor.cs
└── ClinicPlatform.WebAPI/Controllers/
    └── AiController.cs              # POST /api/ai/command
```

## 角色權限矩陣

| Role | Allowed Actions |
|------|-----------------|
| Nurse | call_next, skip, query_queue, query_stats |
| Doctor | complete_consult, create_prescription, query_queue, query_stats |
| Admin | query_queue, query_stats |

## 呼叫流程

```
Client
  │ POST /api/ai/command { role:"Nurse", command:"叫下一位" }
  ▼
AiController
  │ aiService.CommandAsync(request)
  ▼
AiService.CommandAsync
  │ 1. CommandPromptBuilder.BuildSystemPrompt(role)   // 依角色動態 prompt
  │ 2. 呼叫 ILlmClient (fallback 機制) 取得 JSON
  │ 3. ParseCommandResponse 解析
  │ 4. 包裝 result = needsConfirm ? "confirm" : "done"
  ▼
CommandResponse { action, params, result, message }
  │
  ▼
Client 顯示 confirm 對話框（如需要），用戶確認後：
  │ POST /api/command/execute { action, params }
  ▼
CommandRouter.RouteAsync(context)
  │ 1. _executorMap.TryGetValue(action, out executor)  // dictionary 路由
  │ 2. executor.AllowedRoles.Contains(context.Role)    // 權限雙層驗證
  │ 3. executor.ExecuteAsync(context)
  ▼
CommandExecutionResult { success, message, data }
```

## Params 格式規範

```text
call_next           : {"queueType": "waiting"}
skip                : {"queueNumber": 號碼數字}
query_queue         : {}
complete_consult    : {"needsMedication": true|false}
create_prescription : {"drugName", "dosage": "500mg", "frequency": "QD/BID/TID/QID", "days": 3}
query_stats         : {}
```

## 關鍵設計決策

| 決策 | 理由 |
|------|------|
| Role hardcode 在 `RolePermissions` dictionary | 權限核心不應動態配置，避免被 runtime 篡改 |
| System prompt 只暴露該角色可用 action | 縮小 LLM 選擇空間、提高解析準確度、降低越權風險 |
| Router 層再次驗證權限 | LLM 是黑盒無法保證，應用層必須有 hard check |
| 自訂 JSON 格式而非 LLM Function Calling | OMLX/某些開源模型未完整支援原生 function calling |
| `needsConfirm` 二階段 | 狀態改變型操作前端 confirm，查詢直接 done |
| `_executorMap` 在 ctor 建立（非 request 時） | 避免每次 routing 都重建字典 |
| executor 例外統一訊息 | 避免 stack trace 洩漏到前端 |
| `temperature=0.1`（比分流 0.2 更低） | action 解析不需創意，越確定越好 |

## 安全性考量

1. **雙層權限**：Prompt 層 + Router 層，任一層失守仍有兜底
2. **未知 action 拒絕**：即使 LLM 幻覺出不存在的 action 也不會執行
3. **例外隱藏**：不暴露內部錯誤訊息給前端
4. **不落 DB**：指令歷史不記錄，避免敏感指令（如刪除動作）持久化

## Out of Scope（本 Phase 不做）

- 指令執行歷史表 + 稽核日誌
- Web Speech API 語音輸入（Phase 5）
- 多輪對話上下文
- Function Calling 原生格式支援
- 指令自動補全 / 建議
- Admin 可配置角色權限（目前 hardcode）
