# Feature Specification: 分支整理與工作流重構

**Feature Branch**: `006-branch-reorg`
**Created**: 2026-04-05
**Status**: Draft
**Type**: Chore / DevOps（非功能開發，為工作流重構）
**Input**: User description: "將 main 整理為最完整版（含 005-signalr），將 demo 重建為 main 的展示投影（透過 overlay 套用淺色主題、Mock 資料、移除私有文件）"

## 背景與動機

### 歷史問題

目前 main 和 demo 兩個分支**功能內容相同但歷史分叉**：
- AI 分流、自然語言指令、Qwen2.5 優化 — 兩邊各 commit 一次（hash 不同）
- 共同祖先：`850c428`（Vue 3 前端建立）
- main 領先 demo 7 commits、demo 領先 main 8 commits
- 預期 cherry-pick 衝突 45 檔案

### 根本原因

過去未規劃清楚工作流，同一功能在兩條分支各自實作，造成：
1. **歷史不可 merge**（git 認為是兩件事）
2. **無法判斷功能是否同步**
3. **每次新功能要做兩次**

### 新工作流（已寫入 CLAUDE.md）

- **main = SSOT**（單一真相來源）
- **demo = main 的 build artifact**（不是開發分支）
- **feature branch** → merge main → （推版時）執行 `sync-to-demo.sh` 重建 demo

## User Scenarios & Testing

### User Story 1 - 收斂當前工作到 main (Priority: P1)

將 `005-signalr` 分支的 5 個 commits（SignalR 實作）merge 回 main，讓 main 成為包含所有最新功能的完整版本。

**Why this priority**：這是後續所有操作的前置。demo 重建必須以「最新的 main」為基底，否則展示版會落後。

**Independent Test**：切到 main 後執行 `git log --oneline -10`，應看到 005-signalr 的 5 個 commits；執行後端 `dotnet build` 應 0 錯誤；啟動後端後 SignalR Hub 應可連線。

**Acceptance Scenarios**:
1. **Given** 當前在 005-signalr 分支，**When** 執行 merge 到 main **Then** main HEAD 包含 SignalR 實作
2. **Given** main 已 merge 完成，**When** 執行 `dotnet build` **Then** 建置成功
3. **Given** main 啟動後端，**When** 前端連 `/hubs/visit` **Then** SignalR 連線建立成功

---

### User Story 2 - 抽取 demo overlay 內容 (Priority: P1)

分析 demo 分支相對於 main 的**展示專屬差異**，將這些差異打包成可重放的 overlay 內容（放在 main 的 `demo-overlay/` 目錄），使 demo 可以被重建。

**Why this priority**：沒有明確的 overlay 內容，就無法自動化重建 demo。這是新工作流的核心資產。

**Independent Test**：檢查 `demo-overlay/` 目錄內含完整的展示包裝（淺色主題檔案、Mock 資料、README），執行 overlay 套用腳本後，產生的 demo 視覺效果應與原 demo 一致。

**Acceptance Scenarios**:
1. **Given** main 分支，**When** 檢查 `demo-overlay/` 目錄 **Then** 存在淺色主題 CSS、導航列 App.vue、Mock 資料 views、README.md
2. **Given** 全新的 demo 分支基於 main，**When** 套用 overlay **Then** 視覺效果與重建前的 demo 相同（淺色主題、導航列出現、Mock 資料顯示）

---

### User Story 3 - 重建 demo 為 main 的投影 (Priority: P1)

刪除目前 demo 分支，從 main 重新建立，套用 overlay，確保 demo 內容 = main 程式碼 + 固定展示包裝。

**Why this priority**：這是重構的最終目標。完成後兩邊內容同步、未來工作流可運作。

**Independent Test**：新 demo 分支對比 main 時，程式碼差異應只有：
- 移除 specs/、.specify/、.claude/commands/speckit.*
- 新增淺色主題 style.css、導航列 App.vue
- 新增 README.md

不應有任何核心商業邏輯差異（除 WorkflowEngine.cs 精簡）。

**Acceptance Scenarios**:
1. **Given** 新 demo 分支，**When** 執行 `branch-diff.sh` 比對 main **Then** src 實質差異僅包含 `style.css`、`App.vue`、`Queue.vue`（展示包裝）、`WorkflowEngine.cs`（精簡）
2. **Given** 新 demo 分支，**When** 啟動前後端 **Then** 展示流程（AI 分流、自然語言指令、SignalR）全部運作
3. **Given** 新 demo 分支，**When** push 到 public remote **Then** 公開作品集顯示最新內容

---

### User Story 4 - 建立 sync-to-demo.sh 自動化腳本 (Priority: P2)

撰寫可重複執行的 shell 腳本，未來 main 更新後執行一次就能重建 demo。

**Why this priority**：第一次重建可以手動做，但新工作流的「推版才同步」需要自動化工具。若沒有這腳本，下次又會退回手動 cherry-pick。

**Independent Test**：在 main 做一個小改動後執行 `./scripts/sync-to-demo.sh`，驗證 demo 分支自動更新並 push 成功。

**Acceptance Scenarios**:
1. **Given** main 有新 commit，**When** 執行 `./scripts/sync-to-demo.sh` **Then** demo 分支重建、overlay 套用、force push 到兩個 remote
2. **Given** 腳本執行中斷（Ctrl+C），**When** 重新執行 **Then** 從中斷處可恢復（或全部重來，不留髒資料）

---

### Edge Cases

- **未 commit 的變更**：執行重構前若 working tree 不乾淨，腳本應拒絕執行並提示
- **remote 未設定**：若 `public` remote 不存在，腳本應提示並跳過該 push
- **WorkflowEngine.cs 衝突**：main 版含 SignalR notifier + 條件引擎、demo 版精簡 — overlay 需保留 notifier 推播邏輯
- **demo 有人手動 commit**：若 demo 分支有本地未推送 commit 會被 force push 覆蓋 — 需明確警示
- **overlay patch 順序錯誤**：必須先移除私有文件再套主題（避免主題套用到還存在的 specs/）

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST 將 005-signalr 的 5 個 commits 完整保留在 main 分支（不丟失）
- **FR-002**: System MUST 在 main 建立 `demo-overlay/` 目錄，內含完整的 demo 展示包裝
- **FR-003**: System MUST 提供 `scripts/sync-to-demo.sh` 自動化腳本重建 demo
- **FR-004**: 腳本 MUST 檢查 working tree 乾淨才執行
- **FR-005**: 腳本 MUST 按順序套用 overlay：移除私有 → 套用主題 → 套用 Mock 資料 → commit
- **FR-006**: 腳本 MUST 支援 force push 到 origin 和 public 兩個 remote
- **FR-007**: 新 demo 分支 MUST 能成功啟動前後端並展示所有功能（AI 分流、指令、SignalR）
- **FR-008**: System MUST 更新 `docs/sync-checklist.md` 反映新的工作流
- **FR-009**: 重建後的 demo MUST 保留作品集介紹 README.md

### Key Entities

- **Overlay**：demo 相對於 main 的展示專屬變更集合
  - 移除層：specs/、.specify/、.claude/commands/speckit.*
  - 主題層：frontend/src/style.css（淺色醫療風）
  - 展示層：frontend/src/App.vue（導航列）、views/*.vue（Mock 資料）
  - 文件層：README.md
  - 精簡層：backend/.../WorkflowEngine.cs（移除條件引擎 -90 行）

- **Sync 腳本**：sync-to-demo.sh
  - 前置檢查、從 main 重建 demo、套用 overlay、push

## Clarifications (2026-04-06)

- **舊 demo 備份策略**：保留一份到 `demo-archive` 分支（保留 8 個 demo 獨有 commits 的歷史），但不繼續開發
- **Force push 策略**：對 origin 和 public 兩個 remote 都 force push（重建 demo 必要步驟）
- **Merge 策略**：005-signalr → main 使用 `git merge --no-ff`（保留 feature branch 歷史 + merge commit）

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: main 分支 `git log --oneline | wc -l` 數字增加 5（005-signalr 的 commits）
- **SC-002**: 執行 `branch-diff.sh files` 後，main ↔ demo 的 **src 差異檔案 ≤ 5 個**（僅 overlay 範圍）
- **SC-003**: 執行 `./scripts/sync-to-demo.sh` **總耗時 < 30 秒**（不含 push）
- **SC-004**: demo 重建後 push 到 public remote 成功、網頁 GitHub 顯示最新 commit
- **SC-005**: 重建後的 demo 分支啟動後端 `dotnet build` **0 錯誤 0 警告**
- **SC-006**: 未來新功能 merge 到 main 後，執行 sync-to-demo.sh **單一指令即可同步 demo**
