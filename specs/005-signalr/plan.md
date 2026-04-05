# Implementation Plan: SignalR 即時通訊推播

**Branch**: `005-signalr` | **Date**: 2026-04-05 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/005-signalr/spec.md`

## Summary

將已存在但未啟用的 SignalR 骨架（`VisitHub`、`useSignalR`）補齊為端到端推播管線。定義 `INotificationPublisher` 介面於 Application 層，Infrastructure 層以 `SignalRNotificationPublisher` 包裝 `IHubContext<VisitHub>`，注入至 `QueueService` 與 `VisitService` 的寫入方法（CallNext / Skip / StartConsult / CompleteConsult / AdvanceStep），在交易成功後推送事件至對應 group。前端 `useSignalR` 補齊重連後補拉資料的邏輯，`Queue.vue` 訂閱事件更新 UI。

## Technical Context

**Language/Version**：C# 13 / .NET 10、TypeScript 5 / Vue 3
**Primary Dependencies**：`Microsoft.AspNetCore.SignalR`（內建）、`@microsoft/signalr` 8.x（已安裝）
**Storage**：不涉及持久化（推播為 fire-and-forget，訊息遺失由前端重連後 API 補拉彌補）
**Testing**：xUnit（Publisher 單元測試以 `Mock<IHubContext>` 驗證 group name 與 payload）、手動 E2E（多分頁同步）
**Target Platform**：單節點部署（無 scale-out 需求，暫不導入 Redis backplane）
**Project Type**：Web service + SPA
**Performance Goals**：事件抵達前端 P95 < 2 秒、100 並行連線穩定、訊息送達率 ≥ 99%
**Constraints**：Hub 方法需授權（僅允許加入自己診所的 group）、訊息 payload 最小化（只帶 ID、type、timestamp，詳細資料前端呼叫 API 取得）
**Scale/Scope**：單診所同時在線連線數 < 50、事件頻率 < 10 msg/秒

## Constitution Check

（依 CLAUDE.md 原則檢核）

- ✅ **繁體中文**：log、錯誤訊息使用繁中
- ✅ **Clean Architecture**：`INotificationPublisher` 介面於 Application 層、實作於 Infrastructure 層、`VisitHub` 於 WebAPI 層
- ✅ **錯誤不中斷**：推播失敗僅記 log，不影響原始業務交易（try-catch 包裝）
- ✅ **與現有 Result<T> 模式相容**：Service 回傳不變，Publisher 在 Service 內部非同步呼叫

## Project Structure

### Documentation

```text
specs/005-signalr/
├── spec.md              # 功能規格（WHAT & WHY）
├── plan.md              # 本檔案（HOW）
└── tasks.md             # 待 /speckit.tasks 產出
```

### Source Code

```text
backend/src/
├── ClinicPlatform.Application/
│   └── Features/Notifications/
│       └── INotificationPublisher.cs    # 新增：推播介面
│
├── ClinicPlatform.Infrastructure/
│   └── Services/Notifications/
│       └── SignalRNotificationPublisher.cs   # 新增：SignalR 實作
│
└── ClinicPlatform.WebAPI/
    ├── Hubs/
    │   └── VisitHub.cs                  # 修改：補授權檢查、命名對齊、log
    └── Program.cs                       # 修改：註冊 Publisher、CORS for SignalR

frontend/src/
├── composables/
│   └── useSignalR.ts                    # 修改：補重連後補拉、狀態 enum、事件訂閱 API
├── stores/
│   └── signalr.ts                       # 新增：全域連線狀態與群組管理
└── views/
    ├── patient/Queue.vue                # 修改：訂閱 VisitStepChanged、PatientCalled
    └── nurse/Callboard.vue              # 修改（若存在）：訂閱 QueueUpdated
```

## Design Decisions

### 決策 1：訊息合約 — 薄 payload + 前端 API 補拉

**選項 A**：推播時帶完整資料（QueueEntry 完整物件）
**選項 B**：推播只帶 ID/type/timestamp，前端收到後呼叫 API 取最新資料 ← **採用**

**理由**：
- payload 小（< 200 bytes）降低頻寬與序列化成本
- 避免敏感資料（病患姓名）在 SignalR 通道外洩到無關 client
- 前端 API 本來就要能重拉（斷線重連後必需），多呼叫一次不增加複雜度
- 訊息順序問題大幅減輕（即使訊息錯序，最後一次 API 拉取結果永遠是正確的）

### 決策 2：事件名稱與觸發點

| 事件名稱 | 觸發點（Service 方法） | 目標 group | Payload |
|---------|---------------------|-----------|---------|
| `QueueUpdated` | CallNextAsync / SkipAsync / CheckInAsync / StartConsultAsync / CompleteConsultAsync | `clinic_{clinicId}` | `{ clinicId, queueType, changeType, timestamp }` |
| `VisitStepChanged` | AdvanceStepAsync / StartConsultAsync / CompleteConsultAsync | `visit_{visitId}` | `{ visitId, newStep, stepName, timestamp }` |
| `PatientCalled` | CallNextAsync（成功時） | `visit_{visitId}` | `{ visitId, roomNumber, calledAt }` |

### 決策 3：授權策略 — 延後到 MVP 後

目前（MVP）：不做 Hub 授權檢查（單一開發者、本地測試）。
未來（Phase 2）：Hub 方法加 `[Authorize]`、從 JWT claim 取 `clinicId` 並驗證 group 加入請求。

**理由**：系統尚未導入 JWT 驗證（Controllers 也沒 `[Authorize]`），SignalR 獨自超前會造成整合問題。留 TODO 註解標記。

### 決策 4：Scale-out — 暫不導入 Redis backplane

**理由**：目前單節點部署、連線數遠低於單機承受上限。日後 scale-out 時再加 `AddStackExchangeRedis()`，程式不需大改。

### 決策 5：推播錯誤處理 — Fire-and-forget + 記 log

Publisher 內部 try-catch，失敗僅記 warning log，不影響原交易。原因：事件通知失敗的成本遠低於業務交易回滾。前端重連會補拉最新狀態。

### 決策 6：前端連線狀態 enum

```typescript
type ConnectionState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting'
```

UI 以小圓點 + 文字顯示於頁面角落（綠/黃/紅）。

## 介面定義（虛擬碼）

```csharp
// Application 層
public interface INotificationPublisher
{
    Task PublishQueueUpdatedAsync(Guid clinicId, string queueType, string changeType);
    Task PublishVisitStepChangedAsync(Guid visitId, string newStep, string stepName);
    Task PublishPatientCalledAsync(Guid visitId, string roomNumber);
}

// Infrastructure 層
public class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<VisitHub> _hub;
    private readonly ILogger<SignalRNotificationPublisher> _logger;
    // 實作 try-catch + log
}
```

## Risks & Mitigations

| 風險 | 影響 | 緩解 |
|-----|------|-----|
| SignalR 連線未建立就推送 | 訊息遺失 | 前端重連後呼叫 API 補拉（FR-003） |
| Hub 方法 group 名稱與 Publisher 不一致 | 訊息推到空 group | 集中定義 `GroupNames` 靜態類別 |
| 多分頁同 visit 重複推送 | 無（SignalR 以 connectionId 為單位推） | N/A |
| 交易 commit 前就推播導致資料不一致 | 前端拉到舊資料 | Publisher 在 `SaveChangesAsync` 之後呼叫 |
| CORS 未設定導致前端連不上 Hub | 連線失敗 | Program.cs 已有 CORS，確認 `AllowCredentials` + `WithOrigins` |

## Open Questions

無。（授權策略、scale-out 已在決策中明確 defer 到未來）

## Success Verification

- **SC-001**（P95 < 2s）：本機 curl 觸發 API 後，前端 DevTools Network 面板看 WebSocket 訊息時間戳
- **SC-003**（polling 下降 ≥ 90%）：比較前後 `/api/queues` 請求數（用 preview_network 觀察）
- **SC-005**（叫號顯示 < 3s）：手動測試 Story 1 的 acceptance scenario 2
