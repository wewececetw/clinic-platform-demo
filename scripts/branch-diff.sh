#!/usr/bin/env bash
# branch-diff.sh — main ↔ demo 分支差異檢視工具
#
# Usage:
#   ./scripts/branch-diff.sh              顯示完整差異報告
#   ./scripts/branch-diff.sh commits      只看 commit 差異
#   ./scripts/branch-diff.sh files        只看檔案差異
#   ./scripts/branch-diff.sh health       漂移健康度檢查
#   ./scripts/branch-diff.sh sync-preview 預覽同步會帶入哪些 commits

set -euo pipefail

BRANCH_A="${BRANCH_A:-main}"
BRANCH_B="${BRANCH_B:-demo}"
MODE="${1:-full}"

# ---------- 顏色 ----------
R='\033[0;31m'
G='\033[0;32m'
Y='\033[1;33m'
B='\033[0;34m'
C='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m'

hr() { printf "${C}%s${NC}\n" "────────────────────────────────────────────────────────────"; }
title() { printf "\n${BOLD}${B}▶ %s${NC}\n" "$1"; hr; }

# ---------- 檢查分支存在 ----------
for b in "$BRANCH_A" "$BRANCH_B"; do
  if ! git rev-parse --verify "$b" &>/dev/null; then
    printf "${R}✗ 分支 %s 不存在${NC}\n" "$b"
    exit 1
  fi
done

# ---------- 通用統計 ----------
AHEAD_A=$(git rev-list --count "$BRANCH_B..$BRANCH_A")
AHEAD_B=$(git rev-list --count "$BRANCH_A..$BRANCH_B")
COMMON=$(git merge-base "$BRANCH_A" "$BRANCH_B")
COMMON_SHORT=$(git rev-parse --short "$COMMON")
COMMON_MSG=$(git log "$COMMON" -1 --format="%s")

show_overview() {
  title "分支概況"
  printf "  ${BOLD}%s${NC}  領先 %s：${G}%s commits${NC}\n" "$BRANCH_A" "$BRANCH_B" "$AHEAD_A"
  printf "  ${BOLD}%s${NC}  領先 %s：${G}%s commits${NC}\n" "$BRANCH_B" "$BRANCH_A" "$AHEAD_B"
  printf "  共同祖先：${Y}%s${NC} %s\n" "$COMMON_SHORT" "$COMMON_MSG"

  CURRENT=$(git branch --show-current)
  printf "  當前分支：${C}%s${NC}\n" "$CURRENT"
}

show_commits() {
  title "$BRANCH_A 獨有 commits（$AHEAD_A 個）"
  if [ "$AHEAD_A" -gt 0 ]; then
    git log "$BRANCH_B..$BRANCH_A" --oneline --color=always | head -20
  else
    printf "  ${G}（無）${NC}\n"
  fi

  title "$BRANCH_B 獨有 commits（$AHEAD_B 個）"
  if [ "$AHEAD_B" -gt 0 ]; then
    git log "$BRANCH_A..$BRANCH_B" --oneline --color=always | head -20
  else
    printf "  ${G}（無）${NC}\n"
  fi
}

show_files() {
  title "檔案層差異（$BRANCH_A vs $BRANCH_B）"

  SRC_DIFF=$(git diff "$BRANCH_A".."$BRANCH_B" --name-only -- backend/src/ frontend/src/ 2>/dev/null | wc -l | tr -d ' ')
  DOC_DIFF=$(git diff "$BRANCH_A".."$BRANCH_B" --name-only -- specs/ docs/ .specify/ 2>/dev/null | wc -l | tr -d ' ')
  ALL_DIFF=$(git diff "$BRANCH_A".."$BRANCH_B" --name-only 2>/dev/null | wc -l | tr -d ' ')

  printf "  ${BOLD}程式碼檔案差異${NC}（src/）：${Y}%s${NC} 個\n" "$SRC_DIFF"
  printf "  ${BOLD}文件檔案差異${NC}（specs、docs）：${C}%s${NC} 個\n" "$DOC_DIFF"
  printf "  ${BOLD}全部檔案差異${NC}：${C}%s${NC} 個\n" "$ALL_DIFF"

  echo
  printf "${BOLD}  🔴 內容真的不同的 src 檔案（需關注）：${NC}\n"
  git diff "$BRANCH_A".."$BRANCH_B" --stat -- backend/src/ frontend/src/ 2>/dev/null | grep -v "^ *\$" | sed 's/^/    /' || echo "    （無）"

  echo
  printf "${BOLD}  📄 %s 獨有檔案（%s 沒有）：${NC}\n" "$BRANCH_A" "$BRANCH_B"
  git diff "$BRANCH_A".."$BRANCH_B" --diff-filter=D --name-only 2>/dev/null | head -15 | sed 's/^/    - /' || true
  ONLY_A=$(git diff "$BRANCH_A".."$BRANCH_B" --diff-filter=D --name-only 2>/dev/null | wc -l | tr -d ' ')
  [ "$ONLY_A" -gt 15 ] && printf "    ${Y}... 還有 %s 個${NC}\n" "$((ONLY_A - 15))"

  echo
  printf "${BOLD}  📄 %s 獨有檔案（%s 沒有）：${NC}\n" "$BRANCH_B" "$BRANCH_A"
  git diff "$BRANCH_A".."$BRANCH_B" --diff-filter=A --name-only 2>/dev/null | head -15 | sed 's/^/    + /' || true
  ONLY_B=$(git diff "$BRANCH_A".."$BRANCH_B" --diff-filter=A --name-only 2>/dev/null | wc -l | tr -d ' ')
  [ "$ONLY_B" -gt 15 ] && printf "    ${Y}... 還有 %s 個${NC}\n" "$((ONLY_B - 15))"
}

show_health() {
  title "漂移健康度檢測"

  # 計算實質差異檔案數（排除統計摘要行）
  SRC_REAL=$(git diff "$BRANCH_A".."$BRANCH_B" --name-only -- backend/src/ frontend/src/ 2>/dev/null | grep -c . 2>/dev/null)
  SRC_REAL=${SRC_REAL:-0}
  LAST_SYNC=$(git log "$BRANCH_B" --grep="同步\|sync\|cherry-pick" -1 --format="%ar" 2>/dev/null)
  [ -z "$LAST_SYNC" ] && LAST_SYNC="從未"

  STATUS="${G}健康${NC}"
  WARN_SRC=""
  WARN_COMMITS=""

  if [ "$SRC_REAL" -gt 5 ]; then
    STATUS="${R}需同步${NC}"
    WARN_SRC="src 檔案差異 $SRC_REAL > 5"
  fi
  if [ "$AHEAD_A" -gt 3 ]; then
    [ "$STATUS" = "${G}健康${NC}" ] && STATUS="${Y}建議同步${NC}"
    WARN_COMMITS="$BRANCH_A 領先 $AHEAD_A commits > 3"
  fi

  printf "  狀態：%b\n" "$STATUS"
  printf "  程式碼實質差異：${Y}%s${NC} 個檔案\n" "$SRC_REAL"
  printf "  %s 領先 commits：${Y}%s${NC}\n" "$BRANCH_A" "$AHEAD_A"
  printf "  上次同步：${C}%s${NC}\n" "$LAST_SYNC"

  if [ -n "$WARN_SRC" ] || [ -n "$WARN_COMMITS" ]; then
    echo
    printf "  ${R}紅線警示：${NC}\n"
    [ -n "$WARN_SRC" ] && printf "    ⚠️  %s\n" "$WARN_SRC"
    [ -n "$WARN_COMMITS" ] && printf "    ⚠️  %s\n" "$WARN_COMMITS"
  fi
}

show_sync_preview() {
  title "同步預覽：如果把 $BRANCH_A 的新 commits 搬到 $BRANCH_B"

  if [ "$AHEAD_A" -eq 0 ]; then
    printf "  ${G}✓ 無需同步，$BRANCH_B 已包含 $BRANCH_A 所有 commits${NC}\n"
    return
  fi

  printf "  會執行 cherry-pick 的 commits：\n"
  git log "$BRANCH_B..$BRANCH_A" --oneline --reverse --color=always | sed 's/^/    /'

  echo
  printf "  預期衝突檔案（$BRANCH_A 改過且 $BRANCH_B 也改過）：\n"
  MAIN_CHANGED=$(git diff "$COMMON..$BRANCH_A" --name-only 2>/dev/null | sort -u)
  DEMO_CHANGED=$(git diff "$COMMON..$BRANCH_B" --name-only 2>/dev/null | sort -u)
  CONFLICTS=$(comm -12 <(echo "$MAIN_CHANGED") <(echo "$DEMO_CHANGED") 2>/dev/null)

  if [ -z "$CONFLICTS" ]; then
    printf "    ${G}（預期無衝突）${NC}\n"
  else
    echo "$CONFLICTS" | sed 's/^/    ⚠️  /'
  fi

  echo
  printf "  ${BOLD}同步指令：${NC}\n"
  printf "    ${C}git checkout %s${NC}\n" "$BRANCH_B"
  while IFS='|' read -r HASH MSG; do
    printf "    ${C}git cherry-pick %s${NC}  # %s\n" "$HASH" "$MSG"
  done < <(git log "$BRANCH_B..$BRANCH_A" --reverse --format="%h|%s")
  printf "    ${C}git push origin %s${NC}\n" "$BRANCH_B"
  printf "    ${C}git push public %s:main${NC}\n" "$BRANCH_B"
}

# ---------- 主流程 ----------
case "$MODE" in
  commits)
    show_overview
    show_commits
    ;;
  files)
    show_overview
    show_files
    ;;
  health)
    show_overview
    show_health
    ;;
  sync-preview|preview|sync)
    show_overview
    show_sync_preview
    ;;
  full|*)
    show_overview
    show_commits
    show_files
    show_health
    show_sync_preview
    ;;
esac

echo
hr
printf "${C}💡 其他用法：${NC}\n"
printf "  ${C}./scripts/branch-diff.sh commits${NC}       只看 commit 差異\n"
printf "  ${C}./scripts/branch-diff.sh files${NC}         只看檔案差異\n"
printf "  ${C}./scripts/branch-diff.sh health${NC}        漂移健康度\n"
printf "  ${C}./scripts/branch-diff.sh sync-preview${NC}  預覽同步操作\n"
printf "  ${C}BRANCH_A=005-signalr BRANCH_B=main %s${NC}  比對其他分支\n" "./scripts/branch-diff.sh"
