# Feature Specification: AI 智慧症狀分流

**Feature Branch**: `001-ai-triage`
**Created**: 2026-04-05
**Status**: Completed（反向補文檔）
**Input**: 病患報到時輸入症狀文字描述，由 AI 建議適合的看診科別、優先度與預估等候時間。系統採用雙 LLM provider（OMLX 本地 + Groq 雲端）以 fallback 方式確保可用性。

## User Scenarios & Testing

### User Story 1 - 病患輸入症狀取得科別建議（Priority: P1）

病患在報到機或報到介面輸入自己的症狀（例如「喉嚨痛三天、有發燒」），系統呼叫 AI 分析後回傳建議的科別名稱、優先度與預估等候分鐘數。若 AI 建議的科別存在於該院所，則自動帶入報到流程讓病患確認。

**Why this priority**：這是 AI 分流功能的核心價值——降低病患選錯科別的比例、縮短報到櫃檯人工分流時間。沒有這個故事，整個 Feature 就不成立。

**Independent Test**：單獨呼叫 `POST /api/ai/triage` 帶入 `{clinicId, symptoms}`，驗證回傳 JSON 含 `department`、`priority`、`estimatedWaitMinutes`、`reasoning` 四個欄位，且 `departmentId` 能對應到該院所的有效科別。

**Acceptance Scenarios**：

1. **Given** 院所有「家醫科、耳鼻喉科、小兒科」三個啟用中的科別，**When** 病患輸入「喉嚨痛、鼻塞」，**Then** 系統回傳 `department="耳鼻喉科"` 且 `departmentId` 對應到該科別的 GUID。
2. **Given** 院所僅有單一科別，**When** 病患輸入任意症狀，**Then** 系統回傳該唯一科別作為建議。
3. **Given** 病患描述含「胸痛、呼吸困難」等緊急症狀，**When** 呼叫分流，**Then** `priority` 回傳 `2`（緊急）。

---

### User Story 2 - LLM Provider 自動 Fallback（Priority: P1）

系統設定優先使用本地 OMLX（Qwen2.5-7B-Instruct）以節省成本與保護資料；當 OMLX 服務不可用（連線失敗、回應無法解析）時，自動 fallback 到 Groq 雲端 API 繼續服務；若兩者皆失敗則回傳明確錯誤訊息給使用者。

**Why this priority**：這是 Feature 的**可用性保證**。若本地模型當機就整個 AI 功能壞掉，病患會卡在報到流程，這不可接受。

**Independent Test**：停用 OMLX 服務（或關閉 endpoint），呼叫 `POST /api/ai/triage`，驗證仍能成功取得分流結果，且 log 顯示嘗試 OMLX 失敗後切換至 Groq 的紀錄。

**Acceptance Scenarios**：

1. **Given** OMLX 與 Groq 皆可用且 `AI:Provider=Omlx`，**When** 呼叫分流，**Then** 優先使用 OMLX，log 記錄 `嘗試使用 Omlx 進行 AI 分流`。
2. **Given** OMLX 連線超時，**When** 呼叫分流，**Then** 自動切換至 Groq，最終仍回傳成功結果。
3. **Given** OMLX 與 Groq 皆失敗，**When** 呼叫分流，**Then** 回傳 `Result.Fail("AI 分流服務暫時無法使用，請手動選擇科別")`，不拋例外中斷報到流程。

---

### User Story 3 - AI 回應解析容錯（Priority: P2）

LLM 回應有時會包含 thinking 過程、markdown 程式碼區塊、或多餘文字；系統須能從混亂回應中擷取出正確的 JSON 區塊並解析，解析失敗時視為該 provider 失敗並嘗試下一個。

**Why this priority**：非思考模型大致穩定，但仍偶爾會產出非純 JSON 的文字。沒有這層容錯會讓整個功能在實際使用中頻繁 500 錯誤。

**Independent Test**：單元測試 `ParseTriageResponse` 方法，塞入各種含 thinking、markdown、或多個 JSON 物件的字串，驗證能正確擷取含 `department` 欄位的 JSON 物件。

**Acceptance Scenarios**：

1. **Given** LLM 回應含 `<think>...</think>` 再加上 markdown JSON 區塊，**When** 解析，**Then** 成功取出 `department="家醫科"`。
2. **Given** LLM 回應為無法解析的純文字，**When** 解析，**Then** 回傳 `null` 並讓 AiService fallback 至下一個 provider。

---

## Requirements

### Functional Requirements

- **FR-001**：系統必須提供 `POST /api/ai/triage` API，接收 `{clinicId: Guid, symptoms: string}` 並回傳分流結果或錯誤訊息。
- **FR-002**：系統必須從資料庫動態讀取該院所（`ClinicId`）啟用中（`IsActive=true`）的科別清單作為 AI 可選項。
- **FR-003**：AI 回應必須包含：`department`（科別名稱）、`departmentId`（對應 GUID）、`priority`（0/1/2）、`estimatedWaitMinutes`（整數）、`reasoning`（繁體中文說明）。
- **FR-004**：系統必須支援至少兩個 LLM provider，並依設定檔 `AI:Provider` 決定優先順序。
- **FR-005**：當優先 provider 失敗（HTTP 錯誤、超時、回應解析失敗）時，系統必須自動 fallback 至其他 provider。
- **FR-006**：所有 LLM 呼叫必須走統一的 `ILlmClient` 抽象介面，新增 provider 只需實作該介面並註冊至 DI 容器。
- **FR-007**：OMLX 與 Groq 必須採用 OpenAI 相容格式（`/v1/chat/completions`）以共用回應模型。
- **FR-008**：當所有 provider 皆失敗時，系統必須回傳可讀錯誤訊息，不得中斷呼叫端流程（不拋出未處理例外）。
- **FR-009**：系統必須記錄每次 provider 嘗試、成功、失敗的 log，方便除錯與成本分析。

### Non-Functional Requirements

- **NFR-001**：使用本地 OMLX（Qwen2.5-7B-Instruct 非思考模型）時，單次分流回應時間須 ≤ 5 秒。
- **NFR-002**：LLM 請求溫度設為 0.2（分流）/ 0.1（指令），確保回應穩定可解析。
- **NFR-003**：`max_tokens` 限制為 256，避免冗長回應拖慢響應時間。
- **NFR-004**：OMLX 作為預設 provider，優先保護病患症狀資料不離開本地網路。
- **NFR-005**：Groq API Key 須透過 `IConfiguration` 讀取，不得 hardcode 在程式碼中。

### Key Entities

- **TriageRequest**：分流請求，包含 `ClinicId`（院所識別）與 `Symptoms`（病患症狀描述文字）。
- **TriageResponse**：分流結果，包含 `Department`（科別名稱）、`DepartmentId`（科別 GUID，可為 null）、`Priority`（0=一般, 1=優先, 2=緊急）、`EstimatedWaitMinutes`、`Reasoning`。
- **DepartmentInfo**：傳給 LLM 的精簡科別資料（`Id`, `Name`）。
- **LlmMessage / LlmRequest / LlmResponse**：LLM 呼叫的統一抽象格式，與具體 provider 解耦。

## Success Criteria

- **SC-001**：病患輸入 10 組常見症狀（感冒、腸胃炎、胸痛、過敏、受傷等），AI 建議科別正確率 ≥ 80%。
- **SC-002**：OMLX 單次分流 P95 回應時間 ≤ 3 秒。
- **SC-003**：fallback 機制可在 OMLX 服務停機時，於 10 秒內自動切換至 Groq 並成功回應。
- **SC-004**：所有 provider 失敗時，API 仍能回傳 HTTP 200 + `Result.Fail`，不可產生 500 錯誤。

## Out of Scope

- **不做**醫療診斷（只做科別建議，明確在 system prompt 標示「你不是醫師」）。
- **不做**症狀的結構化抽取（如體溫數值、症狀持續天數），純以自然語言描述。
- **不做**分流結果的歷史回饋學習機制（未來 Phase 可考慮）。
- **不做**跨院所分流（每次呼叫僅使用單一 `ClinicId` 的科別）。
