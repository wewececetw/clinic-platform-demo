# CLAUDE.md

## 語言規範
- 所有回覆、註解、commit 訊息、文件一律使用**繁體中文**

## Commit 規範
- 每次 git commit 訊息結尾加上：
  Co-Authored-By: Barron <wewececetw@gmail.com>
- **不要**加上 Claude 的 Co-Authored-By，只留 Barron

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
