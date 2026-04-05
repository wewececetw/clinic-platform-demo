# Implementation Plan: 分支整理與工作流重構

**Branch**: `006-branch-reorg` | **Date**: 2026-04-06 | **Spec**: [spec.md](./spec.md)

## Summary

以 `git merge --no-ff` 將 `005-signalr` 合併回 main，建立一次性 `demo-archive` 分支保留舊 demo 歷史，然後用「main 為基底 + overlay 套用」的方式重建 demo。overlay 透過 git 的「patch 擷取」方式（取出 demo 相對於 main 的精準 diff）保存在 main 的 `demo-overlay/` 目錄，未來由 `scripts/sync-to-demo.sh` 自動套用。

## Technical Context

**工具**：git 2.x、bash 5.x
**關鍵操作**：`git merge --no-ff`、`git tag`、`git apply`、`git push --force`
**Target Platform**：macOS（zsh）+ GitHub remote
**Constraints**：
- 舊 demo 有 8 個 commits 要保留備份（demo-archive 分支）
- 重建時必須 force push（不可避免）
- overlay 必須**可重放**（每次重建 demo 結果一致）

## Constitution Check

（依新的 CLAUDE.md 工作流原則檢核）

- ✅ **繁體中文**：所有 commit message、log、腳本註解使用繁中
- ✅ **main = SSOT**：重構後 demo 成為 main 的投影
- ✅ **demo 是 build artifact**：透過 sync 腳本重建，不直接開發
- ✅ **推版才同步**：sync-to-demo.sh 為手動觸發

## Design Decisions

### 決策 1：overlay 儲存格式 — patch 檔 vs 檔案複製

**選項 A**：`demo-overlay/` 直接存完整檔案（style.css、App.vue 等）
**選項 B**：`demo-overlay/patches/*.patch` 儲存 git diff patch
**採用**：**選項 A（完整檔案複製）** ← **更務實**

**理由**：
- patch 檔會因 main 的檔案更新而失效（需要手動 rebase）
- 完整檔案複製更直觀、易維護
- 套用時用 `cp` 覆蓋，簡單可靠
- 缺點（檔案需手動更新）可接受，因為 overlay 內容變動頻率低

### 決策 2：WorkflowEngine.cs 精簡版處理

**問題**：demo 版 WorkflowEngine.cs 移除條件引擎（-90 行），但 005-signalr 在 main 版加了 notifier 推播。
**決策**：**demo overlay 的 WorkflowEngine.cs 要包含 notifier 呼叫**（保留 SignalR 功能），但移除條件引擎邏輯。

**實作方式**：從當前 main（已 merge SignalR）複製 WorkflowEngine.cs，手動移除條件引擎部分，存入 `demo-overlay/backend-simplified/WorkflowEngine.cs`。

### 決策 3：sync-to-demo.sh 執行方式

**流程**：
```
1. 檢查前置條件
   - working tree 乾淨
   - 當前分支是 main
   - remote origin 和 public 存在
2. 從 main 建新 demo
   - git branch -D demo（刪除舊本地分支）
   - git checkout -b demo
3. 套用 overlay
   - 移除私有目錄（specs/、.specify/、.claude/commands/speckit.*）
   - 複製 demo-overlay/ 下所有檔案到對應位置
   - 移除 demo-overlay/ 目錄本身（demo 不需要這個目錄）
4. commit
   - git add -A
   - git commit -m "chore: demo overlay 套用 ($(date +%Y-%m-%d))"
5. push
   - git push origin demo --force
   - git push public demo:main --force（若 public 存在）
6. 切回 main
```

**失敗處理**：每步驟有錯就中止，不自動 rollback（讓使用者手動檢查）。

### 決策 4：demo-archive 分支定位

**作用**：保留舊 demo 歷史（8 個 commits）供未來考古
**位置**：local + origin（**不推 public**，避免污染作品集）
**後續**：**唯讀**，不繼續開發

### 決策 5：merge 順序與時機

```
1. 確認 005-signalr 工作完成（已完成）
2. merge --no-ff 005-signalr → main
3. push main 到 origin
4. 建立 demo-archive 分支保留舊 demo
5. 重建 demo（透過 sync-to-demo.sh）
6. push demo 到兩個 remote
```

**關鍵**：**必須先 merge 再重建 demo**，否則新 demo 會缺少 SignalR。

## Project Structure

### 新增檔案/目錄

```
main/
├── demo-overlay/                          # 新增（overlay 資源）
│   ├── README.md                          # overlay 說明
│   ├── frontend/                          # 前端展示包裝
│   │   ├── src/
│   │   │   ├── App.vue                    # 導航列版本
│   │   │   ├── style.css                  # 淺色醫療主題
│   │   │   └── views/
│   │   │       └── pharmacy/Queue.vue     # Mock 資料版本
│   ├── backend/                           # 後端精簡包裝
│   │   └── WorkflowEngine.cs              # 移除條件引擎版本
│   ├── root/                              # 根目錄額外檔案
│   │   ├── README.md                      # 作品集介紹
│   │   ├── .gitignore                     # demo 用 gitignore
│   │   └── CLAUDE.md                      # demo 精簡版（若需要）
│   └── remove-list.txt                    # 要移除的路徑清單
│
└── scripts/
    └── sync-to-demo.sh                    # 自動化同步腳本
```

### 修改檔案

- `CLAUDE.md` — 新增分支整理的歷史紀錄（已做）
- `docs/sync-checklist.md` — 更新為新工作流版本
- `docs/branch-sync-analysis.md` — 標記「已重構」

## Risks & Mitigations

| 風險 | 影響 | 緩解 |
|-----|------|-----|
| Force push 覆蓋其他人正在參考的 demo | 對方需 reset | 單人開發專案，風險低 |
| overlay 檔案與 main 對應檔案不同步（API 變更） | demo 建置失敗 | 每次 main API 變更後手動更新 overlay；sync 腳本後跑 `dotnet build` 驗證 |
| WorkflowEngine.cs 精簡版缺少 notifier 呼叫 | SignalR 不觸發 | 決策 2 明確要求 demo 版也含 notifier |
| public remote 未設定 | push 失敗 | 腳本檢查，不存在則跳過並警示 |
| demo-overlay/ 目錄被誤 commit 到 demo 分支 | demo 內容污染 | sync 腳本套用後明確刪除該目錄再 commit |

## Open Questions

無（已透過 Clarifications 階段釐清）。

## Success Verification

- **SC-001**（main 增加 5 commits）：`git log main --oneline | wc -l` 前後對比
- **SC-002**（src 差異 ≤ 5）：執行 `./scripts/branch-diff.sh files`
- **SC-003**（腳本 < 30s）：`time ./scripts/sync-to-demo.sh`
- **SC-005**（dotnet build 零錯誤）：重建 demo 後執行 `dotnet build`
- **SC-006**（單一指令同步）：未來 main 有新 commit 後執行 `./scripts/sync-to-demo.sh`
