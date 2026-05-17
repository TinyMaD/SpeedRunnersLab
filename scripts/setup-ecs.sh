#!/usr/bin/env bash
# ECS 一键初始化脚本（ghcr.io 方案）
#
# 用法（SSH 进 ECS 后执行）：
#   cd /root/home/srlab
#   curl -fsSL https://raw.githubusercontent.com/TinyMaD/SpeedRunnersLab/master/scripts/setup-ecs.sh | bash

set -e

C_RESET="\033[0m"; C_CYAN="\033[36m"; C_GREEN="\033[32m"; C_YELLOW="\033[33m"; C_RED="\033[31m"
step() { echo; echo -e "${C_CYAN}[$1/$2] $3${C_RESET}"; }
ok()   { echo -e "  ${C_GREEN}✓${C_RESET} $1"; }
warn() { echo -e "  ${C_YELLOW}⚠${C_RESET} $1"; }
err()  { echo -e "  ${C_RED}✗${C_RESET} $1" >&2; }

# 仓库相关常量
GIT_URL="https://github.com/TinyMaD/SpeedRunnersLab.git"
GHCR_OWNER="tinymad"  # ghcr.io 上小写
EXPECTED_DIR="/root/home/srlab"

cat <<'BANNER'

╔══════════════════════════════════════════════════════════╗
║   SpeedRunnersLab ECS 一键初始化（ghcr.io 方案）            ║
╚══════════════════════════════════════════════════════════╝

BANNER

# ----------------------------------------------------------
step 1 4 "前置检查"
# ----------------------------------------------------------

if [ "$(pwd)" != "$EXPECTED_DIR" ]; then
    err "期望在 $EXPECTED_DIR 执行，当前在 $(pwd)。先 cd $EXPECTED_DIR"
    exit 1
fi
ok "工作目录正确"

if ! command -v docker >/dev/null 2>&1; then
    err "docker 没装"
    exit 1
fi
ok "docker 已安装"

if ! command -v git >/dev/null 2>&1; then
    err "git 没装。yum install -y git 或 apt install -y git"
    exit 1
fi
ok "git 已安装"

# ----------------------------------------------------------
step 2 4 "把目录改造成 git 仓库"
# ----------------------------------------------------------

if [ -d ".git" ]; then
    warn "已经是 git 仓库，跳过 init，直接拉最新"
    # 显式 refspec 确保 origin/master 引用被创建
    git fetch origin "+refs/heads/master:refs/remotes/origin/master" --depth 1
    git reset --hard origin/master
    ok "已对齐 origin/master"
else
    BACKUP_DIR="/root/srlab-backup-$(date +%Y%m%d-%H%M%S)"
    mkdir -p "$BACKUP_DIR"
    cp -r docker-compose.yml "$BACKUP_DIR/" 2>/dev/null || true
    ok "已备份 docker-compose.yml 到 $BACKUP_DIR"

    git init -q
    git remote add origin "$GIT_URL"
    # 关键：用显式 refspec，否则浅 fetch 不会创建 refs/remotes/origin/master
    git fetch origin "+refs/heads/master:refs/remotes/origin/master" --depth 1
    git checkout -B master origin/master
    git branch --set-upstream-to=origin/master master 2>/dev/null || true
    ok "已绑定到 $GIT_URL 的 master 分支"
fi

# ----------------------------------------------------------
step 3 4 "确保 docker compose v2 可用"
# ----------------------------------------------------------

if docker compose version >/dev/null 2>&1; then
    ok "docker compose v2 已就绪：$(docker compose version | head -1)"
else
    warn "未检测到 docker compose v2，安装中..."
    mkdir -p /usr/local/lib/docker/cli-plugins
    COMPOSE_VER="v2.27.0"
    curl -SL "https://github.com/docker/compose/releases/download/${COMPOSE_VER}/docker-compose-linux-x86_64" \
        -o /usr/local/lib/docker/cli-plugins/docker-compose
    chmod +x /usr/local/lib/docker/cli-plugins/docker-compose
    if docker compose version >/dev/null 2>&1; then
        ok "已安装 $(docker compose version | head -1)"
    else
        err "安装失败"
        exit 1
    fi
fi

# ----------------------------------------------------------
step 4 4 "写入 .env 并检查敏感配置"
# ----------------------------------------------------------

# 写入 .env，docker compose 会自动读
cat > .env <<EOF
GHCR_OWNER=$GHCR_OWNER
IMAGE_TAG=latest
EOF
ok "已写入 .env (GHCR_OWNER=$GHCR_OWNER)"

CHECK_FILES=(
    "./SpeedRunners.API/SpeedRunners/appsettings.Production.json"
    "./SpeedRunners.Scheduler/App.config"
)
MISSING=0
for f in "${CHECK_FILES[@]}"; do
    if [ -f "$f" ]; then
        ok "存在 $f"
    else
        warn "缺失 $f  ← CI 部署后容器会启动失败！"
        MISSING=$((MISSING+1))
    fi
done

# 清旧的本地构建产物
if [ -d "./SpeedRunners.UI/dist" ]; then
    rm -rf ./SpeedRunners.UI/dist
    ok "已删除旧的 SpeedRunners.UI/dist"
fi

# ----------------------------------------------------------
echo
if [ $MISSING -gt 0 ]; then
    echo -e "${C_YELLOW}╔══════════════════════════════════════════════════════════╗"
    echo -e "║   ECS 初始化完成 ⚠  有 $MISSING 个敏感配置文件缺失！                ║"
    echo -e "║   请从本地或备份恢复后再触发 CI                              ║"
    echo -e "╚══════════════════════════════════════════════════════════╝${C_RESET}"
else
    echo -e "${C_GREEN}╔══════════════════════════════════════════════════════════╗"
    echo -e "║   ECS 初始化完成 ✓                                          ║"
    echo -e "║                                                            ║"
    echo -e "║   下一步：浏览器打开                                          ║"
    echo -e "║   https://github.com/TinyMaD/SpeedRunnersLab/actions       ║"
    echo -e "║   选 'Build & Deploy to Production' → 点 'Run workflow'      ║"
    echo -e "║                                                            ║"
    echo -e "║   首次构建约 10~15 分钟，之后每次 3~5 分钟                     ║"
    echo -e "╚══════════════════════════════════════════════════════════╝${C_RESET}"
fi
