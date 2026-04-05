# Demo 啟動指南 — 面試用

**適用場景**：面試當天在 Mac 上快速啟動完整 demo

## 前置需求（只需做一次）

- ✅ Docker Desktop（已安裝）
- ✅ .NET 10 SDK（`/opt/homebrew/bin/dotnet`）
- ✅ Node.js 22+（`/opt/homebrew/bin/node`）
- ✅ 專案已 clone 到 `~/Desktop/hospital`

## 一鍵啟動（3 個 terminal 視窗）

### Terminal 1：DB 服務

```bash
cd ~/Desktop/hospital
/usr/local/bin/docker compose up -d

# 驗證
/usr/local/bin/docker ps | grep clinic
# 應該看到 clinic-mysql 和 clinic-redis
```

### Terminal 2：後端

```bash
cd ~/Desktop/hospital/backend/src/ClinicPlatform.WebAPI
/opt/homebrew/bin/dotnet run

# 等待看到：Now listening on: http://localhost:5160
```

### Terminal 3：前端

```bash
cd ~/Desktop/hospital/frontend
PATH="/opt/homebrew/bin:$PATH" npm run dev

# 等待看到：Local: http://localhost:5173/
```

## 瀏覽器打開

- 前端：http://localhost:5173
- 後端 API 文件：http://localhost:5160/openapi/v1.json

## Demo 路徑（建議展示順序）

1. **報到** → 輸入症狀（例：「喉嚨痛、發燒」）→ 展示 AI 分流結果
2. **候診** → 顯示號碼牌 + 排隊位置
3. **護理師** → 叫下一位 → 展示自動更新候診清單
4. **醫師** → 看診介面 → 展示自然語言開處方
5. **藥劑師** → 調劑 → 展示 Workflow 自動推進
6. **管理員後台** → 展示統計數據

## AI Provider 切換

### 預設：OMLX（本地 Qwen2.5-7B）
- 檔案：`backend/src/ClinicPlatform.WebAPI/appsettings.json`
- `"Provider": "Omlx"`
- 需本機 OMLX 服務運行於 `localhost:9000`

### 備援：Groq（雲端）
- 需設定 `AI.Groq.ApiKey`
- 改 `"Provider": "Groq"`
- 需要網路

### ⚠️ 離線備案
若面試現場無網路且 OMLX 沒開：
- AI 功能可能失敗，但 Result.Fail 會回優雅錯誤訊息不當機
- 展示時強調「fallback 機制」即可，無需真的呼叫

## 常見問題排除

### 後端連不上 DB
```bash
# 重啟 DB 容器
/usr/local/bin/docker compose restart mysql
sleep 5
```

### 前端 Vite 報錯
```bash
cd ~/Desktop/hospital/frontend
rm -rf node_modules/.vite
npm run dev
```

### Port 被占用
```bash
# 找出占用者
lsof -i :5160  # 後端
lsof -i :5173  # 前端
# kill -9 <PID>
```

## 優雅停止

```bash
# Terminal 2、3：Ctrl+C
# Terminal 1:
/usr/local/bin/docker compose down
```
