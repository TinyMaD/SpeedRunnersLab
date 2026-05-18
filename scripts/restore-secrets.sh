#!/usr/bin/env bash
# 在【新 ECS】上跑，把 backup-secrets.sh 产出的 tar.gz 解包到原位置 + 重建 acme.sh cron。
#
# 用法:
#   bash restore-secrets.sh /root/srlab-secrets-bak-YYYYMMDD-HHMMSS.tar.gz

set -e

PACK="${1:-}"
if [ -z "$PACK" ] || [ ! -f "$PACK" ]; then
    echo "用法: $0 <备份包.tar.gz>"
    [ -n "$PACK" ] && echo "找不到文件: $PACK"
    exit 1
fi

echo "==> 解包到 /"
tar -xzf "$PACK" -C /
echo "    解包完成"

echo ""
echo "==> 重建 acme.sh cron（每天 5:00 自动续期）"
if crontab -l 2>/dev/null | grep -q 'acme.sh --cron'; then
    echo "    cron 已存在，跳过"
else
    (crontab -l 2>/dev/null; echo '0 5 * * * "/root/.acme.sh"/acme.sh --cron --home "/root/.acme.sh" > /dev/null') | crontab -
    echo "    已添加 cron"
fi

echo ""
echo "==> 验证"
echo "--- 当前 crontab ---"
crontab -l | sed 's/^/    /'
echo ""

echo "--- acme.sh 管理的证书 ---"
[ -x /root/.acme.sh/acme.sh ] && /root/.acme.sh/acme.sh --list 2>/dev/null | sed 's/^/    /' || echo "    acme.sh 未恢复"
echo ""

echo "--- nginx 目录证书 ---"
ls -la /root/home/srlab/SpeedRunners.UI/nginx/ 2>/dev/null | sed 's/^/    /' || echo "    目录不存在"
echo ""

echo "--- 项目敏感配置 ---"
for f in \
    "/root/home/srlab/SpeedRunners.API/SpeedRunners/appsettings.Production.json" \
    "/root/home/srlab/SpeedRunners.Scheduler/App.config" \
    "/root/home/srlab/.env"
do
    [ -f "$f" ] && echo "    [OK] $f" || echo "    [缺失] $f"
done

echo ""
echo "================================================================"
echo "  恢复完成"
echo "  立刻删掉备份包: rm $PACK"
echo "================================================================"
