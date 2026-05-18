#!/usr/bin/env bash
# 【新 ECS】一键部署入口。串起：项目初始化 + secrets 恢复 + 启动容器。
#
# 用法（在新机上 SSH 后）：
#
#   # 模式 A：有旧机备份包（推荐）
#   curl -fsSL https://raw.githubusercontent.com/TinyMaD/SpeedRunnersLab/master/scripts/bootstrap.sh \
#     | bash -s /root/srlab-secrets-bak-XXXX.tar.gz
#
#   # 模式 B：没备份，先把项目跑起来，secrets 之后补
#   curl -fsSL https://raw.githubusercontent.com/TinyMaD/SpeedRunnersLab/master/scripts/bootstrap.sh | bash

set -e

BACKUP="${1:-}"
WORK_DIR="/root/home/srlab"
RAW_BASE="https://raw.githubusercontent.com/TinyMaD/SpeedRunnersLab/master/scripts"

echo ""
echo "================================================================"
echo "  SpeedRunnersLab 一键部署"
[ -n "$BACKUP" ] && echo "  模式: 含备份恢复 (包: $BACKUP)" || echo "  模式: 无备份（仅项目部署）"
echo "================================================================"

# ---------------- 前置检查 ----------------
echo ""
echo "==> [1/4] 前置检查"
command -v docker >/dev/null 2>&1 || { echo "  docker 没装。yum install -y docker-ce 或参考 https://docs.docker.com/engine/install/"; exit 1; }
command -v git    >/dev/null 2>&1 || { echo "  git 没装。yum install -y git";   exit 1; }
command -v curl   >/dev/null 2>&1 || { echo "  curl 没装。yum install -y curl"; exit 1; }
systemctl is-active --quiet docker || systemctl start docker
echo "    docker/git/curl 就绪"

# 确保工作目录存在
mkdir -p "$WORK_DIR"
cd "$WORK_DIR"

# ---------------- 项目初始化 ----------------
echo ""
echo "==> [2/4] 跑项目初始化脚本 (setup-ecs.sh)"
curl -fsSL "$RAW_BASE/setup-ecs.sh" | bash

# ---------------- 备份恢复（可选）----------------
if [ -n "$BACKUP" ]; then
    echo ""
    echo "==> [3/4] 恢复 secrets（acme.sh + 证书 + appsettings + .env）"
    if [ ! -f "$BACKUP" ]; then
        echo "  找不到备份包: $BACKUP"
        exit 1
    fi
    curl -fsSL "$RAW_BASE/restore-secrets.sh" | bash -s "$BACKUP"
else
    echo ""
    echo "==> [3/4] 跳过备份恢复（你没传备份包参数）"
    echo "    注意：appsettings.Production.json / App.config / SSL 证书 还没就位，"
    echo "    docker 起来后 API / UI 容器会失败。手动补完这些文件再启动。"
fi

# ---------------- 启动容器 ----------------
echo ""
echo "==> [4/4] 拉镜像 + 启动"
docker compose -f docker-compose.prod.yml pull
docker compose -f docker-compose.prod.yml up -d

sleep 3
echo ""
echo "--- 容器状态 ---"
docker compose -f docker-compose.prod.yml ps

echo ""
echo "================================================================"
if [ -n "$BACKUP" ]; then
    echo "  全部完成。访问 https://www.speedrunners.cn/ 验证"
    echo ""
    echo "  自动续期已恢复，下次续期日："
    /root/.acme.sh/acme.sh --list 2>/dev/null | awk 'NR==2 {print "    " $0}'
    echo ""
    echo "  立刻删掉备份包（含明文敏感信息）："
    echo "    rm $BACKUP"
else
    echo "  项目部署完成，但 SSL 和敏感配置未恢复。"
    echo "  补完以下文件后重启容器即可："
    echo "    $WORK_DIR/SpeedRunners.API/SpeedRunners/appsettings.Production.json"
    echo "    $WORK_DIR/SpeedRunners.Scheduler/App.config"
    echo "    $WORK_DIR/SpeedRunners.UI/nginx/api.speedrunners.cn.crt"
    echo "    $WORK_DIR/SpeedRunners.UI/nginx/api.speedrunners.cn.key"
    echo "    然后: docker compose -f docker-compose.prod.yml restart"
fi
echo "================================================================"
