#requires -Version 5.1
<#
.SYNOPSIS
  本地一次性自动化配置脚本：生成 SSH key、收集 ACR/ECS 信息、写入 GitHub Secrets。
.DESCRIPTION
  执行后会引导你完成所有"本地端"准备工作。装了 GitHub CLI (gh) 会全自动写 Secrets；
  没装也行，会输出粘贴说明 + 把每个 Secret 复制到剪贴板让你逐个粘到 GitHub UI。
.EXAMPLE
  .\scripts\setup-local.ps1
#>

$ErrorActionPreference = "Stop"

function Write-Step($num, $total, $msg) {
    Write-Host ""
    Write-Host "[$num/$total] $msg" -ForegroundColor Cyan
}
function Write-Ok($msg)   { Write-Host "  ✓ $msg" -ForegroundColor Green }
function Write-Info($msg) { Write-Host "  · $msg" -ForegroundColor Gray }
function Write-Warn($msg) { Write-Host "  ⚠ $msg" -ForegroundColor Yellow }
function Ask($prompt, $secure=$false) {
    if ($secure) {
        $sec = Read-Host -Prompt "  $prompt" -AsSecureString
        return [System.Net.NetworkCredential]::new("", $sec).Password
    } else {
        return Read-Host -Prompt "  $prompt"
    }
}
function Pause-Enter($msg = "按 Enter 继续") {
    Read-Host "  >>> $msg"
}

Write-Host @"

╔══════════════════════════════════════════════════════════╗
║   SpeedRunnersLab CI/CD 本地一键配置                       ║
║   预计耗时：5~10 分钟                                       ║
╚══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Magenta

# ============================================================
Write-Step 1 6 "环境自检"
# ============================================================

$sshOK     = (Get-Command ssh -ErrorAction SilentlyContinue)        -ne $null
$keygenOK  = (Get-Command ssh-keygen -ErrorAction SilentlyContinue) -ne $null
$ghOK      = (Get-Command gh -ErrorAction SilentlyContinue)         -ne $null
$gitOK     = (Get-Command git -ErrorAction SilentlyContinue)        -ne $null

if (-not $sshOK)    { Write-Host "  ✗ 没找到 ssh，需要装 Git for Windows 或 Win10+ OpenSSH" -ForegroundColor Red; exit 1 }
if (-not $keygenOK) { Write-Host "  ✗ 没找到 ssh-keygen" -ForegroundColor Red; exit 1 }
if (-not $gitOK)    { Write-Host "  ✗ 没找到 git" -ForegroundColor Red; exit 1 }
Write-Ok "ssh / ssh-keygen / git 都在"

if ($ghOK) {
    Write-Ok "GitHub CLI (gh) 已安装，可以全自动写 Secrets"
    # 确认 gh 已登录
    $ghAuth = & gh auth status 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Warn "gh 还没登录，现在跳转浏览器登录..."
        & gh auth login
        if ($LASTEXITCODE -ne 0) { Write-Host "  ✗ gh 登录失败" -ForegroundColor Red; exit 1 }
    }
} else {
    Write-Warn "GitHub CLI (gh) 没装，将退到半自动模式（每个 Secret 复制到剪贴板，你手动粘贴）"
    Write-Info "推荐装 gh：winget install --id GitHub.cli  （5 分钟一次性投入，省后续）"
    $install = Ask "现在要不要装 gh？输 y 自动装 / 输 n 继续半自动 (y/n)"
    if ($install -eq "y") {
        & winget install --id GitHub.cli --silent --accept-package-agreements --accept-source-agreements
        Write-Warn "装完了。请关掉这个 PowerShell 窗口，开一个新窗口重新跑本脚本（让 PATH 生效）"
        exit 0
    }
}

# ============================================================
Write-Step 2 6 "生成专用部署 SSH key"
# ============================================================

$keyPath = "$env:USERPROFILE\.ssh\srlab_deploy"
if (Test-Path $keyPath) {
    Write-Warn "已存在 $keyPath，复用现有 key"
} else {
    & ssh-keygen -t ed25519 -C "github-actions-srlab-deploy" -f $keyPath -N '""' | Out-Null
    Write-Ok "生成完成：$keyPath"
}

$pubKey  = (Get-Content "$keyPath.pub" -Raw).Trim()
$privKey = (Get-Content $keyPath -Raw)

# ============================================================
Write-Step 3 6 "把公钥追加到 ECS"
# ============================================================

Write-Host ""
Write-Host "  以下公钥已复制到剪贴板：" -ForegroundColor Yellow
Write-Host "    $pubKey" -ForegroundColor DarkGray
$pubKey | Set-Clipboard
Write-Host ""
Write-Host "  请打开另一个终端，SSH 进你的 ECS（用现在的方式），执行：" -ForegroundColor Yellow
Write-Host ""
Write-Host "    echo `"<Ctrl+V 粘贴公钥>`" >> ~/.ssh/authorized_keys && chmod 600 ~/.ssh/authorized_keys" -ForegroundColor White
Write-Host ""
Pause-Enter "完成上面操作后按 Enter"

# ============================================================
Write-Step 4 6 "测试 SSH 连通性"
# ============================================================

$ecsHost = Ask "请输入 ECS 公网 IP 或域名"
Write-Info "正在用新 key 测试连接 root@$ecsHost ..."
& ssh -i $keyPath -o StrictHostKeyChecking=accept-new -o ConnectTimeout=10 "root@$ecsHost" "echo connected"
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ SSH 连接失败。可能是：公钥没追加成功 / ECS 安全组没开 22 端口 / IP 输错了" -ForegroundColor Red
    exit 1
}
Write-Ok "SSH 连通"

# ============================================================
Write-Step 5 6 "收集阿里云 ACR 信息"
# ============================================================

Write-Host ""
Write-Host "  如果还没在阿里云开通 ACR，现在打开浏览器开通：" -ForegroundColor Yellow
Write-Host "    1. 访问 https://cr.console.aliyun.com" -ForegroundColor White
Write-Host "    2. 选 '个人实例'（不要选企业版）" -ForegroundColor White
Write-Host "    3. 设置 Registry 登录密码（记下来！）" -ForegroundColor White
Write-Host "    4. 创建命名空间 srlab（私有）" -ForegroundColor White
Write-Host "    5. 创建 3 个仓库：srlab-api / srlab-ui / srlab-scheduler" -ForegroundColor White
Write-Host "    6. 确认仓库区域和你 ECS 相同（如都是 cn-hangzhou）" -ForegroundColor White
Write-Host ""
$openBrowser = Ask "现在要打开 ACR 控制台吗？(y/n)"
if ($openBrowser -eq "y") {
    Start-Process "https://cr.console.aliyun.com"
    Pause-Enter "完成 ACR 配置后按 Enter 继续"
}

Write-Host ""
$registry  = Ask "ACR Registry 地址 (如 registry.cn-hangzhou.aliyuncs.com)"
$namespace = Ask "命名空间 (如 srlab)"
$acrUser   = Ask "ACR 用户名 (如 xxx@aliyun.com)"
$acrPass   = Ask "ACR 密码" $true

# ============================================================
Write-Step 6 6 "写入 GitHub Secrets"
# ============================================================

$secrets = [ordered]@{
    "ALIYUN_REGISTRY"   = $registry
    "ALIYUN_NAMESPACE"  = $namespace
    "ALIYUN_USERNAME"   = $acrUser
    "ALIYUN_PASSWORD"   = $acrPass
    "SSH_HOST"          = $ecsHost
    "SSH_PRIVATE_KEY"   = $privKey
}

if ($ghOK) {
    # gh CLI 自动写
    $repo = & git config --get remote.origin.url
    $repo = $repo -replace '\.git$', '' -replace '^.*github.com[:/]', ''
    Write-Info "目标仓库：$repo"
    foreach ($k in $secrets.Keys) {
        $v = $secrets[$k]
        $v | & gh secret set $k --repo $repo
        if ($LASTEXITCODE -eq 0) {
            Write-Ok "Secret $k 已写入"
        } else {
            Write-Host "  ✗ Secret $k 写入失败" -ForegroundColor Red
        }
    }
} else {
    # 半自动：依次复制到剪贴板，提示去 GitHub UI 粘贴
    $repoUrl = (& git config --get remote.origin.url) -replace '\.git$', ''
    $secretsUrl = "$repoUrl/settings/secrets/actions/new"
    Write-Host ""
    Write-Host "  接下来在浏览器打开：$secretsUrl" -ForegroundColor Yellow
    Write-Host "  对下面每个 Secret，本脚本会把值复制到剪贴板，你只需要：" -ForegroundColor Yellow
    Write-Host "    1. 点 'New repository secret'" -ForegroundColor Yellow
    Write-Host "    2. Name 字段输入下面的名字" -ForegroundColor Yellow
    Write-Host "    3. Value 字段 Ctrl+V" -ForegroundColor Yellow
    Write-Host "    4. 点 'Add secret'，回这边按 Enter 进入下一个" -ForegroundColor Yellow
    Pause-Enter "现在打开浏览器后按 Enter 开始"
    Start-Process $secretsUrl
    foreach ($k in $secrets.Keys) {
        $secrets[$k] | Set-Clipboard
        Write-Host ""
        Write-Host "  >> Secret 名字：$k （值已在剪贴板）" -ForegroundColor Magenta
        Pause-Enter "粘贴并保存后按 Enter"
    }
}

# ============================================================
Write-Host ""
Write-Host @"

╔══════════════════════════════════════════════════════════╗
║   本地配置完成 ✓                                            ║
╠══════════════════════════════════════════════════════════╣
║   下一步：                                                  ║
║                                                            ║
║   1. SSH 进 ECS：                                            ║
║        ssh -i $keyPath root@$ecsHost
║                                                            ║
║   2. 在 ECS 上执行一键脚本：                                  ║
║        cd /root/home/srlab                                  ║
║        # 把仓库 git 化 + 装 docker compose v2 + docker login   ║
║        curl -fsSL https://raw.githubusercontent.com/<你的GH用户>/<仓库>/master/scripts/setup-ecs.sh | bash  ║
║                                                            ║
║   3. 浏览器打开 GitHub Actions，点 'Run workflow' 开始首次发版  ║
║                                                            ║
╚══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Green
