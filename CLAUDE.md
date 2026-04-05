# CLAUDE.md

## 語言規範
- 所有回覆、註解、commit 訊息、文件一律使用**繁體中文**

## Commit 規範
- 每次 git commit 訊息結尾加上：
  Co-Authored-By: Barron <wewececetw@gmail.com>
- **不要**加上 Claude 的 Co-Authored-By，只留 Barron

## 分支與部署策略
- main 分支 → `origin`（wewececetw/hospital，私有）
- demo 分支 → 同時推 `origin` 和 `public`（wewececetw/clinic-platform-demo，公開作品集）
- 每次 demo 分支有變更推送時，**務必也推到 public remote**：`git push public demo:main`

## 專案概述
- 醫療院所門診流程管理平台（Multi-tenant SaaS）
- 後端：ASP.NET Core + EF Core + MySQL（Clean Architecture）
- 前端：Vue 3 + Vite（PWA）
- 快取：Redis
- 即時通訊：SignalR

## 技術決策
- MySQL provider 使用 MySql.EntityFrameworkCore 10.0.1（非 Pomelo，因 .NET 10 相容性）
- Workflow Engine 用有向圖建模（workflow_definitions → workflow_steps → workflow_transitions）
- 候診佇列 Redis 為主、MySQL queue_entries 為持久化備份
- AI 整合：OMLX（Ollama）本地優先、Groq 免費 tier 備援，統一 OpenAI 相容格式

## 開發方法論
- 使用 **Spec Kit** 進行 Spec-Driven Development
- 流程：/speckit.constitution → /speckit.specify → /speckit.clarify → /speckit.plan → /speckit.tasks → /speckit.implement
- 選用增強：/speckit.analyze（跨產出物一致性）、/speckit.checklist（品質檢查）
- 規格文件存放於 `specs/` 目錄

## AI 功能方向（main — 商業版）

### Phase 1：智慧症狀分流 ✅ 已完成基礎架構
- 病患報到時輸入症狀，AI 建議科別 + 優先度 + 預估等候
- LLM 抽象層（ILlmClient）支援多 provider fallback

### Phase 2：自然語言指令
- 護理師：「叫下一位」「3 號過號」
- 醫師：「開普拿疼 500mg TID 三天份」
- 技術：LLM Function Calling / Tool Use 對接現有 Service
- 需要 CommandPromptBuilder + Action Router

### Phase 3：AI 輔助看診
- 根據症狀 + 病歷建議可能診斷與用藥參考
- 技術：RAG（檢索增強生成）+ 醫療知識庫向量化
- 需要：向量資料庫（Qdrant/Milvus）、嵌入模型、知識庫建置

### Phase 4：智慧排程優化
- 歷史資料訓練候診時間預測模型
- 動態調整叫號順序（考慮看診時長、科別特性）
- 技術：時序分析 + 預測模型，可用 ML.NET 或 Python sidecar

### Phase 5：多模態擴充
- 語音輸入指令（Web Speech API → LLM）
- 影像辨識輔助（傷口照片初步分類）
- 需要：Whisper 語音模型、Vision 模型整合

## 分支策略
- **main**：完整商業版，含條件式 Workflow Engine、SDD 規格文件、全部 AI Phase
- **demo**：展示版，供 104 作品集，含 AI 分流 + Groq 備援（展示架構能力）
- demo 移除：specs/ 設計文件、Workflow Engine 條件引擎
