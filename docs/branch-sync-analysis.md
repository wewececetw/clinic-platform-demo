# 分支同步分析報告

**分析日期**：2026-04-05
**分析對象**：main ↔ demo ↔ 005-signalr

---

## 1. 現況盤點

### 1.1 分支拓樸

```
                                                 ← 現在在這
                         005-signalr (5 commits)
                              ↑
                             main (商業版, 私有)
                              ↑
         共同祖先 850c428 ──┤
                              ↓
                             demo (作品集, public)
```

**共同祖先**：`850c428 feat: 建立 Vue 3 前端`

### 1.2 Commit 獨有統計

| 分支 | 獨有 commits | 主要內容 |
|-----|-------------|---------|
| main | 7 | AI 分流、自然語言指令、Qwen2.5 優化、商業文件、Spec Kit 規格 |
| demo | 8 | 同上 4 項 + 淺色主題、導航列、Mock 資料、條件引擎精簡 |
| 005-signalr | 5 | SignalR spec/plan/tasks/實作、同步清單、DEMO.md |

### 1.3 檔案層差異（main vs demo）

| 類型 | 數量 | 說明 |
|-----|-----|------|
| **demo 移除** | 32 檔案 / -5714 行 | specs/、.specify/、.claude/commands/、README 等 |
| **demo 新增** | 11 檔案 / +356 行 | 淺色主題、Mock 資料、管理員後台 |
| **真正不同內容** | **1 檔案** | WorkflowEngine.cs（demo 精簡 -90 行） |

**關鍵觀察**：**功能層程式碼幾乎完全一致**。真正的分歧只有：
1. `WorkflowEngine.cs` — demo 拿掉條件引擎
2. UI 主題、Mock 資料 — demo 展示包裝
3. 文件類檔案 — demo 移除 SDD 規格

---

## 2. 不同步的根本原因

### 問題 1：平行 commit 造成歷史分叉（主因）

```
過去的開發模式：
  實作 AI 分流 → commit 到 main (dc7f0a3)
               → 切到 demo → 重做一次 → commit (ab21fc6)
```

**後果**：功能一樣，但 git 認為是兩個不同 commit。無法 merge、無法判斷「功能是否同步」。

### 問題 2：商業邏輯與展示包裝混在 demo 分支

demo 分支同時做了兩件事：
- 同步 main 的新功能（AI 分流、自然語言指令...）
- 做展示化包裝（淺色主題、Mock 資料、移除商業邏輯）

這兩件事綁在同一條分支，無法用 git merge/rebase 乾淨分離。

### 問題 3：沒有「同步檢查點」機制

沒有明確的同步時機定義，變成「想到再同步」，漂移累積。

---

## 3. 三個解決方案

### 解法 A：Cherry-pick 策略（短期，1 小時內）

**適用場景**：需要趕快讓 demo 有新功能（面試前、作品集 review）

**流程**：
```bash
# 每次 main 有新功能 commit 後
git checkout demo
git cherry-pick <main-feat-hash>
# 手動解衝突（UI 主題、Mock 資料）
git push origin demo
git push public demo:main
```

**優點**：
- ✅ 快速、可逆
- ✅ 不需要改變現有工作流
- ✅ commit 歷史乾淨

**缺點**：
- ❌ 每次新功能都要手動做一次
- ❌ 衝突需要人判斷
- ❌ 仍然會有功能同步延遲

### 解法 B：Overlay 模式（中期，初次 4-6 小時）

**核心想法**：把 demo 的「展示包裝」抽成獨立 commit set，永遠套在 main 之上。

**現況 demo 的展示包裝 commits**：
- `1d6dea3` 移除 workflow 條件引擎（WorkflowEngine.cs）
- `7de4aa7` 淺色醫療主題（style.css）
- `18d5421` 導航列 + Mock 資料（App.vue、各 views）

**重構步驟**：

1. **提取 overlay commits**
   ```bash
   git checkout demo
   # 把上面 3 個 commits 打包成 demo-overlay 標籤
   git tag demo-overlay-v1
   ```

2. **建立同步腳本 `scripts/sync-to-demo.sh`**
   ```bash
   #!/bin/bash
   # 1. 從 main 建新分支
   git checkout main -b demo-new
   # 2. 移除 main 獨有檔案（specs/、.specify/）
   rm -rf specs/ .specify/ .claude/commands/speckit.*
   git commit -am "chore: demo 分支移除規格文件"
   # 3. cherry-pick overlay commits
   git cherry-pick demo-overlay-v1~2..demo-overlay-v1
   # 4. force push
   git branch -D demo
   git branch -m demo
   git push origin demo --force
   git push public demo:main --force
   ```

3. **未來工作流**：
   ```
   新功能 → 在 main 開發並 merge
         ↓
   執行 ./scripts/sync-to-demo.sh
         ↓
   demo 自動 rebuild，永遠 = main + overlay
   ```

**優點**：
- ✅ 一勞永逸
- ✅ demo 永遠跟 main 同步
- ✅ 自動化，不依賴記性

**缺點**：
- ❌ Force push demo（歷史重寫）
- ❌ 初次重構有工作量
- ❌ demo 的 commit 歷史對外觀看不漂亮（都是 cherry-pick）

### 解法 C：Monorepo + Build Pipeline（長期，改造型）

**做法**：demo 不再是 git 分支，而是 main 的建置產出。

```bash
main 的 scripts/build-demo.sh:
  1. 複製 main 到 /tmp/demo-build
  2. 刪除 specs/、.specify/、.claude/commands/speckit.*
  3. 套用 demo 的 UI 主題 patch
  4. push 到 public remote 的 main branch
```

**優點**：
- ✅ 從根本消除分支同步問題
- ✅ demo 自動永遠最新
- ✅ 只需維護 main + overlay patch

**缺點**：
- ❌ 要寫 build pipeline
- ❌ 改變現有工作流最多
- ❌ demo 失去「可獨立開發」特性

---

## 4. 建議

### 當下（005-signalr 完成前）

**不動 demo**。理由：
- 005-signalr 還在開發分支，沒 merge 回 main
- 動 demo 會讓問題範圍擴大

### 短期（005-signalr merge 回 main 後）

**採用解法 A（Cherry-pick）**，理由：
- 成本最低、一次性
- 只需 cherry-pick 1 個 commit（`3b1f8c7 feat: 實作 SignalR...`）
- 預期衝突點：`useSignalR.ts`、`Queue.vue`（demo 的淺色主題 CSS 會衝突）

### 中期（累積 3-5 個新功能後）

**升級到解法 B（Overlay）**，理由：
- 每次 cherry-pick 的成本會累積
- 當漂移到某個臨界點，重構一次更划算
- 臨界點建議：下次有 3 個新功能待同步時

### 長期（解法 C）

暫不建議。除非專案規模擴大、有多人開發需要。

---

## 5. 005-signalr 特定的同步考量

### 檔案異動預覽

005-signalr 會帶以下檔案進 demo（假設走解法 A）：

| 新增檔案 | 預期衝突風險 |
|---------|------------|
| `backend/.../Notifications/INotificationPublisher.cs` | 無（新檔案） |
| `backend/.../Notifications/SignalRNotificationPublisher.cs` | 無 |
| `backend/.../Hubs/GroupNames.cs` | 無 |
| `backend/.../Hubs/VisitHub.cs` | 低（demo 可能沒改過此檔） |
| `backend/.../Services/QueueService.cs` | 低（demo 用 main 版本） |
| `backend/.../Services/VisitService.cs` | 低 |
| `backend/.../Services/CheckInService.cs` | 低 |
| **`backend/.../Services/WorkflowEngine.cs`** | **高（demo 已精簡此檔）** |
| `backend/.../Program.cs` | 中（CORS 改動可能衝突） |
| `frontend/src/composables/useSignalR.ts` | 中（重寫） |
| **`frontend/src/views/patient/Queue.vue`** | **高（demo 有淺色主題）** |

### 衝突解決策略

**WorkflowEngine.cs**：
- main 版含條件引擎（90 行），+ notifier 推播
- demo 版精簡（無條件引擎）
- **解法**：demo 接受 main 的 notifier 呼叫，但保留 demo 的簡化分支邏輯

**Queue.vue**：
- main 版：暗色主題 + SignalR 整合
- demo 版：淺色主題 + 原 SignalR 骨架
- **解法**：保留 demo 的淺色 CSS 變數，套用 main 的 script 邏輯與 template

---

## 6. 監控指標

### 漂移健康度檢測

每月執行一次：

```bash
# 1. 功能差異（僅 src 檔案，排除文件）
git diff main..demo --stat -- backend/src/ frontend/src/

# 2. commit 落差
echo "main 領先 demo: $(git log main --not demo --oneline | wc -l) commits"

# 3. 上次同步時間
git log demo --grep="同步\|sync" -1 --format="%ai %s"
```

**紅線指標**：
- src 檔案差異 > 5 個 → 該同步了
- main 領先 demo > 3 個 feature commit → 該同步了
- 超過 2 週未同步 → 該同步了

---

## 7. 行動項（未來執行）

- [ ] 005-signalr merge 回 main 後，執行解法 A cherry-pick
- [ ] 當累積 3 個新功能未同步時，評估升級解法 B
- [ ] 每月執行一次漂移健康度檢測
- [ ] 更新 CLAUDE.md 加入「新功能只在 main 開發」明文規則
