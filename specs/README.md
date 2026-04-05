# Specs — 反向補文檔

本目錄使用 **Spec Kit** 方法論，針對已完成功能反向補寫規格文件（spec.md / plan.md / tasks.md），作為架構說明與技術展示用途。

## 文件列表

| # | Feature | 狀態 | 連結 |
|---|---------|------|------|
| 001 | **AI 智慧症狀分流**（雙 LLM provider fallback）| ✅ 完成 | [spec](./001-ai-triage/spec.md) / [plan](./001-ai-triage/plan.md) / [tasks](./001-ai-triage/tasks.md) |
| 002 | **條件式 Workflow Engine**（有向圖建模）| ✅ 完成 | [spec](./002-workflow-engine/spec.md) / [plan](./002-workflow-engine/plan.md) / [tasks](./002-workflow-engine/tasks.md) |
| 003 | **候診佇列管理** | ✅ 完成 | [spec](./003-queue-management/spec.md) / [plan](./003-queue-management/plan.md) / [tasks](./003-queue-management/tasks.md) |
| 004 | **AI 自然語言指令**（Action Router + 雙層權限）| ✅ 完成 | [spec](./004-nl-command/spec.md) / [plan](./004-nl-command/plan.md) / [tasks](./004-nl-command/tasks.md) |

## 為什麼是這 4 個

專案含 10+ 個功能模組（報到 / 看診 / 處方 / 管理員後台 / 前端 6 介面...），但此處**只針對差異化、有架構深度的功能**補 spec：

- **001 AI 分流** — 展示 LLM provider 抽象層與 fallback 設計
- **002 Workflow Engine** — 展示有向圖建模 + 條件引擎
- **003 候診佇列** — 展示優先度排序與原子更新策略
- **004 自然語言指令** — 展示 AI Action Router 與雙層權限

標準 CRUD（報到、處方紀錄等）與前端 UI 不納入，因規格文件化價值低。

## 文件結構

每個 feature 目錄包含：

```text
NNN-feature-name/
├── spec.md    # WHAT & WHY：User stories (P1/P2/...)、FR/NFR、Success Criteria
├── plan.md    # HOW：技術架構、資料模型、關鍵設計決策
└── tasks.md   # 任務清單（全部 ✅）+ 依賴圖 + 未來改善項目
```

## 未做的 spec（Out of Scope）

- 前端 Vue 3 介面（6 個 view）
- 報到 / 看診 / 處方 / 管理員 CRUD Service
- Multi-tenant 基礎建設
- EF Core 資料庫設計

這些功能可從 code 與 git history 直接理解，補 spec 的 ROI 低。
