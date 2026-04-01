# Feature Specification: natural-language-command

> Methodology: Spec-Driven Development | Tool: Vibe Lens
> Inspired by [GitHub Spec Kit](https://github.com/github/spec-kit)

**Created**: 2026-03-31
**Status**: Approved

---

## Summary

讓醫護人員透過自然語言指令操作門診系統，取代傳統按鈕操作。護理師可以說「叫下一位」「3號過號」，醫師可以說「開普拿疼 500mg TID 三天份」「完成看診，需要拿藥」。系統透過 LLM 解析意圖，對應到現有的 Service 方法執行。使用現有 ILlmClient 抽象層（OMLX/Groq fallback），不新增 LLM provider。

## User Scenarios & Testing

### P1: As a 護理師, I want 輸入「叫下一位」就自動叫號 so that 不用在多個畫面間切換操作

**Why this priority**: 叫號是門診最頻繁的操作，護理師每天執行上百次，且無參數，適合作為端到端驗證的第一個 story

**Acceptance Scenarios**:

1. **Given** 候診佇列中有至少 1 位候診病患
   **When** 護理師輸入「叫下一位」
   **Then** 系統回傳 `{action: "call_next", needsConfirm: true}`，前端顯示確認對話框，確認後叫號成功

2. **Given** 候診佇列為空
   **When** 護理師輸入「叫下一位」
   **Then** 系統回傳 message 提示「目前沒有候診病患」

### P2: As a 護理師, I want 輸入「3號過號」就跳過該病患 so that 快速處理未到場的病患

**Why this priority**: 過號是叫號的延伸，頻率次高，需從自然語言擷取號碼參數，驗證 LLM 參數解析能力

**Acceptance Scenarios**:

1. **Given** 3 號病患在候診佇列中
   **When** 護理師輸入「3號過號」
   **Then** 系統回傳 `{action: "skip", params: {queueNumber: 3}, needsConfirm: true}`，確認後該病患被標記為過號

2. **Given** 輸入的號碼不存在於佇列中
   **When** 護理師輸入「99號過號」
   **Then** executor 執行時回傳錯誤訊息「查無此號碼」

### P3: As a 護理師, I want 輸入「今天還有幾位候診」查詢候診狀態 so that 掌握即時候診人數

**Why this priority**: 查詢類指令不涉及狀態變更，風險低，驗證系統讀取資料能力

**Acceptance Scenarios**:

1. **Given** 目前有 5 位候診病患
   **When** 護理師輸入「今天還有幾位候診」
   **Then** 系統回傳 `{action: "query_queue", needsConfirm: false}` 並直接顯示候診人數與清單

### P4: As a 醫師, I want 輸入「開普拿疼 500mg TID 三天份」自動建立處方 so that 減少手動填寫表單

**Why this priority**: 涉及藥品名稱、劑量、頻率（TID/BID/QD）、天數等多參數解析，複雜度最高

**Acceptance Scenarios**:

1. **Given** 醫師正在看診中
   **When** 醫師輸入「開普拿疼 500mg TID 三天份」
   **Then** 系統回傳 `{action: "create_prescription", params: {drugName: "普拿疼", dosage: "500mg", frequency: "TID", days: 3}, needsConfirm: true}`，確認後建立處方

2. **Given** 醫師輸入不完整的處方指令
   **When** 醫師輸入「開普拿疼」
   **Then** LLM 回傳 message 詢問缺少的資訊（劑量、頻率、天數）

### P5: As a 醫師, I want 輸入「完成看診，需要拿藥」結束看診並轉藥局 so that 一句話完成流程推進

**Why this priority**: 涉及工作流程狀態推進（看診→藥局），需確保狀態轉換正確性

**Acceptance Scenarios**:

1. **Given** 醫師正在看診中
   **When** 醫師輸入「完成看診，需要拿藥」
   **Then** 系統回傳 `{action: "complete_consult", params: {needsMedication: true}, needsConfirm: true}`，確認後將病患狀態轉為「等候取藥」

2. **Given** 醫師輸入「看完了，不用拿藥」
   **When** 系統解析
   **Then** 回傳 `{params: {needsMedication: false}}`，確認後病患狀態轉為「完成」

### P6: As a 管理員, I want 輸入「今天看了幾個病人」查詢統計 so that 快速了解營運狀況

**Why this priority**: 管理員使用頻率最低，需聚合查詢，對日常流程影響最小

**Acceptance Scenarios**:

1. **Given** 今日已完成看診 30 位病患
   **When** 管理員輸入「今天看了幾個病人」
   **Then** 系統回傳 `{action: "query_stats", needsConfirm: false}` 並顯示今日統計

## Requirements

### Functional Requirements

- **FR-001**: LLM 解析使用者自然語言意圖，回傳結構化 JSON（action + params + message + needsConfirm）
- **FR-002**: 支援 6 種 action：call_next、skip、query_queue、create_prescription、complete_consult、query_stats
- **FR-003**: 每個 action 對應到現有 Service 方法（QueueService、ConsultService 等），不新增業務邏輯
- **FR-004**: 依角色限制可用 action — 護理師：call_next/skip/query_queue/query_stats；醫師：create_prescription/complete_consult/query_queue/query_stats；管理員：query_queue/query_stats
- **FR-005**: 查詢類指令（query_*）直接回傳結果，操作類指令需前端確認後才執行
- **FR-006**: 使用現有 ILlmClient 抽象層，provider fallback 機制與 AI 分流一致
- **FR-007**: Action Router — ICommandExecutor 介面，每個 action 一個 executor 實作
- **FR-008**: 前端各角色 Dashboard 加入指令輸入框，支援文字輸入

### Non-Functional Requirements

- **NFR-001**: LLM 解析回應時間 < 3 秒（與 AI 分流一致）
- **NFR-002**: LLM 解析失敗時回傳友善錯誤訊息「指令解析失敗，請換個說法再試一次」，不中斷系統
- **NFR-003**: 所有操作類指令執行前必須經過角色權限驗證 + 前端確認雙重保護

## Key Entities

| Entity | Description | Key Attributes |
|--------|-------------|----------------|
| CommandAction | 指令動作列舉 | CallNext, Skip, QueryQueue, CreatePrescription, CompleteConsult, QueryStats, Unknown |
| CommandRequest | 指令請求 DTO | ClinicId, Role, Command(自然語言文字) |
| CommandResponse | 指令回應 DTO | Action, Parameters, Result(confirm/done/error), Message |
| ICommandExecutor | 指令執行器介面 | CanHandle(action), ExecuteAsync(context) |

## Edge Cases

- 使用者輸入無意義文字（如「哈哈哈」）→ LLM 回傳 action: "unknown"，message 提示重新輸入
- 使用者嘗試執行無權限的操作（如護理師說「開處方」）→ CommandPromptBuilder 不會列出該 action，LLM 不會產生該 action；若 LLM 仍回傳，Router 做二次權限檢查拒絕
- LLM 回傳的 JSON 格式錯誤或缺少必要欄位 → ParseCommandResponse 回傳 null，觸發 fallback 錯誤訊息
- 同時多人下指令 → 各請求獨立，靠 Redis 佇列的原子操作保證一致性
- 網路異常導致所有 LLM provider 都失敗 → 回傳「AI 服務暫時無法使用」

## Success Criteria

- [ ] 護理師可用自然語言完成叫號、過號操作，成功率 > 90%
- [ ] 醫師可用自然語言完成看診、開處方操作，成功率 > 85%
- [ ] LLM 解析回應時間 < 3 秒（P95）
- [ ] 操作類指令 100% 經過確認才執行
- [ ] 無權限 action 100% 被攔截

## Assumptions

- 現有 QueueService、ConsultService 等 Service 層 API 穩定可用
- LLM（OMLX/Groq）能正確理解繁體中文醫療用語
- 前端已有各角色 Dashboard 頁面可嵌入指令輸入元件

## Constraints & Dependencies

- 依賴 Phase 1 AI 分流已建立的 ILlmClient 抽象層
- 開處方功能依賴藥品資料表（需確認是否已建立）
- 前端 Vue 3 + TypeScript 技術棧
