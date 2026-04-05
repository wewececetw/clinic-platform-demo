#!/usr/bin/env bash
# sync-to-demo.sh — 從 main 重建 demo 分支並套用 overlay
#
# Usage:
#   ./scripts/sync-to-demo.sh              實際執行
#   ./scripts/sync-to-demo.sh --dry-run    只印不執行
#
# 前置條件：
#   - 當前分支為 main
#   - working tree 乾淨
#   - 存在 demo-overlay/ 目錄（內含 overlay 檔案）

set -euo pipefail

# ---------- 顏色 ----------
R='\033[0;31m'
G='\033[0;32m'
Y='\033[0;33m'
C='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m'

# ---------- 參數解析 ----------
DRY_RUN=false
while [[ $# -gt 0 ]]; do
    case $1 in
        --dry-run)
            DRY_RUN=true; shift ;;
        -h|--help)
            sed -n '2,12p' "$0"; exit 0 ;;
        *)
            echo -e "${R}✗ 未知參數: $1${NC}"; exit 1 ;;
    esac
done

run() {
    if [[ "$DRY_RUN" == true ]]; then
        echo -e "${Y}  [DRY-RUN] $*${NC}"
    else
        eval "$@"
    fi
}

step() { echo -e "\n${BOLD}${C}▶ $1${NC}"; }
ok()   { echo -e "${G}  ✓ $1${NC}"; }
fail() { echo -e "${R}  ✗ $1${NC}"; exit 1; }

[[ "$DRY_RUN" == true ]] && echo -e "${Y}${BOLD}=== DRY-RUN 模式，不會實際改動 ===${NC}"

# ---------- 1. 前置檢查 ----------
step "前置檢查"

if ! git diff --quiet || ! git diff --cached --quiet; then
    fail "working tree 不乾淨，請先 commit 或 stash"
fi
ok "working tree 乾淨"

CURRENT_BRANCH=$(git branch --show-current)
if [[ "$CURRENT_BRANCH" != "main" ]]; then
    fail "當前分支必須是 main，目前在 $CURRENT_BRANCH"
fi
ok "當前分支：main"

if ! git remote | grep -q "^origin$"; then
    fail "找不到 origin remote"
fi
ok "origin remote 存在"

if [[ ! -d "demo-overlay" ]]; then
    fail "找不到 demo-overlay/ 目錄"
fi
ok "demo-overlay/ 目錄存在"

# ---------- 2. 重建 demo 分支（從 main）----------
step "重建 demo 分支"

# 先刪除本地舊 demo（若存在），再從 main 建立新的
run "git branch -D demo 2>/dev/null || true"
run "git checkout -b demo"
ok "新 demo 分支已從 main 建立"

# ---------- 3. 套用移除清單 ----------
step "套用移除清單（demo 不需要的私有檔案）"

if [[ -f "demo-overlay/remove-list.txt" ]]; then
    while IFS= read -r path || [[ -n "$path" ]]; do
        # 跳過空行與註解
        [[ -z "$path" || "$path" =~ ^# ]] && continue
        if [[ -e "$path" ]]; then
            run "rm -rf \"$path\""
            [[ "$DRY_RUN" == false ]] && ok "已移除 $path"
        fi
    done < "demo-overlay/remove-list.txt"
else
    echo -e "${Y}  （無 remove-list.txt，跳過移除步驟）${NC}"
fi

# ---------- 4. 套用 overlay 檔案 ----------
step "套用 overlay 檔案"

# 前端展示包裝
if [[ -d "demo-overlay/frontend" ]]; then
    run "cp -R demo-overlay/frontend/. frontend/"
    [[ "$DRY_RUN" == false ]] && ok "已套用 frontend overlay"
fi

# 後端精簡版 WorkflowEngine
if [[ -f "demo-overlay/backend/WorkflowEngine.cs" ]]; then
    run "cp demo-overlay/backend/WorkflowEngine.cs backend/src/ClinicPlatform.Infrastructure/Services/WorkflowEngine.cs"
    [[ "$DRY_RUN" == false ]] && ok "已套用精簡版 WorkflowEngine.cs"
fi

# 根目錄檔案（README.md、.gitignore 等）
if [[ -d "demo-overlay/root" ]]; then
    run "cp -R demo-overlay/root/. ."
    [[ "$DRY_RUN" == false ]] && ok "已套用根目錄 overlay"
fi

# ---------- 5. 刪除 demo-overlay 目錄 ----------
step "清理 overlay 目錄（demo 分支不需保留）"

run "rm -rf demo-overlay"
[[ "$DRY_RUN" == false ]] && ok "已刪除 demo-overlay/"

# ---------- 6. Commit ----------
step "Commit overlay 套用"

TODAY=$(date +%Y-%m-%d)
run "git add -A"
run "git commit -m \"chore: demo overlay 套用 ($TODAY)

Co-Authored-By: Barron <wewececetw@gmail.com>\""
[[ "$DRY_RUN" == false ]] && ok "commit 完成"

# ---------- 7. Push ----------
step "推送到 remote（force push）"

run "git push origin demo --force"
[[ "$DRY_RUN" == false ]] && ok "已推送到 origin"

if git remote | grep -q "^public$"; then
    run "git push public demo:main --force"
    [[ "$DRY_RUN" == false ]] && ok "已推送到 public"
else
    echo -e "${Y}  （跳過：public remote 不存在）${NC}"
fi

# ---------- 8. 切回 main ----------
step "切回 main 分支"
run "git checkout main"
[[ "$DRY_RUN" == false ]] && ok "已回到 main"

echo
echo -e "${G}${BOLD}🎉 demo 重建完成${NC}"
echo -e "${C}下一步：${NC}"
echo -e "  - 執行 ${BOLD}./scripts/branch-diff.sh files${NC} 驗證差異"
echo -e "  - 切到 demo 分支測試建置：${BOLD}git checkout demo && dotnet build backend/${NC}"
