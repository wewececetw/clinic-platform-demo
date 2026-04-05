# Implementation Plan: 候診佇列管理

**Branch**: `003-queue-management` | **Date**: 2026-04-05 | **Spec**: [spec.md](./spec.md)

**Note**: 反向補文檔，對應 commit e2d79df。實際實作目前**僅使用 MySQL**，CLAUDE.md 中提到的「Redis 為主、MySQL 為持久化備份」屬原始架構設計，尚未實作 Redis 層。

## Summary

以 `QueueEntry` 表儲存候診條目，對 `(ClinicId, QueueType, Status=Waiting)` 條件查詢並依 `(Priority DESC, CreatedAt ASC)` 排序。`QueueService` 提供四個操作：查詢佇列、查詢位置、叫下一位、過號。叫號操作整合 WorkflowEngine 的 `called` 步驟定位，在同一 `SaveChangesAsync` 中更新 QueueEntry + Visit + VisitEvent 三者。

## Technical Context

**Language/Version**：C# 13 / .NET 10
**Primary Dependencies**：EF Core 10（`Include`、`ThenInclude` 載入關聯）
**Storage**：MySQL 8（`queue_entries` 表）
**Testing**：xUnit（整合測試，EF Core InMemory 或 SQLite）
**Project Type**：Web service
**Performance Goals**：查詢 P95 ≤ 100ms、叫號操作 ≤ 200ms
**Constraints**：候診數量典型 10-50 人、單診所 QPS < 10

## Constitution Check

- ✅ **繁體中文**：錯誤訊息、DTO 的 `FullName` fallback `"匿名"` 全繁中
- ✅ **Clean Architecture**：`IQueueService` 介面在 Application 層、實作在 Infrastructure 層
- ✅ **原子性**：QueueEntry + Visit + VisitEvent 同交易

## Project Structure

```text
backend/src/
├── ClinicPlatform.Domain/Entities/
│   └── QueueEntry.cs
├── ClinicPlatform.Application/Features/Queue/
│   ├── IQueueService.cs
│   ├── QueueEntryDto.cs
│   ├── QueuePositionDto.cs
│   └── CallNextRequest.cs
├── ClinicPlatform.Infrastructure/Services/
│   └── QueueService.cs
└── ClinicPlatform.WebAPI/Controllers/
    └── QueueController.cs
```

## Data Model

### queue_entries
| 欄位 | 型別 | 說明 | 索引 |
|------|------|------|------|
| Id | Guid | PK | |
| ClinicId | Guid | FK | ✓（複合）|
| VisitId | Guid | FK | ✓ |
| QueueType | enum | Waiting/Pharmacy | ✓（複合）|
| QueueNumber | int | 顯示用號碼牌 | |
| Priority | int | 0/1/2 | ✓（複合）|
| Status | enum | Waiting/Called/Skipped/Completed | ✓（複合）|
| CreatedAt | datetime | 報到時間 | ✓（複合）|
| CalledAt | datetime? | 叫號時間 | |
| SkippedAt | datetime? | 過號時間 | |

**建議複合索引**：`(ClinicId, QueueType, Status, Priority DESC, CreatedAt ASC)` 覆蓋主查詢。

## Operations

### 1. `GetQueueAsync(clinicId, queueType)`

```
SELECT entries
WHERE ClinicId=? AND QueueType=? AND Status=Waiting
ORDER BY Priority DESC, CreatedAt ASC
INCLUDE Visit.Patient
```

### 2. `GetPositionAsync(clinicId, visitId)`

```
entry = SELECT WHERE VisitId=?
position = COUNT WHERE (Priority > entry.Priority
                        OR (Priority = entry.Priority AND CreatedAt < entry.CreatedAt))
return position + 1
```

### 3. `CallNextAsync(request, callerUserId)`

```
entry = SELECT TOP 1 WHERE Status=Waiting
             ORDER BY Priority DESC, CreatedAt ASC
entry.Status = Called
entry.CalledAt = UtcNow

visit.CurrentStepId = workflow_steps WHERE StepCode="called"
if (request.RoomId) visit.RoomId = request.RoomId

INSERT VisitEvent (FromStepId, ToStepId, Manual, callerUserId, "叫號")
SaveChanges  -- 三者同交易
```

### 4. `SkipAsync(clinicId, visitId, callerUserId)`

```
entry = SELECT WHERE VisitId=?
if (entry.Status != Waiting) Fail
entry.Status = Skipped
entry.SkippedAt = UtcNow
SaveChanges
```

## 關鍵設計決策

| 決策 | 理由 |
|------|------|
| 以 MySQL 為主，不用 Redis | MVP 階段規模小（10-50 人），MySQL 已足夠；Redis 列為未來優化項 |
| `Enum.TryParse` 忽略大小寫 | 前端傳 `"waiting"` 或 `"Waiting"` 都能接受 |
| 叫號同步更新 Visit.CurrentStepId | 減少前端額外呼叫；避免兩者狀態不同步 |
| 位置計算用 `COUNT` 而非 row number | 簡單易懂、EF Core 可翻譯成 SQL |
| 過號檢查 `Status=Waiting` | 避免重複過號、保護狀態機 |
| DTO 的 `FullName ?? "匿名"` | 保護隱私並處理 Patient 無姓名情境 |

## Out of Scope（本 Phase 不做）

- Redis 快取層（CLAUDE.md 原規劃，未實作）
- 樂觀鎖 / 悲觀鎖防併發叫號
- SignalR 即時號碼推播
- 自動過號機制（報到 > N 分鐘未到）
- 佇列狀態統計 dashboard
