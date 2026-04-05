# demo-overlay — demo 分支展示包裝

本目錄存放 demo 分支相對於 main 的「展示專屬差異」，由 `scripts/sync-to-demo.sh` 套用。

## 用途

demo 分支 = main 程式碼 + 此 overlay，作為 **build artifact**，不是獨立開發分支。

## 目錄結構

```
demo-overlay/
├── README.md                          # 本檔案
├── remove-list.txt                    # 要移除的私有路徑清單
├── frontend/                          # 前端展示包裝
│   └── src/
│       ├── App.vue                    # 含導航列的版本
│       ├── style.css                  # 淺色醫療主題
│       └── views/
│           └── pharmacy/Queue.vue     # 含 Mock 資料的配藥佇列
├── backend/                           # 後端精簡版
│   └── WorkflowEngine.cs              # 線性推進（移除條件引擎）
└── root/                              # 根目錄檔案
    └── README.md                      # 作品集介紹
```

## 套用流程（sync-to-demo.sh 執行）

1. 從 main 建立新 demo 分支
2. 讀取 `remove-list.txt` 刪除私有路徑（specs/、.specify/ 等）
3. 複製 `frontend/` 到 `frontend/`（覆蓋同名檔案）
4. 複製 `backend/WorkflowEngine.cs` 到 `backend/src/.../WorkflowEngine.cs`
5. 複製 `root/*` 到專案根目錄
6. 刪除 `demo-overlay/` 本身（demo 不需要此目錄）
7. commit + force push

## 何時需要更新 overlay

- main 的 API 簽章變更 → 更新對應 overlay 檔案以避免建置失敗
- 需要新增/修改 Mock 資料 → 修改 `frontend/src/views/*/Queue.vue`
- 換主題配色 → 修改 `style.css`
- 作品集介紹文案更新 → 修改 `root/README.md`

## 原則

- **overlay 只改展示層**，不改核心商業邏輯
- **overlay 檔案必須與 main 對應檔案的介面一致**（函式簽章、import）
- **WorkflowEngine.cs 是唯一例外** — 需人工合併 main 新功能與 demo 精簡版邏輯
