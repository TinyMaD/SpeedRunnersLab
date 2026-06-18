# 发版操作手册

> 当前推荐方案已经切到 ECS self-hosted runner：不再通过 GitHub Actions SSH 到服务器，也不需要日常开放 22 端口。新流程见 [DEPLOY_RUNNER.md](DEPLOY_RUNNER.md)。下面内容保留为旧 SSH 发版方案参考。

把手动发版改造为 GitHub Actions 自动发版到阿里云 ECS。
**方案：GitHub Container Registry (ghcr.io) —— 全链路在 GitHub 里，不需要阿里云 ACR。**

---

## ⚡ 极简流程（10 分钟一次性配置，之后每次发版 10 秒）

只有 **3 步**：

### Step 0：把这次改动 push 到 GitHub
```powershell
git add .
git commit -m "feat: 接入 GitHub Actions CI/CD"
git push origin master
```
（ECS 的一键脚本要从 GitHub 拉，没 push 上去拉不到）

### Step 1：本地双击一键脚本（5 分钟）
```powershell
.\scripts\setup-local.ps1
```
脚本做完所有事：
- 自动生成专用 SSH key
- 公钥放剪贴板让你粘到 ECS（30 秒手动）
- 测试 SSH 连通
- 写入 GitHub 的 2 个 Secret（装了 `gh` CLI 全自动；没装就引导你复制粘贴）

**推荐装 `gh`**（脚本会提示，一行 `winget install --id GitHub.cli`）。

### Step 2：ECS 跑一键脚本（3 分钟）

SSH 进 ECS（脚本结束会给你完整命令），执行：
```bash
cd /root/home/srlab
curl -fsSL https://raw.githubusercontent.com/TinyMaD/SpeedRunnersLab/master/scripts/setup-ecs.sh | bash
```
自动做：
- 把 `/root/home/srlab` 改造成 git 仓库
- 安装 docker compose v2（如果没装）
- 写入 `.env`
- 检查敏感配置是否就位

### Step 3：触发首次发版（10 秒）
浏览器打开 https://github.com/TinyMaD/SpeedRunnersLab/actions
→ 左侧 **Build & Deploy to Production** → 右上角 **Run workflow** → 直接点绿色按钮

等 10~15 分钟（首次构建慢）。

---

## 日常发版

之后每次只需要：
1. `git push origin master`
2. GitHub 仓库 → Actions → Run workflow（10 秒鼠标）
3. 等 3~5 分钟

---

## 换新云服务器迁移

数据库迁移（mysqldump）独立做，这里只覆盖**项目 + SSL 自动续期**。

### Step 1：旧机打包敏感配置（30 秒）

```bash
bash /root/home/srlab/scripts/backup-secrets.sh
# 产出 /root/srlab-secrets-bak-YYYYMMDD-HHMMSS.tar.gz
```

包里有：
- `/root/.acme.sh/`（账号 + 通配符证书 + 七牛 deploy hook 状态）
- `/root/deploy_qiniu_all.sh`（RenewHook）
- `appsettings.Production.json`、`App.config`、SSL 证书、`.env`

⚠ **此包含明文敏感信息**（七牛 AK/SK、数据库密码、阿里 DNS Key），scp 后立刻在两端删干净，别留在网盘/聊天工具/git 里。

### Step 2：拷到新机

```bash
scp /root/srlab-secrets-bak-*.tar.gz root@<新机IP>:/root/
rm /root/srlab-secrets-bak-*.tar.gz   # 旧机立刻删
```

### Step 3：新机一行命令搞定（5 分钟）

SSH 进新机，确保 `docker` `git` `curl` 装好（一般云镜像自带，没有就 `yum install -y docker git curl`），然后：

```bash
curl -fsSL https://raw.githubusercontent.com/TinyMaD/SpeedRunnersLab/master/scripts/bootstrap.sh \
  | bash -s /root/srlab-secrets-bak-YYYYMMDD-HHMMSS.tar.gz
```

脚本自动做：
1. 跑 `setup-ecs.sh`：git 初始化项目、装 docker compose v2、写 `.env`
2. 跑 `restore-secrets.sh`：解包 → 还原 `/root/.acme.sh/` + 证书 + appsettings + App.config + RenewHook 脚本 → 重建 acme.sh 的 cron（每天 5:00 自动续期）
3. `docker compose pull && up -d` 启动所有容器
4. 输出 `acme.sh --list` 让你看到下次续期日期

### Step 4：切流量（5 分钟）

| 改什么 | 在哪改 |
|---|---|
| GitHub Secret `SSH_HOST` 改新机 IP | github.com/TinyMaD/SpeedRunnersLab/settings/secrets/actions |
| 阿里云 DNS 把 `api.speedrunners.cn` A 记录指新机 IP | 阿里云控制台 |
| 七牛 CDN 源站 IP 改新机 | 七牛云控制台 |
| 新机阿里云安全组开 22/80/443 | 阿里云控制台 |

完成后浏览器开 `https://www.speedrunners.cn/` 验证。OK 就把老 ECS 释放。

### 备份 / 恢复脚本如果你想本地测一遍

不要在生产 ECS 直接测 restore（会覆盖现有配置）。要测就用临时 ECS 或本地 Linux。

---

## 为什么不用阿里云 ACR？

阿里云 2024 年末调整了政策，**新账号不再开放 ACR 个人版**，只能企业版（付费/试用）。

**ghcr.io 反而更省事**：

| 维度 | 阿里云 ACR | ghcr.io（当前方案） |
|---|---|---|
| 费用 | 个人版免费，但新账号开不了 | **完全免费**（公开仓库无限制） |
| 凭据 | 要开通账号 + 记 Registry 密码 | **CI 用 `GITHUB_TOKEN` 自动认证** |
| Secrets 数量 | 6 个 | **2 个**（只剩 SSH 相关） |
| ECS 拉镜像速度 | 同区域内网快 | 走公网，首次 5~8 分钟，之后只拉差异层快得多 |

对你的实际影响：单次发版从 3~5 分钟变成 5~8 分钟，能完全接受。

---

## 故障排查

### Step 1 本地脚本报错

| 报错 | 解决 |
|---|---|
| 没找到 ssh / ssh-keygen | 装 Git for Windows |
| gh 登录失败 | 手动 `gh auth login`，按提示浏览器登录 |
| SSH 连接失败 | ECS 安全组开 22 端口；公钥粘贴成功 |

### Step 2 ECS 脚本报错

| 报错 | 解决 |
|---|---|
| 期望在 /root/home/srlab 但当前在 X | `cd /root/home/srlab` 再跑 |
| git fetch 网络超时 | ECS 拉 GitHub 慢，重试；或 ECS 配 git proxy |
| curl 拉脚本失败 | 检查 Step 0 是否真的 push 上去了 |

### Step 3 GitHub Actions 失败

打开失败的 job 看日志。常见：

| 现象 | 原因 / 解决 |
|---|---|
| build 阶段权限错（403） | 仓库 Settings → Actions → General → Workflow permissions 选 "Read and write" |
| build-api 编译报错 | 本地先 `dotnet build` 确认能编 |
| build-ui yarn install 卡住 | 重跑（CI 偶发） |
| deploy 阶段 SSH 超时 | ECS 安全组开 22；GitHub Actions 用动态 IP，建议 0.0.0.0/0 允许 22 但只 key 登录 |
| deploy 阶段 git pull 冲突 | ECS 上有人手动改了被 git 跟踪的文件。SSH 进去 `git status`，stash 或 reset |
| 容器起不来 | SSH 进去 `docker compose -f docker-compose.prod.yml logs srlab.api --tail 100` |
| ECS pull 镜像 unauthorized | ghcr 镜像默认是 private。GitHub 个人页 → Packages → 选 srlab-api → Package settings → Change visibility → Public（重复 3 次：api/ui/scheduler） |

### 想回滚

GitHub Actions 触发时**输入框填一个旧的短 sha**（如 `a1b2c3d`），workflow 会用那个旧 tag 启动。

或者 SSH 进 ECS 手动：
```bash
cd /root/home/srlab
GHCR_OWNER=tinymad IMAGE_TAG=a1b2c3d docker compose -f docker-compose.prod.yml up -d
```

---

## 关于 ghcr 镜像可见性

**首次推送后**，ghcr.io 上的镜像默认是 **private**，意味着 ECS 必须先 `docker login ghcr.io` 才能 pull。

有两个选择：

**选项 A：设成 public（推荐，最省事）**
- 镜像里不包含任何敏感信息（敏感配置都是 volume 挂载）
- 反正代码已经在公开仓库，镜像公开影响为零
- ECS 不用 docker login，永远顺畅 pull

**操作**：首次 CI 跑完后，GitHub 个人页 → 顶部 **Packages** → 进 `srlab-api` → 右下角 **Package settings** → 滑到底 **Change visibility** → 选 **Public**。三个镜像各做一次。

**选项 B：保持 private**
- ECS 上要 `docker login ghcr.io -u TinyMaD -p <PAT>` 一次
- 需要在 GitHub 生成一个有 `read:packages` 权限的 PAT (Personal Access Token)
- 多一步设置

**推荐 A**。

---

## 关键文件清单

### 永远不要提交到 git 的（只在 ECS 上）

| 文件 | 用途 |
|---|---|
| `/root/home/srlab/SpeedRunners.API/SpeedRunners/appsettings.Production.json` | 数据库连接、Steam key、Qiniu key、爱发电 token |
| `/root/home/srlab/SpeedRunners.Scheduler/App.config` | Scheduler 的数据库连接 |
| `/root/home/srlab/SpeedRunners.UI/nginx/*.pem` / `*.key` | SSL 证书 |
| `/root/home/srlab/mysql/` | MySQL 数据目录 |
| `/root/home/srlab/.env` | `GHCR_OWNER=tinymad`（由 setup-ecs.sh 自动生成） |

`.gitignore` 已全部忽略。

**强烈建议**：把 `appsettings.Production.json` 和 `App.config` 用密码管理器备份一份。

### 本仓库的相关文件

| 文件 | 作用 |
|---|---|
| `SpeedRunners.{API,UI,Scheduler}/Dockerfile` | 多阶段构建（CI 在镜像里 publish/build） |
| `SpeedRunners.UI/.dockerignore` | 避免 node_modules 污染 build |
| `docker-compose.yml` | **本地开发**用（build 模式），不变 |
| `docker-compose.prod.yml` | **ECS 生产**用（拉 ghcr.io 镜像） |
| `.github/workflows/deploy.yml` | CI 流水线（手动触发 → build × 3 → SSH 部署） |
| `scripts/setup-local.ps1` | 本地一键配置脚本 |
| `scripts/setup-ecs.sh` | ECS 一键配置脚本 |
| `scripts/bootstrap.sh` | 换机迁移入口脚本（setup-ecs + restore-secrets + 启动） |
| `scripts/backup-secrets.sh` | 旧机：打包敏感配置 + acme.sh 状态 |
| `scripts/restore-secrets.sh` | 新机：解包 + 重建 acme.sh cron |

---

## 安全模型

> "我的仓库是公开的，密码会泄露吗？"

不会。三道屏障：

1. **GitHub Actions Secrets**：加密存储，公开仓库别人也读不到。Fork 你仓库的人在他的 Actions 里拿不到你的 Secrets
2. **GITHUB_TOKEN 仅在 workflow 运行时存在**：推完镜像即失效
3. **敏感配置只在 ECS 本地**：`appsettings.Production.json` 等永不进镜像、永不进 git

整个 CI/CD 链路：
```
GitHub 公开仓库（代码） ──→ GitHub Actions（GITHUB_TOKEN 自动）
                              ↓
                          推镜像 ──→ ghcr.io（GitHub 自家镜像仓库）
                              ↓
                          SSH ──→ ECS（用 SSH_PRIVATE_KEY）
                                    ↓
                                docker compose 启动容器
                                    ↑
                                挂载本地敏感配置
```
