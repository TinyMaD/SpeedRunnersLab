#requires -Version 5.1
<#
.SYNOPSIS
  本地一次性配置脚本：生成 SSH key、写入 GitHub Secrets。
.DESCRIPTION
  ghcr.io 方案：镜像放 GitHub 自己的镜像仓库（完全免费，CI 自动用 GITHUB_TOKEN 推），
  所以本地只需要配 2 个 Secret：SSH_HOST + SSH_PRIVATE_KEY。
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
function Ask($prompt) { return Read-Host -Prompt "  $prompt" }
function Pause-Enter($msg = "按 Enter 继续") {
    Read-Host "  >>> $msg"
}

Write-Host @"

╔══════════════════════════════════════════════════════════╗
║   SpeedRunnersLab CI/CD 本地一键配置                       ║
║   方案：GitHub Container Registry (ghcr.io)                ║
║   预计耗时：5 分钟                                          ║
╚══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Magenta

# ============================================================
Write-Step 1 5 "环境自检"
# ============================================================

$sshOK     = (Get-Command ssh -ErrorAction SilentlyContinue)        -ne $null
$keygenOK  = (Get-Command ssh-keygen -ErrorAction SilentlyContinue) -ne $null
$ghOK      = (Get-Command gh -ErrorAction SilentlyContinue)         -ne $null
$gitOK     = (Get-Command git -ErrorAction SilentlyContinue)        -ne $null

if (-not $sshOK)    { Write-Host "  ✗ 没找到 ssh，需要装 Git for Windows" -ForegroundColor Red; exit 1 }
if (-not $keygenOK) { Write-Host "  ✗ 没找到 ssh-keygen" -ForegroundColor Red; exit 1 }
if (-not $gitOK)    { Write-Host "  ✗ 没找到 git" -ForegroundColor Red; exit 1 }
Write-Ok "ssh / ssh-keygen / git 都在"

if ($ghOK) {
    Write-Ok "GitHub CLI (gh) 已安装"
    $ghAuth = & gh auth status 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Warn "gh 还没登录，现在跳转浏览器登录..."
        & gh auth login
        if ($LASTEXITCODE -ne 0) { Write-Host "  ✗ gh 登录失败" -ForegroundColor Red; exit 1 }
    }
} else {
    Write-Warn "GitHub CLI (gh) 没装，会退到手动模式（每个 Secret 复制到剪贴板让你粘贴）"
    Write-Info "想全自动？装 gh：winget install --id GitHub.cli"
    $install = Ask "现在装 gh？输 y 自动装 / 输 n 继续手动 (y/n)"
    if ($install -eq "y") {
        & winget install --id GitHub.cli --silent --accept-package-agreements --accept-source-agreements
        Write-Warn "装完了。关掉这个窗口，开新 PowerShell 重跑本脚本（让 PATH 生效）"
        exit 0
    }
}

# ============================================================
Write-Step 2 5 "生成专用部署 SSH key"
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
Write-Step 3 5 "把公钥追加到 ECS"
# ============================================================

Write-Host ""
Write-Host "  公钥已复制到剪贴板：" -ForegroundColor Yellow
Write-Host "    $pubKey" -ForegroundColor DarkGray
$pubKey | Set-Clipboard
Write-Host ""
Write-Host "  打开另一个终端，SSH 进你的 ECS（用现在的方式），执行：" -ForegroundColor Yellow
Write-Host ""
Write-Host "    mkdir -p ~/.ssh && echo `"<Ctrl+V 粘贴公钥>`" >> ~/.ssh/authorized_keys && chmod 600 ~/.ssh/authorized_keys" -ForegroundColor White
Write-Host ""
Pause-Enter "完成后按 Enter"

# ============================================================
Write-Step 4 5 "测试 SSH 连通性"
# ============================================================

$ecsHost = Ask "请输入 ECS 公网 IP 或域名"
Write-Info "用新 key 测试连接 root@$ecsHost ..."
& ssh -i $keyPath -o StrictHostKeyChecking=accept-new -o ConnectTimeout=10 "root@$ecsHost" "echo connected"
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ SSH 连接失败。可能：公钥没追加成功 / ECS 安全组没开 22 / IP 输错" -ForegroundColor Red
    exit 1
}
Write-Ok "SSH 连通"

# ============================================================
Write-Step 5 5 "写入 GitHub Secrets"
# ============================================================

$secrets = [ordered]@{
    "SSH_HOST"        = $ecsHost
    "SSH_PRIVATE_KEY" = $privKey
}

if ($ghOK) {
    $repo = (& git config --get remote.origin.url) -replace '\.git$', '' -replace '^.*github.com[:/]', ''
    Write-Info "目标仓库：$repo"
    foreach ($k in $secrets.Keys) {
        $secrets[$k] | & gh secret set $k --repo $repo
        if ($LASTEXITCODE -eq 0) {
            Write-Ok "Secret $k 已写入"
        } else {
            Write-Host "  ✗ Secret $k 写入失败" -ForegroundColor Red
        }
    }
} else {
    $repoUrl = (& git config --get remote.origin.url) -replace '\.git$', ''
    $secretsUrl = "$repoUrl/settings/secrets/actions/new"
    Write-Host ""
    Write-Host "  浏览器打开：$secretsUrl" -ForegroundColor Yellow
    Write-Host "  本脚本会把每个 Secret 值复制到剪贴板，你只需要：" -ForegroundColor Yellow
    Write-Host "    1. 点 'New repository secret'" -ForegroundColor Yellow
    Write-Host "    2. Name 字段输入下面的名字" -ForegroundColor Yellow
    Write-Host "    3. Value 字段 Ctrl+V" -ForegroundColor Yellow
    Write-Host "    4. 点 'Add secret'，回这边按 Enter 进入下一个" -ForegroundColor Yellow
    Pause-Enter "打开浏览器后按 Enter 开始"
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
║   2. 在 ECS 上执行一键脚本（先确保改动已 push 到 GitHub）：    ║
║        cd /root/home/srlab                                  ║
║        curl -fsSL https://raw.githubusercontent.com/TinyMaD/SpeedRunnersLab/master/scripts/setup-ecs.sh | bash
║                                                            ║
║   3. 浏览器打开 GitHub Actions，点 'Run workflow' 开始首次发版  ║
║      https://github.com/TinyMaD/SpeedRunnersLab/actions
║                                                            ║
╚══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Green
