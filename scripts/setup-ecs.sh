#!/usr/bin/env bash
# ECS 一键初始化脚本：把当前手动维护的 srlab 目录改造成可被 CI 自动部署的状态
#
# 用法（SSH 进 ECS 后执行）：
#   cd /root/home/srlab
#   bash scripts/setup-ecs.sh
# 或在线一行版：
#   curl -fsSL https://raw.githubusercontent.com/<你的GH用户>/<仓库>/master/scripts/setup-ecs.sh | bash

set -e

C_RESET="\033[0m"; C_CYAN="\033[36m"; C_GREEN="\033[32m"; C_YELLOW="\033[33m"; C_RED="\033[31m"
step() { echo; echo -e "${C_CYAN}[$1/$2] $3${C_RESET}"; }
ok()   { echo -e "  ${C_GREEN}✓${C_RESET} $1"; }
warn() { echo -e "  ${C_YELLOW}⚠${C_RESET} $1"; }
err()  { echo -e "  ${C_RED}✗${C_RESET} $1" >&2; }

cat <<'BANNER'

╔══════════════════════════════════════════════════════════╗
║   SpeedRunnersLab ECS 一键初始化                            ║
╚══════════════════════════════════════════════════════════╝

BANNER

# ----------------------------------------------------------
step 1 5 "前置检查"
# ----------------------------------------------------------

EXPECTED_DIR="/root/home/srlab"
if [ "$(pwd)" != "$EXPECTED_DIR" ]; then
    err "期望在 $EXPECTED_DIR 执行，当前在 $(pwd)。请先 cd $EXPECTED_DIR"
    exit 1
fi
ok "工作目录正确"

if ! command -v docker >/dev/null 2>&1; then
    err "docker 没装。请先装 docker 后再跑此脚本"
    exit 1
fi
ok "docker 已安装"

if ! command -v git >/dev/null 2>&1; then
    err "git 没装。yum install -y git 或 apt install -y git"
    exit 1
fi
ok "git 已安装"

# ----------------------------------------------------------
step 2 5 "把目录改造成 git 仓库"
# ----------------------------------------------------------

if [ -d ".git" ]; then
    warn "已经是 git 仓库，跳过 init"
else
    echo "  当前目录会被绑定到一个 GitHub 仓库。"
    read -r -p "  请输入仓库的 git URL (如 https://github.com/youruser/SpeedRunnersLab.git)：" GIT_URL
    if [ -z "$GIT_URL" ]; then
        err "URL 不能为空"
        exit 1
    fi

    # 备份当前文件夹里 git 跟踪的文件，以防 reset 后丢失改动
    BACKUP_DIR="/root/srlab-backup-$(date +%Y%m%d-%H%M%S)"
    mkdir -p "$BACKUP_DIR"
    cp -r docker-compose.yml "$BACKUP_DIR/" 2>/dev/null || true
    ok "已备份 docker-compose.yml 到 $BACKUP_DIR"

    git init -q
    git remote add origin "$GIT_URL"
    git fetch origin master --depth 1
    git checkout -B master origin/master
    ok "已绑定到 $GIT_URL 的 master 分支"
fi

# ----------------------------------------------------------
step 3 5 "确保 docker compose v2 可用"
# ----------------------------------------------------------

if docker compose version >/dev/null 2>&1; then
    ok "docker compose v2 已就绪：$(docker compose version | head -1)"
else
    warn "未检测到 docker compose v2，正在安装 plugin..."
    mkdir -p /usr/local/lib/docker/cli-plugins
    COMPOSE_VER="v2.27.0"
    curl -SL "https://github.com/docker/compose/releases/download/${COMPOSE_VER}/docker-compose-linux-x86_64" \
        -o /usr/local/lib/docker/cli-plugins/docker-compose
    chmod +x /usr/local/lib/docker/cli-plugins/docker-compose
    if docker compose version >/dev/null 2>&1; then
        ok "已安装 docker compose $(docker compose version | head -1)"
    else
        err "安装失败，请手动处理"
        exit 1
    fi
fi

# ----------------------------------------------------------
step 4 5 "登录阿里云 ACR"
# ----------------------------------------------------------

read -r -p "  ACR Registry 地址 (如 registry.cn-hangzhou.aliyuncs.com)：" REGISTRY
read -r -p "  ACR 用户名：" ACR_USER
read -r -s -p "  ACR 密码：" ACR_PASS; echo

echo "$ACR_PASS" | docker login "$REGISTRY" -u "$ACR_USER" --password-stdin
if [ $? -eq 0 ]; then
    ok "已登录 $REGISTRY（凭据保存在 ~/.docker/config.json）"
else
    err "登录失败，密码错误或网络不通"
    exit 1
fi

# ----------------------------------------------------------
step 5 5 "检查敏感配置文件是否就位"
# ----------------------------------------------------------

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

# 顺手清理旧的本地构建产物
if [ -d "./SpeedRunners.UI/dist" ]; then
    rm -rf ./SpeedRunners.UI/dist
    ok "已删除旧的 SpeedRunners.UI/dist（CI 镜像里自带 dist，不再需要挂载）"
fi

# ----------------------------------------------------------
echo
if [ $MISSING -gt 0 ]; then
    echo -e "${C_YELLOW}╔══════════════════════════════════════════════════════════╗"
    echo -e "║   ECS 初始化完成 ⚠  有 $MISSING 个敏感配置文件缺失！                  ║"
    echo -e "║   请把它们从你本地或备份恢复到对应路径再触发 CI                ║"
    echo -e "╚══════════════════════════════════════════════════════════╝${C_RESET}"
else
    echo -e "${C_GREEN}╔══════════════════════════════════════════════════════════╗"
    echo -e "║   ECS 初始化完成 ✓                                          ║"
    echo -e "║                                                            ║"
    echo -e "║   下一步：浏览器打开 GitHub 仓库 → Actions →                  ║"
    echo -e "║   选 'Build & Deploy to Production' → 点 'Run workflow'      ║"
    echo -e "║                                                            ║"
    echo -e "║   首次构建约 10~15 分钟，之后每次 3~5 分钟                     ║"
    echo -e "╚══════════════════════════════════════════════════════════╝${C_RESET}"
fi
