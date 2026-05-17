#requires -Version 5.1
# 本地一次性配置脚本：生成 SSH key + 写入 GitHub Secrets
# 方案：ghcr.io（不需要阿里云 ACR）
#
# 用法（推荐）：
#   powershell -NoExit -ExecutionPolicy Bypass -File .\scripts\setup-local.ps1
#
# 重要：本文件以 UTF-8 with BOM 保存，PowerShell 5.1 才能正确解析中文。
# 修改后如果跑出乱码，记得保存为"带签名的 UTF-8 / UTF-8 with BOM"。

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

function Write-Step { param($n, $total, $msg)
    Write-Host ""
    Write-Host "[$n/$total] $msg" -ForegroundColor Cyan
}
function Write-Ok   { param($msg) Write-Host "  [OK] $msg"   -ForegroundColor Green  }
function Write-Info { param($msg) Write-Host "  -    $msg"   -ForegroundColor Gray   }
function Write-Warn { param($msg) Write-Host "  [!]  $msg"   -ForegroundColor Yellow }
function Write-Err  { param($msg) Write-Host "  [X]  $msg"   -ForegroundColor Red    }
function Ask        { param($prompt) return Read-Host -Prompt "  $prompt" }
function PauseEnter { param($msg = "按 Enter 继续") Read-Host "  >>> $msg" | Out-Null }

Write-Host ""
Write-Host "================================================================" -ForegroundColor Magenta
Write-Host "  SpeedRunnersLab CI/CD 本地一键配置 (ghcr.io 方案)              " -ForegroundColor Magenta
Write-Host "  预计耗时：5 分钟                                                " -ForegroundColor Magenta
Write-Host "================================================================" -ForegroundColor Magenta

# ============================================================
Write-Step 1 5 "环境自检"
# ============================================================

$sshOK     = $null -ne (Get-Command ssh         -ErrorAction SilentlyContinue)
$keygenOK  = $null -ne (Get-Command ssh-keygen  -ErrorAction SilentlyContinue)
$ghOK      = $null -ne (Get-Command gh          -ErrorAction SilentlyContinue)
$gitOK     = $null -ne (Get-Command git         -ErrorAction SilentlyContinue)

if (-not $sshOK)    { Write-Err "没找到 ssh，需要装 Git for Windows";   exit 1 }
if (-not $keygenOK) { Write-Err "没找到 ssh-keygen";                    exit 1 }
if (-not $gitOK)    { Write-Err "没找到 git";                            exit 1 }
Write-Ok "ssh / ssh-keygen / git 都在"

if ($ghOK) {
    Write-Ok "GitHub CLI (gh) 已安装"
    $null = & gh auth status 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Warn "gh 还没登录，现在跳转浏览器登录..."
        & gh auth login
        if ($LASTEXITCODE -ne 0) { Write-Err "gh 登录失败"; exit 1 }
    }
} else {
    Write-Warn "GitHub CLI (gh) 没装，会退到半自动模式（剪贴板复制 + 手动粘贴）"
    Write-Info "想全自动？装 gh： winget install --id GitHub.cli"
    $install = Ask "现在装 gh？(y/n)"
    if ($install -eq "y") {
        & winget install --id GitHub.cli --silent --accept-package-agreements --accept-source-agreements
        Write-Warn "安装完成。关掉本窗口，开新 PowerShell 重跑本脚本（让 PATH 生效）"
        exit 0
    }
}

# ============================================================
Write-Step 2 5 "生成专用部署 SSH key"
# ============================================================

$keyPath = Join-Path $env:USERPROFILE ".ssh\srlab_deploy"
$keyPub  = "$keyPath.pub"

if (Test-Path $keyPath) {
    Write-Warn "已存在 $keyPath，复用现有 key"
} else {
    $sshDir = Split-Path $keyPath -Parent
    if (-not (Test-Path $sshDir)) { New-Item -ItemType Directory -Path $sshDir | Out-Null }
    # -N 后面跟一对单引号代表空密码
    & ssh-keygen -t ed25519 -C "github-actions-srlab-deploy" -f $keyPath -N '""' | Out-Null
    if (-not (Test-Path $keyPath)) { Write-Err "ssh-keygen 没生成 key 文件"; exit 1 }
    Write-Ok "生成完成：$keyPath"
}

$pubKey  = (Get-Content $keyPub -Raw).Trim()
$privKey = (Get-Content $keyPath -Raw)

# ============================================================
Write-Step 3 5 "把公钥追加到 ECS"
# ============================================================

$pubKey | Set-Clipboard
Write-Host ""
Write-Host "  公钥已复制到剪贴板，内容：" -ForegroundColor Yellow
Write-Host "    $pubKey" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  另开一个终端，用你现在的方式 SSH 进 ECS，依次执行以下 3 行：" -ForegroundColor Yellow
Write-Host ""
Write-Host "    mkdir -p ~/.ssh"                                            -ForegroundColor White
Write-Host "    echo '<在这里粘贴剪贴板里的公钥>' >> ~/.ssh/authorized_keys" -ForegroundColor White
Write-Host "    chmod 600 ~/.ssh/authorized_keys"                           -ForegroundColor White
Write-Host ""
PauseEnter "完成上面 3 行后按 Enter"

# ============================================================
Write-Step 4 5 "测试 SSH 连通性"
# ============================================================

$ecsHost = Ask "请输入 ECS 公网 IP 或域名"
if ([string]::IsNullOrWhiteSpace($ecsHost)) { Write-Err "ECS IP 不能为空"; exit 1 }

Write-Info "用新 key 测试连接 root@$ecsHost ..."
& ssh -i $keyPath -o StrictHostKeyChecking=accept-new -o ConnectTimeout=10 "root@$ecsHost" "echo connected"
if ($LASTEXITCODE -ne 0) {
    Write-Err "SSH 连接失败。可能原因：公钥没追加成功 / ECS 安全组没开 22 / IP 输错"
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

$repoUrl = (& git config --get remote.origin.url)
$repoSlug = $repoUrl -replace '\.git$', '' -replace '^.*github\.com[:/]', ''

if ($ghOK) {
    Write-Info "目标仓库：$repoSlug"
    foreach ($k in $secrets.Keys) {
        $secrets[$k] | & gh secret set $k --repo $repoSlug
        if ($LASTEXITCODE -eq 0) { Write-Ok "Secret $k 已写入" }
        else                     { Write-Err "Secret $k 写入失败" }
    }
} else {
    $repoBase   = $repoUrl -replace '\.git$', ''
    $secretsUrl = "$repoBase/settings/secrets/actions/new"
    Write-Host ""
    Write-Host "  浏览器即将打开：$secretsUrl"                                      -ForegroundColor Yellow
    Write-Host "  本脚本会把每个 Secret 的值复制到剪贴板，你只要：" -ForegroundColor Yellow
    Write-Host "    1. 点页面 New repository secret"                                -ForegroundColor Yellow
    Write-Host "    2. Name 字段输入下面给你的名字"                                  -ForegroundColor Yellow
    Write-Host "    3. Value 字段 Ctrl+V"                                            -ForegroundColor Yellow
    Write-Host "    4. 点 Add secret，回到本窗口按 Enter 进入下一个"                  -ForegroundColor Yellow
    PauseEnter "准备好后按 Enter 打开浏览器"
    Start-Process $secretsUrl
    foreach ($k in $secrets.Keys) {
        $secrets[$k] | Set-Clipboard
        Write-Host ""
        Write-Host "  >> Secret 名字： $k  （值已在剪贴板）" -ForegroundColor Magenta
        PauseEnter "粘贴并保存后按 Enter"
    }
}

# ============================================================
Write-Host ""
Write-Host "================================================================" -ForegroundColor Green
Write-Host "  本地配置完成 [OK]                                              " -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  下一步：" -ForegroundColor Green
Write-Host ""
Write-Host "  1. SSH 进 ECS：" -ForegroundColor White
Write-Host "       ssh -i $keyPath root@$ecsHost" -ForegroundColor Cyan
Write-Host ""
Write-Host "  2. 在 ECS 上执行（先确保改动已 push 到 GitHub）：" -ForegroundColor White
Write-Host "       cd /root/home/srlab" -ForegroundColor Cyan
Write-Host "       curl -fsSL https://raw.githubusercontent.com/TinyMaD/SpeedRunnersLab/master/scripts/setup-ecs.sh | bash" -ForegroundColor Cyan
Write-Host ""
Write-Host "  3. 浏览器打开 Actions 页面，点 Run workflow：" -ForegroundColor White
Write-Host "       https://github.com/TinyMaD/SpeedRunnersLab/actions" -ForegroundColor Cyan
Write-Host ""
