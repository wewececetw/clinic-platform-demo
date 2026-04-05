# 分支同步清單（main ↔ demo）

**最後更新**：2026-04-05
**共同祖先**：`850c428` (feat: 建立 Vue 3 前端)

## 分支定位

| 分支 | 用途 | Remote | 特色 |
|-----|-----|--------|-----|
| `main` | 私有商業完整版 | `origin` only | 完整邏輯、Spec Kit 規格、暗色主題 |
| `demo` | 104 作品集公開版 | `origin` + `public` | 精簡版、Mock 資料、淺色醫療主題 |

**同步策略**：不做 git merge，改手動功能同步（方案 A：Main-First）。新功能在 main 開發完成後，評估是否需回灌 demo 並做精簡改寫。

---

## 🔄 功能同步狀態

### ✅ 兩邊已同步（commit 各自實作過）

| 功能 | main commit | demo commit | 備註 |
|-----|-------------|-------------|-----|
| AI 智慧症狀分流（OMLX + Groq） | `dc7f0a3` | `ab21fc6` | 檔案命名/路徑一致 |
| 切換 Qwen2.5 非思考模型 | `3cdc866` | `b11c865` | 效能優化（20s → 2-3s） |
| AI 自然語言指令（Phase 2） | `25a23ee` | `f92a43a` | CommandExecutor、AI Controller |
| 商業邏輯總覽文件 | `1923643` | `524b35b` | 內容幾乎相同 |

### ⚠️ main 獨有（視情況決定是否進 demo）

| 項目 | 位置 | 建議處置 |
|-----|------|---------|
| Spec Kit 規格文件 4 份 | `specs/001~004/` | **不進 demo**（設計文件屬私有） |
| `.specify/` templates & scripts | `.specify/` | **不進 demo**（開發方法論內部工具） |
| `.claude/commands/speckit.*.md` | `.claude/commands/` | **不進 demo**（私有 workflow 命令） |
| 005-signalr spec（開發中） | `specs/005-signalr/` | **不進 demo**（規格） |
| CLAUDE.md 完整版（含 SDD 方法論） | `CLAUDE.md` | 保留差異，demo 有精簡版 |
| Workflow 條件引擎完整邏輯 | `backend/.../WorkflowEngine.cs` | **不進 demo**（商業核心） |
| 暗色主題 | `frontend/src/style.css` | **不進 demo**（UI 區隔） |

### ⚠️ demo 獨有（不應回灌 main）

| 項目 | 位置 | 原因 |
|-----|------|-----|
| 導航列 + 淺色醫療主題 | `frontend/src/App.vue`, `style.css` | demo 展示包裝 |
| Mock 資料強化 | 各 `views/*.vue` | demo 展示用 |
| 管理員後台（假資料版） | `views/admin/*` | demo 展示用 |
| 精簡版 WorkflowEngine（-90 行） | `backend/.../WorkflowEngine.cs` | 移除條件引擎後的簡化版 |
| README.md（作品集介紹） | `README.md` | 公開展示文件 |

---

## 🔨 開發中 / 待同步

### 005-signalr（進行中，main 線開發）
- **狀態**：spec.md 已撰寫，待 /speckit.plan
- **main**：完整實作（授權、多群組、重連補拉）
- **demo 同步計畫**：
  - [ ] 待 main 完成實作後，評估 demo 是否需要
  - [ ] 若進 demo：簡化為單一叫號動畫展示（可用 setInterval 模擬 WebSocket）或直接用簡化版 SignalR

---

## 📋 同步操作手冊

### 當 main 有新功能要灌入 demo

```bash
# 1. 切到 demo
git checkout demo

# 2. 從 main cherry-pick 該功能的 commits（用 git log main 找 hash）
git cherry-pick <commit-hash>

# 3. 解決可能的衝突（主要是 style.css 主題、App.vue 導航列）
# 4. 移除不該進 demo 的部分（specs/、.specify/、條件引擎邏輯）
# 5. 視需要加上 demo 專屬的 Mock 資料或展示包裝

# 6. 推送到兩個 remote
git push origin demo
git push public demo:main
```

### 當 demo 有展示相關改動（淺色主題、Mock 資料）

**不要灌回 main**。demo 獨有的 UI/資料變更留在 demo。

---

## 🚨 漂移警示

目前漂移程度：**中等**（7 個 commit 獨立差異 + 43 個檔案差異）

**根本原因**：同一功能（AI 分流、自然語言指令、模型優化）在兩邊各自 commit 一次，造成歷史分叉但內容重複。

**改善建議**（未來執行）：
1. 新功能**只在 main 開發**，完成後 cherry-pick 或手動搬運到 demo
2. 每完成 2-3 個功能就同步一次 demo，避免漂移累積
3. demo 分支 push 時務必同時推 `origin` 和 `public`（CLAUDE.md 規則）
