# AI 功能發展藍圖

## 設計理念
以 AI 驅動門診流程智慧化，從「人工操作」進化為「AI 輔助 + 人工確認」模式。
採用漸進式整合策略，每個 Phase 獨立可用、逐步疊加。

---

## Phase 1：智慧症狀分流 ✅ 已完成

病患報到時描述症狀，AI 即時建議最適科別。

**使用者體驗**
```
病患輸入：「頭痛三天、有點發燒、喉嚨痛」
     ↓
AI 回傳：建議掛「內科」、優先度一般、預估等候 15 分
     ↓
一鍵報到，自動帶入科別
```

**技術架構**
```
前端（Vue 3）→ POST /api/ai/triage → AiService
                                        ↓
                              ┌── OMLX（本地 Ollama）← 優先
                  ILlmClient ─┤
                              └── Groq API ← 備援
```

- **ILlmClient 抽象層**：統一 OpenAI 相容格式，provider 可插拔
- **TriagePromptBuilder**：動態組裝 Prompt（注入院所科別清單）
- **JSON 回傳解析**：容錯處理，確保穩定

---

## Phase 2：自然語言指令 🔜 下一步

醫護人員用自然語言操作系統。

```
護理師：「叫下一位」     → QueueService.CallNextAsync()
醫師：「開普拿疼三天份」 → PrescriptionService.CreateAsync()
```

**技術方案**：LLM Function Calling 對接現有 Service API

---

## Phase 3：AI 輔助看診（CDSS）📋 規劃中

臨床決策支援 — 症狀分析 + 用藥建議 + 交互作用檢查。

**技術方案**：RAG（檢索增強生成）+ 醫療知識庫向量化

---

## Phase 4：智慧排程 📋 規劃中

預測候診時間、動態優化叫號順序。

**技術方案**：ML.NET 時序預測模型

---

## Phase 5：多模態 AI 📋 規劃中

語音指令（Whisper）+ 影像辨識（Vision Model）。

---

## 模型策略

| 方案 | 優點 | 適用場景 |
|------|------|----------|
| **OMLX（Ollama 本地）** | 免費、離線、隱私安全 | 開發、內網部署 |
| **Groq（免費 tier）** | 快速推理、高品質 | Demo、小規模部署 |
| **自架 vLLM** | 可控、可微調 | 正式商業部署 |

所有方案共用 `ILlmClient` 介面，切換只需改設定檔。
