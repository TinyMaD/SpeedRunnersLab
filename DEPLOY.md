# 发版操作手册

把手动发版改造为 GitHub Actions 自动发版到阿里云 ECS。
**面向：项目维护者 / 第一次配置 CI/CD 的人。**

---

## ⚡ 极简流程（约 15 分钟一次性配置，之后每次发版 10 秒）

### Step 1：开通阿里云 ACR（5 分钟，必须手动）

> ACR (Container Registry) = 放 Docker 镜像的云端仓库，个人版**完全免费**。

1. 打开 https://cr.console.aliyun.com，用阿里云账号登录
2. 左侧选 **个人实例**（**不要**选企业版，要钱）
3. 第一次进会让你 **设置 Registry 登录密码** → **记下来**（独立于阿里云账号密码，专门用来 `docker login`）
4. 左侧 **命名空间** → 创建 → 名字写 `srlab` → 类型选 **私有**
5. 左侧 **镜像仓库** → 创建 3 个，命名空间都选 `srlab`：
   - `srlab-api`、`srlab-ui`、`srlab-scheduler`
   - 代码源都选 **本地仓库**
6. **关键**：确认右上角区域和你 ECS **同一个**（如都是 `华东1(杭州)`）

记下这 4 条信息备用：
- Registry 地址（如 `registry.cn-hangzhou.aliyuncs.com`）
- 命名空间（`srlab`）
- 用户名（看任一仓库详情页 → 操作指南第一步会显示）
- 密码（你刚才设的 Registry 密码）

---

### Step 2：本地跑一键脚本（5 分钟）

在项目根目录的 PowerShell 里：

```powershell
.\scripts\setup-local.ps1
```

脚本会引导你完成所有事：
- 生成专用 SSH key（自动）
- 把公钥放剪贴板，让你粘到 ECS（30 秒，手动）
- 测试 SSH 连通性（自动）
- 输入 ACR 信息（4 个值）
- 写入 GitHub 的 6 个 Secrets（如装了 `gh` CLI 全自动；没装就引导你复制粘贴）

**装 GitHub CLI 能省事**（脚本会提示安装；可手动 `winget install --id GitHub.cli`）。

---

### Step 3：ECS 跑一键脚本（5 分钟）

SSH 进 ECS（脚本结束会给你完整命令），执行：

```bash
cd /root/home/srlab
bash scripts/setup-ecs.sh
```

但**第一次** `scripts/` 目录还不在 ECS 上（git pull 之前没有），所以用在线版：

```bash
cd /root/home/srlab
curl -fsSL https://raw.githubusercontent.com/<你的GitHub用户>/<仓库名>/master/scripts/setup-ecs.sh | bash
```

脚本会自动：
- 把 `/root/home/srlab` 改造成 git 仓库（绑你的 GitHub）
- 安装 docker compose v2（如果没装）
- 引导 docker login 到 ACR
- 检查 `appsettings.Production.json` / `App.config` 是否就位（不在的话警告你补）
- 清掉旧的本地构建产物

---

### Step 4：触发第一次发版（10 秒）

1. 浏览器打开你的 GitHub 仓库
2. 顶部 **Actions** 标签
3. 左侧选 **Build & Deploy to Production**
4. 右上角 **Run workflow** → 直接点绿色按钮（tag 留空）
5. 等 10~15 分钟（首次构建慢），看到全绿就成功

打开 https://www.speedrunners.cn 验证。

---

## 日常发版

之后每次发版只需要：
1. `git push origin master`
2. GitHub 仓库 → Actions → Run workflow（10 秒鼠标）
3. 等 3~5 分钟

完全不用 SSH 服务器、不用本地构建。

---

## 故障排查

### Step 2 脚本报错

| 报错 | 解决 |
|---|---|
| 没找到 ssh / ssh-keygen | 装 Git for Windows |
| gh 登录失败 | 手动 `gh auth login`，按提示浏览器登录 |
| SSH 连接失败 | 检查 ECS 安全组开 22 端口；公钥粘贴成功 |

### Step 3 脚本报错

| 报错 | 解决 |
|---|---|
| 期望在 /root/home/srlab 但当前在 X | `cd /root/home/srlab` 再跑 |
| ACR 登录失败 | 密码错。回 ACR 控制台 → 访问凭证 → 重置 |
| git fetch 网络超时 | ECS 拉 GitHub 慢，可以重试；或在 ECS 配 git 走代理 |

### Step 4 GitHub Actions 失败

打开失败的 job 看日志。常见：

| 现象 | 原因 / 解决 |
|---|---|
| build 阶段 ACR 登录 401 | Secret 里 ACR 密码错。重新跑 Step 2 或 GitHub UI 改 Secret |
| build-api 编译报错 | 本地先 `dotnet build` 跑一下确认能编 |
| build-ui yarn install 卡住 | 重跑（CI 国外网络偶发） |
| deploy 阶段 SSH 超时 | ECS 安全组 22 端口；或换用阿里云 CI 跑（GitHub Actions IP 在国外） |
| deploy 阶段 git pull 冲突 | ECS 上有人手动改了被 git 跟踪的文件。SSH 进去 `git status` 看，`git stash` 或 `git reset --hard origin/master` |
| 容器起不来 | SSH 进去 `docker compose -f docker-compose.prod.yml logs srlab.api --tail 100` |

### 想回滚

GitHub Actions 触发时填一个旧的短 sha（如 `a1b2c3d`）即可，会用旧镜像启动。

---

## 关键文件清单

### 永远不要提交到 git 的（只在 ECS 上）

| 文件 | 用途 |
|---|---|
| `/root/home/srlab/SpeedRunners.API/SpeedRunners/appsettings.Production.json` | 数据库连接、Steam key、Qiniu key、爱发电 token |
| `/root/home/srlab/SpeedRunners.Scheduler/App.config` | Scheduler 的数据库连接 |
| `/root/home/srlab/SpeedRunners.UI/nginx/*.pem` / `*.key` | api.speedrunners.cn SSL 证书 |
| `/root/home/srlab/mysql/` | MySQL 数据目录 |

`.gitignore` 已经全部忽略。

**强烈建议**：把这几个文件用密码管理器（或私密笔记）备份一份，丢了只能找你之前的副本。

### 本仓库新增/改造的文件

| 文件 | 作用 |
|---|---|
| `SpeedRunners.API/Dockerfile` | 多阶段构建（CI 在镜像里 publish） |
| `SpeedRunners.UI/Dockerfile` | 多阶段构建（CI 在镜像里 yarn build） |
| `SpeedRunners.Scheduler/Dockerfile` | 多阶段构建 |
| `SpeedRunners.UI/.dockerignore` | 避免 node_modules 污染 build |
| `docker-compose.yml` | **本地开发**用（build 模式），不变 |
| `docker-compose.prod.yml` | **ECS 生产**用（image 模式 + 配置文件挂载） |
| `.github/workflows/deploy.yml` | CI 流水线（手动触发 → build × 3 → SSH 部署） |
| `scripts/setup-local.ps1` | 本地一键配置脚本 |
| `scripts/setup-ecs.sh` | ECS 一键配置脚本 |

---

## 安全模型

> "我的仓库是公开的，密码会泄露吗？"

不会。三道屏障：

1. **GitHub Actions Secrets**：加密存储，公开仓库别人也读不到。Fork 你仓库的人在他的 Actions 里拿不到你的 Secrets
2. **私有 ACR**：镜像在私有仓库，别人 pull 不到
3. **敏感配置只在 ECS 本地**：`appsettings.Production.json` 等永不进镜像、永不进 git

整个 CI/CD 链路：
```
GitHub 公开仓库（代码） ──→ GitHub Actions（拿 Secrets）
                              ↓
                          推镜像 ──→ 阿里云 ACR（私有）
                              ↓
                          SSH ──→ ECS
                                    ↓
                                docker compose 启动容器
                                    ↑
                                挂载本地敏感配置
```
