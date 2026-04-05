# Tasks: SignalR 即時通訊推播

**Feature**: 005-signalr | **Status**: 待開發
**Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)

## Phase 1: Setup

無新增設定。SignalR 已於 Program.cs 註冊（`MapHub<VisitHub>("/hubs/visit")`）、前端 `@microsoft/signalr` 套件已安裝。

## Phase 2: Foundational（介面 + 群組命名 + Hub 補強）

**阻塞性**：所有 user story 都依賴此 phase 完成。

- [ ] T001 [P] 新增 `GroupNames` 靜態類別，集中定義 `ClinicGroup(clinicId)` 與 `VisitGroup(visitId)` 兩個方法（`backend/src/ClinicPlatform.WebAPI/Hubs/GroupNames.cs`）
- [ ] T002 [P] 新增 `INotificationPublisher` 介面，宣告三個方法：`PublishQueueUpdatedAsync`、`PublishVisitStepChangedAsync`、`PublishPatientCalledAsync`（`backend/src/ClinicPlatform.Application/Features/Notifications/INotificationPublisher.cs`）
- [ ] T003 新增 `SignalRNotificationPublisher` 實作 `INotificationPublisher`，注入 `IHubContext<VisitHub>` 和 `ILogger`，每個方法 try-catch 包裝 fire-and-forget（`backend/src/ClinicPlatform.Infrastructure/Services/Notifications/SignalRNotificationPublisher.cs`）
- [ ] T004 修改 `VisitHub.cs`：將 `JoinVisit` / `LeaveVisit` 重命名為 `JoinVisitGroup` / `LeaveVisitGroup`（對齊前端呼叫）、新增 `OnConnectedAsync` / `OnDisconnectedAsync` log、使用 `GroupNames` 統一命名（`backend/src/ClinicPlatform.WebAPI/Hubs/VisitHub.cs`）
- [ ] T005 修改 `Program.cs`：註冊 `INotificationPublisher` → `SignalRNotificationPublisher`（Singleton）、確認 CORS 允許 SignalR WebSocket（`backend/src/ClinicPlatform.WebAPI/Program.cs`）

## Phase 3: User Story 2 - 叫號台佇列同步（P1）

**Goal**：多個叫號台/看診室畫面同步顯示佇列變動，避免重複叫號
**Independent Test**：開兩個叫號台分頁（同診所），A 分頁叫號後 B 分頁的佇列列表在 2 秒內自動移除該筆

- [ ] T006 [US2] 修改 `QueueService.CallNextAsync`：建構子注入 `INotificationPublisher`，在 `SaveChangesAsync` 成功後呼叫 `PublishQueueUpdatedAsync(clinicId, queueType, "called")`（`backend/src/ClinicPlatform.Infrastructure/Services/QueueService.cs`）
- [ ] T007 [US2] 修改 `QueueService.SkipAsync`：同上注入模式，推播 `PublishQueueUpdatedAsync(clinicId, queueType, "skipped")`（`backend/src/ClinicPlatform.Infrastructure/Services/QueueService.cs`）
- [ ] T008 [P] [US2] 修改 `CheckInService.CheckInAsync`：注入 Publisher，報到成功後推播 `PublishQueueUpdatedAsync(clinicId, "waiting", "checkedin")`（`backend/src/ClinicPlatform.Infrastructure/Services/CheckInService.cs`）
- [ ] T009 [P] [US2] 修改 `VisitService.StartConsultAsync`：注入 Publisher，推播 `PublishQueueUpdatedAsync(clinicId, "calling", "started")` + `PublishVisitStepChangedAsync(visitId, "in_consult", "看診中")`（`backend/src/ClinicPlatform.Infrastructure/Services/VisitService.cs`）
- [ ] T010 [P] [US2] 修改 `VisitService.CompleteConsultAsync`：注入 Publisher，推播 `PublishQueueUpdatedAsync(clinicId, "in_consult", "completed")` + `PublishVisitStepChangedAsync`（`backend/src/ClinicPlatform.Infrastructure/Services/VisitService.cs`）
- [ ] T011 [US2] 前端叫號台/看診室頁面加入 `useSignalR` 訂閱 `QueueUpdated`，收到後呼叫現有 `GET /api/queues/*` API 補拉最新佇列（目標檔案依現況，通常為 `frontend/src/views/nurse/*.vue` 或 `views/doctor/*.vue`）

## Phase 4: User Story 1 - 病患候診畫面即時更新（P1）

**Goal**：病患端看到順位即時減少、被叫號時顯示「請至 X 診間」
**Independent Test**：開病患候診頁 + 護理師叫號台，按「叫下一位」後病患端 2 秒內順位遞減，輪到自己時畫面顯示提示

- [ ] T012 [US1] 修改 `QueueService.CallNextAsync`：叫號成功後額外推播 `PublishPatientCalledAsync(visitId, roomNumber)` 至 visit group（擴充 T006）（`backend/src/ClinicPlatform.Infrastructure/Services/QueueService.cs`）
- [ ] T013 [US1] 修改 `VisitService.AdvanceStepAsync`：注入 Publisher，推播 `PublishVisitStepChangedAsync`（`backend/src/ClinicPlatform.Infrastructure/Services/VisitService.cs`）
- [ ] T014 [P] [US1] 修改 `useSignalR.ts`：新增 `onPatientCalled` 事件訂閱方法、確認 `JoinVisitGroup` 呼叫名稱一致（`frontend/src/composables/useSignalR.ts`）
- [ ] T015 [US1] 修改 `Queue.vue`：掛載時 connect + JoinVisitGroup、訂閱 `VisitStepChanged` 和 `PatientCalled`、收到 `PatientCalled` 時顯示「請至 X 診間」提示 + 播放提示音、收到 `VisitStepChanged` 時呼叫 API 補拉順位（`frontend/src/views/patient/Queue.vue`）

## Phase 5: User Story 3 - 網路中斷自動重連（P2）

**Goal**：斷線恢復後自動重連、UI 顯示連線狀態、補拉最新資料
**Independent Test**：DevTools Network 切 offline 15 秒後恢復，畫面顯示「重連中 → 已連線」且佇列/順位自動更新

- [ ] T016 [P] [US3] 新增 `stores/signalr.ts`：用 Pinia 建立全域 store，管理 `connectionState: 'disconnected' | 'connecting' | 'connected' | 'reconnecting'`（`frontend/src/stores/signalr.ts`）
- [ ] T017 [US3] 修改 `useSignalR.ts`：綁定 store 狀態、`onreconnecting` 設為 reconnecting、`onreconnected` 設為 connected 並觸發 callback 讓頁面補拉資料、`onclose` 設為 disconnected（`frontend/src/composables/useSignalR.ts`）
- [ ] T018 [US3] 修改 `useSignalR.ts`：新增 `onReconnected(callback)` 公開方法，讓頁面可註冊重連後要執行的補拉邏輯（`frontend/src/composables/useSignalR.ts`）
- [ ] T019 [P] [US3] 修改 `Queue.vue`：加入連線狀態指示器（綠/黃/紅小圓點 + 文字）、註冊 `onReconnected` 補拉順位（`frontend/src/views/patient/Queue.vue`）

## Phase 6: Polish

- [ ] T020 [P] 撰寫 `SignalRNotificationPublisher` 單元測試：用 `Mock<IHubContext<VisitHub>>` 驗證 group name 與 payload 內容正確（`backend/tests/.../SignalRNotificationPublisherTests.cs`）
- [ ] T021 [P] 在 `VisitHub.cs` 加上 TODO 註解，標記未來 JWT 整合後需補授權檢查（`backend/src/ClinicPlatform.WebAPI/Hubs/VisitHub.cs`）
- [ ] T022 驗證 SC-001：用 curl 觸發 `CallNext` API，前端 DevTools Network WebSocket 訊息時間戳 P95 < 2 秒
- [ ] T023 驗證 SC-003：記錄實作前後 `/api/queues` polling 請求次數，確認下降 ≥ 90%

## Dependencies

- **Phase 2** 阻塞所有後續 phase（介面與 Publisher 必須先存在）
- **Phase 3** 與 **Phase 4** 有檔案衝突（都改 `QueueService.cs` 與 `VisitService.cs`），**建議序列執行**（先 Phase 3 奠定佇列推播基礎，Phase 4 擴充病患端事件）
- **Phase 5** 依賴 Phase 3 或 Phase 4 的推播已運作（才能驗證重連補拉）
- **Phase 6** 最後執行

## Parallel Execution Examples

### Phase 2 可平行
T001 + T002 同時做（不同檔案、無互相引用）

### Phase 3 可平行組
T008 (CheckInService) + T009 (VisitService.StartConsult) + T010 (VisitService.Complete) 可平行（不同方法，注入模式相同）
但 T006 + T007 不平行（同檔案 QueueService.cs）

### Phase 5 可平行
T016 (stores/signalr.ts) + T019 (Queue.vue) 不同檔案可平行
T017 + T018 同檔案 useSignalR.ts 必須序列

## Implementation Strategy

**MVP 範圍**：只做 Phase 2 + Phase 3（US2 佇列同步）
- 工作人員端先有即時佇列同步，病患端繼續用現有 polling（短期過渡）
- 驗證 SignalR 管線運作後再加 US1

**完整交付順序**：Phase 2 → Phase 3 (US2) → Phase 4 (US1) → Phase 5 (US3) → Phase 6

**任務總數**：23 個
- Phase 2：5 個（Foundational）
- Phase 3 (US2)：6 個
- Phase 4 (US1)：4 個
- Phase 5 (US3)：4 個
- Phase 6 (Polish)：4 個
