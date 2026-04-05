# Tasks: 候診佇列管理

**Feature**: 003-queue-management | **Status**: ✅ 全部完成（反向補文檔）
**Source Commits**: 72a1c3a, e2d79df

## Phase 1: Domain Model

- [x] [T001] [P] 建立 `QueueEntry` entity（Id, ClinicId, VisitId, QueueType, QueueNumber, Priority, Status, CreatedAt/CalledAt/SkippedAt）
- [x] [T002] [P] 定義 `QueueType` enum（Waiting, Pharmacy）
- [x] [T003] [P] 定義 `QueueEntryStatus` enum（Waiting, Called, Skipped, Completed）

## Phase 2: Persistence

- [x] [T004] EF Core 設定 QueueEntry 的 PK/FK/索引
- [x] [T005] Migration 建立 `queue_entries` 表
- [x] [T006] 建立複合索引覆蓋 `(ClinicId, QueueType, Status, Priority, CreatedAt)` 主查詢

## Phase 3: Application 層介面

- [x] [T007] [P] 定義 `QueueEntryDto`（VisitId, QueueNumber, PatientName, Priority, Status, CheckedInAt）
- [x] [T008] [P] 定義 `QueuePositionDto`（Position, QueueNumber, TotalWaiting）
- [x] [T009] [P] 定義 `CallNextRequest`（ClinicId, QueueType, RoomId?）
- [x] [T010] 定義 `IQueueService` 介面（4 方法）

## Phase 4: 查詢佇列（US1）

- [x] [T011] [US1] 實作 `GetQueueAsync(clinicId, queueType)`
- [x] [T012] [US1] `Enum.TryParse<QueueType>` 忽略大小寫解析
- [x] [T013] [US1] `Include(q => q.Visit).ThenInclude(v => v.Patient)` 載入病患姓名
- [x] [T014] [US1] `OrderByDescending(Priority).ThenBy(CreatedAt)` 排序
- [x] [T015] [US1] 投影至 `QueueEntryDto` 並處理 `FullName ?? "匿名"`

## Phase 5: 叫下一位（US2）

- [x] [T016] [US2] 實作 `CallNextAsync(request, callerUserId)`
- [x] [T017] [US2] 取出佇列首位（`FirstOrDefaultAsync`）
- [x] [T018] [US2] 佇列空時回 `Result.Fail("目前佇列中沒有等待的病患")`
- [x] [T019] [US2] 更新 `QueueEntry.Status=Called` + `CalledAt=UtcNow`
- [x] [T020] [US2] 查詢 workflow `StepCode="called"` 步驟並更新 `Visit.CurrentStepId`
- [x] [T021] [US2] 若 `request.RoomId` 有值則更新 `Visit.RoomId`
- [x] [T022] [US2] 新增 VisitEvent（Notes="叫號"、TriggerType=Manual、TriggeredByUserId=callerUserId）
- [x] [T023] [US2] 三者同 `SaveChangesAsync`

## Phase 6: 查詢位置（US3）

- [x] [T024] [US3] 實作 `GetPositionAsync(clinicId, visitId)`
- [x] [T025] [US3] 找不到 entry 時回 `Result.Fail("找不到該候診紀錄")`
- [x] [T026] [US3] COUNT `(Priority > entry.Priority) OR (Priority = entry.Priority AND CreatedAt < entry.CreatedAt)` + 1
- [x] [T027] [US3] COUNT 佇列中所有 Waiting 總數

## Phase 7: 過號（US4）

- [x] [T028] [US4] 實作 `SkipAsync(clinicId, visitId, callerUserId)`
- [x] [T029] [US4] 檢查 `entry.Status == Waiting`，否則回 `Result.Fail`
- [x] [T030] [US4] 更新 `Status=Skipped` + `SkippedAt=UtcNow`

## Phase 8: API 與整合

- [x] [T031] 建立 `QueueController` 與四個端點
- [x] [T032] DI 註冊 `IQueueService → QueueService`（Scoped）
- [x] [T033] Seed Data：建立範例 QueueEntry 配合 workflow

---

## 依賴圖

```
T001-T003 (Entities/Enums) [P]
    ↓
T004-T006 (Persistence)
    ↓
T007-T010 (DTOs/Interface) [P]
    ↓
T011-T015 (US1 查詢) ──┐
T016-T023 (US2 叫號) ──┤
T024-T027 (US3 位置) ──┼──→ T031-T033 (API/整合)
T028-T030 (US4 過號) ──┘
```

## 未來改善（非本 Phase）

- [ ] 加入 Redis 快取層（原架構規劃）
- [ ] 樂觀鎖（RowVersion）防併發叫號
- [ ] SignalR 即時推播叫號通知前端
- [ ] 自動過號排程（報到 > N 分鐘未到）
- [ ] 候診時間統計 dashboard（平均候診、P95）
- [ ] 壓力測試：100+ 候診併發情境
