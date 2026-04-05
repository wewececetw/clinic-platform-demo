# Tasks: 分支整理與工作流重構

**Feature**: 006-branch-reorg | **Status**: 待執行
**Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)

## Phase 1: 前置檢查與收斂

- [ ] T001 Commit 當前未提交的檔案（CLAUDE.md 工作流規則 + scripts/branch-diff.sh + docs/branch-sync-analysis.md）
- [ ] T002 確認 005-signalr 分支工作完成（post-impl 檢查：dotnet build 0 錯誤、無未 commit 變更）

## Phase 2: 005-signalr → main 合併（US1）

- [ ] T003 [US1] 切換到 main 分支並 pull 最新（`git checkout main && git fetch origin`）
- [ ] T004 [US1] 執行 merge --no-ff 將 005-signalr 合入 main（`git merge --no-ff 005-signalr -m "merge: 005-signalr SignalR 即時通訊推播"`）
- [ ] T005 [US1] 驗證 merge 後 main 建置成功（`dotnet build backend/src/ClinicPlatform.WebAPI/`，應 0 錯誤）
- [ ] T006 [US1] 建立 006-branch-reorg 分支並 cherry-pick 已有 spec/plan/tasks（若需要）或改在 main 上直接操作 ← 評估後決定
- [ ] T007 [US1] push main 到 origin（`git push origin main`）

## Phase 3: 舊 demo 備份（US1 的前置安全網）

- [ ] T008 [US1] 建立 demo-archive 分支保留舊 demo 歷史（`git checkout demo && git checkout -b demo-archive && git push origin demo-archive`）
- [ ] T009 [US1] 建立 demo-legacy-v1 tag 標記舊 demo HEAD（`git tag demo-legacy-v1 demo && git push origin demo-legacy-v1`）

## Phase 4: 抽取 demo overlay（US2）

- [ ] T010 [P] [US2] 建立 demo-overlay/ 目錄結構（`main/demo-overlay/{frontend,backend,root}/`）
- [ ] T011 [P] [US2] 從 demo 複製 frontend/src/App.vue 到 demo-overlay/frontend/src/App.vue（導航列版本）
- [ ] T012 [P] [US2] 從 demo 複製 frontend/src/style.css 到 demo-overlay/frontend/src/style.css（淺色醫療主題）
- [ ] T013 [P] [US2] 從 demo 複製 frontend/src/views/pharmacy/Queue.vue 到 demo-overlay/frontend/src/views/pharmacy/Queue.vue（Mock 資料版本）
- [ ] T014 [P] [US2] 從 demo 複製 README.md 到 demo-overlay/root/README.md（作品集介紹）
- [ ] T015 [US2] 手動編輯 demo-overlay/backend/WorkflowEngine.cs：以 main 的最新版為底（含 notifier 推播），移除條件引擎邏輯（DataSelection 等 -90 行）
- [ ] T016 [P] [US2] 建立 demo-overlay/remove-list.txt：列出 demo 要移除的路徑（specs/、.specify/、.claude/commands/speckit.*、docs/branch-sync-analysis.md、docs/sync-checklist.md、DEMO.md）
- [ ] T017 [US2] 撰寫 demo-overlay/README.md：說明 overlay 結構、各檔案用途、更新時機

## Phase 5: 撰寫 sync-to-demo.sh（US4）

- [ ] T018 [US4] 建立 scripts/sync-to-demo.sh 骨架（shebang、set -euo pipefail、彩色輸出函式）
- [ ] T019 [US4] 實作前置檢查段：working tree 乾淨、當前 main 分支、origin remote 存在
- [ ] T020 [US4] 實作 demo 重建段：`git branch -D demo` + `git checkout -b demo`
- [ ] T021 [US4] 實作 overlay 套用段：讀取 remove-list.txt 刪除私有路徑 + 複製 demo-overlay 檔案到對應位置 + 刪除 demo-overlay/ 本身
- [ ] T022 [US4] 實作 commit 段：`git add -A && git commit -m "chore: demo overlay 套用 (YYYY-MM-DD)"`
- [ ] T023 [US4] 實作 push 段：force push origin + 檢查 public 存在後 force push public demo:main
- [ ] T024 [US4] 實作收尾段：切回 main 分支 + 顯示成功訊息
- [ ] T025 [US4] chmod +x scripts/sync-to-demo.sh 並測試跑一次 dry-run（加 `--dry-run` flag 只印不執行）

## Phase 6: 實際重建 demo（US3）

- [ ] T026 [US3] 執行 `./scripts/sync-to-demo.sh`（不加 dry-run，實際重建）
- [ ] T027 [US3] 驗證 demo 分支內容：執行 `./scripts/branch-diff.sh files` 確認 src 實質差異 ≤ 5 個檔案
- [ ] T028 [US3] 驗證 demo 建置成功：`git checkout demo && dotnet build backend/src/ClinicPlatform.WebAPI/`
- [ ] T029 [US3] 驗證 demo 前端啟動：`cd frontend && npm run dev` + 瀏覽器確認淺色主題、導航列、Mock 資料正常
- [ ] T030 [US3] 驗證 SignalR 功能在 demo 運作：觸發報到/叫號 → 觀察前端收到事件
- [ ] T031 [US3] push demo 到 origin + public（若已透過 sync 腳本完成則跳過）

## Phase 7: 文件更新與收尾

- [ ] T032 [P] 更新 docs/sync-checklist.md：標記為「已重構，改用 sync-to-demo.sh」
- [ ] T033 [P] 更新 docs/branch-sync-analysis.md：在文末加上「2026-04-06 重構結果」區塊
- [ ] T034 刪除過時的 CLAUDE.md 「分支內容差異」區塊內容，改為指向 demo-overlay/README.md
- [ ] T035 Commit 所有文件更新到 main
- [ ] T036 驗證 SC-006：在 main 做一個無關小改動 → 執行 sync-to-demo.sh → 確認 demo 自動更新

## Dependencies

- **Phase 1 → Phase 2**：必須先 commit 當前變更才能 merge
- **Phase 2 → Phase 3**：main 已有 SignalR 才值得備份（舊 demo 不含 SignalR）
- **Phase 3 → Phase 4**：備份後才能動 demo 內容（抽 overlay 會需要 checkout demo）
- **Phase 4 → Phase 5**：overlay 內容存在才能寫 sync 腳本引用
- **Phase 5 → Phase 6**：腳本寫完才能跑
- **Phase 6 → Phase 7**：demo 重建成功才更新文件

## Parallel Execution Examples

### Phase 4 可平行
T010（建目錄）→ 然後 T011+T012+T013+T014+T016 可同時做（都是獨立檔案）
T015 WorkflowEngine.cs 需要手動編輯，序列執行

### Phase 7 可平行
T032 + T033 不同檔案可同時做

## Implementation Strategy

**MVP 範圍**：Phase 1-3 + Phase 6（手動重建）
- Phase 4-5 的 overlay + 腳本可延後做，先用手動方式重建 demo 收斂現況
- 但若要實踐「新工作流」，Phase 4-5 是關鍵

**推薦執行順序**：
1. **立即**：Phase 1-2（收斂 005-signalr）
2. **立即**：Phase 3（備份保險）
3. **本次會話**：Phase 4-5（建立 overlay + 腳本）
4. **驗證**：Phase 6-7（實際重建 + 文件）

**任務總數**：36 個（含 P 可平行）

## 關鍵決策執行點

- **T004 merge --no-ff**：不可改為 rebase，會打亂 005-signalr 的 commit hash
- **T008-T009 備份**：一定要先備份再動 demo（防意外）
- **T015 WorkflowEngine.cs**：這是唯一需要**人工判斷**的 overlay 檔案（其他都直接複製）
- **T026 force push**：這是 point of no return，執行前確認備份完成
