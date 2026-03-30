# AI 智慧分流系統 — Analysis

## 現有系統盤點

### 已有的基礎設施
- 5 個 Service（CheckIn, Queue, Visit, Prescription, Workflow）
- 2 個科別（內科、一般外科）已在 SeedData
- 報到流程支援 OTP / QR Code / 手動三種方式
- 前端報到頁已有 tab 切換機制

### 需要擴充的部分
- 後端：新增 AI Feature 層（IAiService + 實作）
- 後端：新增 AiController
- Infrastructure：新增 LLM Client（OMLX + Groq 雙 provider）
- 前端：報到頁加第四個 tab「AI 分流」
- 設定：appsettings.json 加 AI 相關設定

## 技術分析

### LLM 呼叫架構
```
IAiService（Application 層介面）
    ↓
AiService（Infrastructure 層實作）
    ↓
ILlmClient（抽象介面）
    ├── OmlxLlmClient（本地 Ollama，預設）
    └── GroqLlmClient（Groq API，備援）
```

### Prompt 設計策略
- 分流 Prompt：給定科別清單，要求 JSON 格式回傳
- 指令 Prompt：給定可用的 action 清單，用類似 Function Calling 的方式回傳
- 系統 Prompt 強調「僅供參考，不做醫療診斷」

### OMLX（Ollama）整合
- 端點：`http://localhost:11434/v1/chat/completions`（OpenAI 相容格式）
- 模型：使用者本地可用模型（如 qwen2.5、llama3.1）
- 不需 API Key

### Groq 整合
- 端點：`https://api.groq.com/openai/v1/chat/completions`
- 模型：llama-3.3-70b-versatile
- 需要 API Key（免費 tier 足夠 demo）
- 每分鐘 30 requests 限制

## 風險評估
- 本地模型回應品質可能不穩定 → 用 JSON schema 約束回傳格式
- 回應速度依機器效能 → 前端加 loading 動畫和 timeout
- 中文支援度 → Qwen 系列對中文支援最佳
