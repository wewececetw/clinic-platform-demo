# clinic-platform Constitution

> Methodology: Spec-Driven Development | Tool: Vibe Lens
> Inspired by [GitHub Spec Kit](https://github.com/github/spec-kit)

**Version**: 1.0.0 | **Ratified**: 2026-03-31

---

## Core Principles

### Principle 1: Clean Architecture（Domain / Application / Infrastructure / WebAPI）

### Principle 2: Multi-tenant SaaS：所有資料以 clinic_id 隔離

### Principle 3: 後端 ASP.NET Core 10 + EF Core + MySQL

### Principle 4: 前端 Vue 3 + TypeScript + Vite（PWA）

### Principle 5: API 一律 RESTful，回傳統一 Result<T> 格式

### Principle 6: LLM 抽象層（ILlmClient）支援多 provider fallback

### Principle 7: 所有文件、註解、commit 使用繁體中文

## Additional Constraints

MySQL provider 使用 MySql.EntityFrameworkCore 10.0.1（非 Pomelo）
候診佇列 Redis 為主、MySQL queue_entries 為持久化備份
AI 整合：OMLX 本地優先、Groq 免費 tier 備援，統一 OpenAI 相容格式
安全性：OWASP Top 10 防護、多租戶資料隔離
效能：API 回應 < 500ms，AI 分流 < 5 秒

## Development Workflow

sdd_specify → sdd_analyze → sdd_plan → sdd_tasks → sdd_implement
每個 Phase 完成後需 commit
demo 分支變更需同步推送至 public remote

## Governance

This constitution supersedes conflicting practices.
Amendments require documented rationale and version increment.

- MAJOR version: backward-incompatible principle changes
- MINOR version: new principles or sections added
- PATCH version: clarifications or wording improvements
