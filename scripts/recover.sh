#!/usr/bin/env bash
# 幂等恢复脚本：把 ECS 上的 srlab 目录强制对齐到 GitHub master 并完成 CI/CD 初始化
set -e

GIT_URL="https://github.com/TinyMaD/SpeedRunnersLab.git"
GHCR_OWNER="tinymad"
WORK_DIR="/root/home/srlab"
TS=$(date +%Y%m%d-%H%M%S)

cd "$WORK_DIR"

echo ""
echo "==> [1/5] 强制把 git 对齐到 origin/master"
[ -d ".git" ] || git init -q
git remote remove origin 2>/dev/null || true
git remote add origin "$GIT_URL"
git fetch origin "+refs/heads/master:refs/remotes/origin/master"

BACKUP_DIR="/root/srlab-backup-$TS"
mkdir -p "$BACKUP_DIR"
git ls-tree -r origin/master --name-only | while read -r f; do
    if [ -f "$f" ] && ! git ls-files --error-unmatch "$f" >/dev/null 2>&1; then
        mkdir -p "$BACKUP_DIR/$(dirname "$f")"
        cp "$f" "$BACKUP_DIR/$f" 2>/dev/null || true
    fi
done
echo "    冲突文件备份在: $BACKUP_DIR"

git checkout -f -B master origin/master
git reset --hard origin/master
git branch --set-upstream-to=origin/master master 2>/dev/null || true
echo "    最新 commit: $(git log -1 --oneline)"

echo ""
echo "==> [2/5] 装 docker compose v2（如果没装）"
if docker compose version >/dev/null 2>&1; then
    echo "    已就绪: $(docker compose version | head -1)"
else
    mkdir -p /usr/local/lib/docker/cli-plugins
    curl -SL https://github.com/docker/compose/releases/download/v2.27.0/docker-compose-linux-x86_64 \
        -o /usr/local/lib/docker/cli-plugins/docker-compose
    chmod +x /usr/local/lib/docker/cli-plugins/docker-compose
    echo "    已安装: $(docker compose version | head -1)"
fi

echo ""
echo "==> [3/5] 写 .env"
cat > .env <<ENV
GHCR_OWNER=$GHCR_OWNER
IMAGE_TAG=latest
ENV
echo "    .env 内容:"
cat .env | sed 's/^/      /'

echo ""
echo "==> [4/5] 清旧本地构建产物"
rm -rf ./SpeedRunners.UI/dist 2>/dev/null || true
echo "    已删除 SpeedRunners.UI/dist"

echo ""
echo "==> [5/5] 检查敏感配置文件"
MISSING=0
for f in \
    "./SpeedRunners.API/SpeedRunners/appsettings.Production.json" \
    "./SpeedRunners.Scheduler/App.config"
do
    if [ -f "$f" ]; then
        echo "    [OK] $f"
    else
        echo "    [缺失] $f"
        MISSING=$((MISSING+1))
    fi
done

echo ""
echo "================================================================"
if [ $MISSING -gt 0 ]; then
    echo "  完成 但有 $MISSING 个敏感配置缺失！补全后再触发 CI"
else
    echo "  全部完成 OK"
    echo ""
    echo "  下一步: 浏览器打开"
    echo "    https://github.com/TinyMaD/SpeedRunnersLab/actions"
    echo "  选 'Build & Deploy to Production' -> 点 'Run workflow'"
fi
echo "================================================================"
