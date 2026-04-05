# CLAUDE.md

## 語言規範
- 所有回覆、註解、commit 訊息、文件一律使用**繁體中文**

## Commit 規範
- 每次 git commit 訊息結尾加上：
  Co-Authored-By: Barron <wewececetw@gmail.com>
- **不要**加上 Claude 的 Co-Authored-By，只留 Barron

## 分支與開發工作流（重要）

### 分支定位
- **main**：單一真相來源（SSOT），所有商業邏輯、完整功能、SDD 規格文件
  - Remote：`origin`（wewececetw/hospital，私有）
- **demo**：main 的展示投影，**不是獨立開發分支**
  - 內容 = main 程式碼 - 私有文件（specs/、.specify/）+ 展示包裝（淺色主題、Mock 資料）
  - Remote：`origin` + `public`（wewececetw/clinic-platform-demo，公開作品集）
- **feature branch**（如 005-signalr）：從 main 長出，完成後 merge 回 main

### 工作流原則（必守）

1. **開發主力永遠在 main 或 feature branch**
   - ✅ 寫 spec/plan/tasks/implement 都在 feature branch
   - ✅ 調 bug、重構、測試都在 main 或 feature branch
   - ❌ **絕對不在 demo 分支直接開發**（過去的錯誤，造成兩邊歷史分叉）

2. **demo 是 build artifact，不是開發分支**
   - demo 只能透過 `sync-to-demo.sh` 從 main 重建
   - 任何直接在 demo commit 功能邏輯都是錯的

3. **推版才同步**（非每次 main 變動就同步）
   - 觸發時機：面試前、作品集更新、展示 milestone
   - 執行：`./scripts/sync-to-demo.sh`
   - 之後：`git push origin demo && git push public demo:main`

4. **demo overlay 內容固定**
   - 淺色醫療主題（style.css）
   - 導航列 + Mock 資料（App.vue、各 views）
   - 移除 specs/、.specify/、.claude/commands/speckit.*
   - README.md（作品集介紹）

### 新功能標準流程

```
新功能需求
  ↓
git checkout main && git checkout -b NNN-feature-name
  ↓
撰寫 specs/NNN-feature-name/spec.md → plan.md → tasks.md
  ↓
實作 + 測試
  ↓
git checkout main && git merge NNN-feature-name
  ↓
push main
  ↓
（有推版需求才執行）./scripts/sync-to-demo.sh
```

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

## 分支內容差異（main vs demo）

**main 有但 demo 沒有**（私有/開發工具）：
- `specs/` 全部 SDD 規格文件
- `.specify/` templates & scripts
- `.claude/commands/speckit.*.md`
- `docs/branch-sync-analysis.md`、`docs/sync-checklist.md`（內部文件）

**demo 有但 main 沒有**（展示包裝）：
- 淺色醫療主題 `frontend/src/style.css`
- 導航列 `frontend/src/App.vue` + Mock 資料（各 views）
- `README.md` 作品集介紹
- WorkflowEngine.cs 精簡版（移除條件引擎）

**兩邊功能內容應該一致**（透過 sync-to-demo.sh 保證）
