#!/usr/bin/env bash
# 在【旧 ECS】上跑，把所有"不能进 git 的东西"打包成一个 tar.gz：
#   - /root/.acme.sh/                                    （acme.sh 全部状态：账号/证书/deploy hook/七牛 AK SK）
#   - /root/deploy_qiniu_all.sh                          （RenewHook 引用的脚本，可能不存在）
#   - /root/home/srlab/SpeedRunners.API/SpeedRunners/appsettings.Production.json
#   - /root/home/srlab/SpeedRunners.Scheduler/App.config
#   - /root/home/srlab/SpeedRunners.UI/nginx/*.crt *.key *.pem
#   - /root/home/srlab/.env
#
# 产出：/root/srlab-secrets-bak-YYYYMMDD-HHMMSS.tar.gz
# 该包【含明文敏感信息】，scp 到新机后立刻删掉，别留在 git/网盘/聊天工具里。

set -e

SRLAB_DIR="/root/home/srlab"
TS=$(date +%Y%m%d-%H%M%S)
OUT="/root/srlab-secrets-bak-$TS.tar.gz"

TMP=$(mktemp -d)
trap "rm -rf $TMP" EXIT

echo "==> 收集 acme.sh 状态"
mkdir -p "$TMP/root"
[ -d /root/.acme.sh ]            && cp -a /root/.acme.sh             "$TMP/root/"           && echo "    [OK] /root/.acme.sh"
[ -f /root/deploy_qiniu_all.sh ] && cp /root/deploy_qiniu_all.sh     "$TMP/root/"           && echo "    [OK] /root/deploy_qiniu_all.sh"

echo "==> 收集项目敏感配置"
mkdir -p "$TMP$SRLAB_DIR/SpeedRunners.API/SpeedRunners"
mkdir -p "$TMP$SRLAB_DIR/SpeedRunners.Scheduler"
mkdir -p "$TMP$SRLAB_DIR/SpeedRunners.UI/nginx"

F1="$SRLAB_DIR/SpeedRunners.API/SpeedRunners/appsettings.Production.json"
F2="$SRLAB_DIR/SpeedRunners.Scheduler/App.config"
F3="$SRLAB_DIR/.env"

[ -f "$F1" ] && cp "$F1" "$TMP$SRLAB_DIR/SpeedRunners.API/SpeedRunners/" && echo "    [OK] appsettings.Production.json" || echo "    [跳过] $F1 不存在"
[ -f "$F2" ] && cp "$F2" "$TMP$SRLAB_DIR/SpeedRunners.Scheduler/"        && echo "    [OK] App.config"                  || echo "    [跳过] $F2 不存在"
[ -f "$F3" ] && cp "$F3" "$TMP$SRLAB_DIR/"                               && echo "    [OK] .env"                        || echo "    [跳过] $F3 不存在"

# 证书三种后缀都收一遍（你当前用 .crt，但留点弹性）
copied=0
for ext in crt key pem; do
    for f in $SRLAB_DIR/SpeedRunners.UI/nginx/*.$ext; do
        [ -f "$f" ] || continue
        cp "$f" "$TMP$SRLAB_DIR/SpeedRunners.UI/nginx/"
        copied=$((copied+1))
    done
done
echo "    [OK] nginx 证书文件 $copied 个"

echo ""
echo "==> 打包"
tar -czf "$OUT" -C "$TMP" .
chmod 600 "$OUT"

echo ""
echo "================================================================"
echo "  备份完成"
echo "  文件: $OUT"
ls -lh "$OUT" | awk '{print "  大小: " $5}'
echo ""
echo "  下一步: scp 到新机后立刻在旧机删掉这个包"
echo "    scp $OUT root@<新机IP>:/root/"
echo "    rm $OUT      # 旧机上别留"
echo "================================================================"
