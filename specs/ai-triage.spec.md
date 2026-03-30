# AI 智慧分流系統 — Spec

## 目標
為門診平台整合 AI 能力，提供智慧症狀分流與自然語言操作介面。

## 功能範圍

### Phase 1：智慧症狀分流（病患端）
- 病患報到時可輸入自然語言描述症狀
- AI 分析症狀後回傳：建議科別、優先度、預估等候時間
- 結果可直接帶入報到流程（自動選科、設定優先度）

### Phase 2：自然語言指令（醫護端）
- 護理師：「叫下一位」「把 3 號過號」
- 醫師：「開普拿疼 500mg TID 三天份」「完成看診，需要領藥」
- 用 Tool Use / Function Calling 對接現有 Service API

## 技術選型
- **主要模型**：本地 OMLX（Ollama） — 完全免費、離線可用
- **備援模型**：Groq API（免費 tier） — 更快推理、Tool Use 支援佳
- **模型切換**：統一 OpenAI SDK 格式，base URL 切換即可
- **後端整合**：ASP.NET Core + HttpClient 呼叫 LLM API

## 系統限制
- 不做醫療診斷，僅做科別建議與分流
- 明確告知使用者「此為 AI 建議，僅供參考」
- 本地模型優先，降低成本和隱私風險

## API 端點設計

### POST /api/ai/triage
- 輸入：`{ clinicId, symptoms }` （症狀描述文字）
- 輸出：`{ department, departmentId, priority, estimatedWaitMinutes, reasoning }`

### POST /api/ai/command
- 輸入：`{ clinicId, userId, role, command }` （自然語言指令）
- 輸出：`{ action, params, result, message }`

## 前端變更
- 報到頁新增「AI 智慧分流」tab
- 輸入症狀 → 顯示 AI 建議 → 一鍵報到
- 醫護端加入指令輸入框（Phase 2）
