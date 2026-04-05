# Feature Specification: SignalR 即時通訊推播

**Feature Branch**: `005-signalr`
**Created**: 2026-04-05
**Status**: Draft
**Input**: User description: "把候診佇列、叫號、看診狀態變更等事件透過 SignalR 即時推播給相關角色的前端，取代輪詢"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - 病患候診畫面即時更新 (Priority: P1)

病患完成報到後，在候診畫面（`Queue.vue`）看到自己的號碼、前方剩餘人數、預估等候時間。當護理師在叫號台按下「叫下一位」時，病患畫面應**立即**（< 2 秒內）反映新的順位資訊，不需重新整理。當輪到自己時，畫面應顯著提示「請至 X 診間」並播放提示音。

**Why this priority**: 這是 SignalR 最核心的使用場景，也是病患體驗的主要差異化點。沒有即時推播，病患只能靠輪詢或手動 F5，體驗差且伺服器負擔大。這個 user story 獨立完成就能交付一個可 demo 的 MVP。

**Independent Test**: 開兩個瀏覽器分頁 — 一個是病患候診頁（含 visitId），另一個是護理師叫號台。在護理師端按「叫下一位」，驗證病患端在 2 秒內順位數字自動減少，且當病患成為被叫號者時畫面出現提示。不需重整頁面。

**Acceptance Scenarios**:

1. **Given** 病患 A 在候診畫面順位為 3，**When** 護理師叫走前一位病患 **Then** 病患 A 的畫面順位在 2 秒內自動變為 2
2. **Given** 病患 A 的順位為 1，**When** 護理師對病患 A 按下「叫號」 **Then** 病患 A 畫面顯示「請至 3 號診間」並觸發提示音
3. **Given** 病患 A 的網路暫時斷線 10 秒，**When** 網路恢復 **Then** SignalR 自動重連並拉取最新順位，畫面顯示與當前佇列一致
4. **Given** 病患同時開了兩個分頁觀看同一個 visit，**When** 順位變更 **Then** 兩個分頁都同步更新

---

### User Story 2 - 叫號台/看診室佇列同步 (Priority: P1)

護理師的叫號台、醫師的看診室畫面顯示該診所、該科別的即時候診佇列。當任何人員進行操作（報到、叫號、過號、開始看診、完成看診），所有在線的叫號台與看診室畫面應立即同步更新佇列列表，避免兩位護理師同時叫同一位病患。

**Why this priority**: 多位工作人員並行操作是實際診所日常，沒有同步會造成資料衝突、重複叫號、病患困擾。屬於「資料一致性」剛性需求，必須與 P1 並列。

**Independent Test**: 開兩個叫號台分頁（同一診所）。在 A 分頁按「叫下一位」，驗證 B 分頁的佇列列表在 2 秒內移除該筆、佇列人數數字同步遞減；在 B 分頁按「過號」，A 分頁亦同步反映。

**Acceptance Scenarios**:

1. **Given** 兩個叫號台 A、B 顯示相同佇列，**When** A 叫號一位病患 **Then** B 的佇列列表中該病患立即被移出「等待中」狀態
2. **Given** 醫師完成看診，**When** 後端觸發 `CompleteConsult` **Then** 該診所所有叫號台與看診室畫面的佇列同步更新，被看診病患從「看診中」移除
3. **Given** 新病患完成報到，**When** Queue 收到新 entry **Then** 所有叫號台畫面佇列尾端出現該病患且人數數字 +1

---

### User Story 3 - 網路中斷自動重連 (Priority: P2)

當使用者網路短暫中斷（Wi-Fi 切換、背景進入省電、閘道重啟）後恢復，SignalR 連線應自動重建，並重新加入先前的 group（clinic / visit），且在重連後拉一次最新狀態補差（因為斷線期間的訊息會遺失）。使用者介面應提示連線狀態（已連線 / 重連中 / 離線）。

**Why this priority**: 是穩定性需求而非核心功能。P1 完成後使用者已能感受到即時性，但網路瞬斷場景若處理不當會造成畫面停留在舊資料，隱性誤導。

**Independent Test**: 連線後用 DevTools Offline 模式斷網 15 秒，確認畫面顯示「重連中」；恢復網路後畫面顯示「已連線」並自動補拉最新佇列資料。

**Acceptance Scenarios**:

1. **Given** 病患候診畫面連線正常，**When** 網路斷線 **Then** 畫面 2 秒內顯示「連線中斷，重新連線中…」
2. **Given** 網路斷線中，**When** 網路恢復 **Then** SignalR 自動重連、重新加入 visit group、並主動呼叫 API 拉一次最新順位
3. **Given** 網路斷線超過 60 秒，**When** 持續無法連線 **Then** 畫面提示「請檢查網路」並提供手動重連按鈕

---

### Edge Cases

- 當 SignalR 連線因伺服器重啟而中斷時，所有 client 應能自動重連並重新加入 group
- 當一位使用者同時開啟多個分頁觀看同一 visit，每個分頁都應獨立接收推播（以 connectionId 為單位）
- 當 Hub 方法被惡意 client 呼叫（例如監聽別的診所 group）時，應有身份驗證與授權檢查（同一診所的使用者才能加入該 clinic group）
- 當推播訊息量突增（例如大診所同時 50 人報到），Hub 應能承受且不丟訊息
- 當訊息送達順序不一致（舊訊息晚於新訊息到達）時，前端應以訊息內的 timestamp 或 version 判斷是否忽略

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: 系統 MUST 在病患報到、叫號、過號、開始看診、完成看診、Visit 步驟推進等事件發生時，透過 SignalR 向相關群組推送訊息
- **FR-002**: 前端 MUST 能以 `clinicId` 加入診所群組、以 `visitId` 加入個別就診群組，接收對應範圍的推播
- **FR-003**: 系統 MUST 支援自動重連（斷線後嘗試 0s / 2s / 5s / 10s / 30s），重連後前端自動重新加入原先群組
- **FR-004**: 系統 MUST 定義明確的訊息合約（event name + payload schema），前後端共用型別定義
- **FR-005**: 系統 MUST 驗證加入群組的 client 具備對應診所的存取權（JWT 或 session claim 中的 clinicId 與請求參數一致）
- **FR-006**: 當 Visit 的 workflow step 推進時，系統 MUST 推送 `VisitStepChanged` 事件至該 visit group，包含新 step、stepName、timestamp
- **FR-007**: 當佇列有任何變動（新增、叫號、過號、移除）時，系統 MUST 推送 `QueueUpdated` 事件至該 clinic group，包含 queueType、變動類型
- **FR-008**: 當病患被叫號時，系統 MUST 額外推送 `PatientCalled` 事件至該 visit group，包含診間號碼、呼叫時間
- **FR-009**: 前端 MUST 在 UI 顯示當前連線狀態（已連線 / 重連中 / 離線）
- **FR-010**: 後端 MUST 在 Hub 生命週期事件（OnConnected / OnDisconnected）中記錄 log，便於除錯與監控

### Key Entities

- **SignalR Message**: 即時推播訊息，包含 eventName、payload（JSON）、timestamp、targetGroup
- **Hub Group**: 邏輯群組，目前支援 `clinic_{clinicId}` 與 `visit_{visitId}` 兩種命名空間
- **Connection Context**: 每個 SignalR 連線包含 connectionId、使用者身份（userId、clinicId、role）、已加入的 group 列表

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 事件發生後推播抵達前端的端到端延遲 P95 < 2 秒（本機測試）
- **SC-002**: 前端在網路中斷 30 秒內恢復時，能自動重連並補齊資料，使用者不需手動重整
- **SC-003**: 取代輪詢後，前端對 `/api/queues/*` 與 `/api/visits/*/status` 的 polling 請求數量下降 ≥ 90%
- **SC-004**: 100 個並行 SignalR 連線場景下，伺服器 CPU/記憶體維持穩定，訊息送達率 ≥ 99%
- **SC-005**: 病患從被叫號到畫面顯示「請至診間」的時間 < 3 秒（含網路傳輸）
