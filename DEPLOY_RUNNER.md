# Self-hosted Runner 发布方案

这个方案把构建和部署分开：

- GitHub hosted runner：构建 `srlab-api`、`srlab-ui`、`srlab-scheduler` 并推送到 `ghcr.io`。
- ECS self-hosted runner：只在服务器本机执行部署，不需要公网开放 `22`。

## 一次性初始化

先用现有方式 SSH 到 ECS 一次，确保仓库已经在 `/root/home/srlab`，并且已经执行过：

```bash
cd /root/home/srlab
git fetch origin
git reset --hard origin/master
bash scripts/setup-ecs.sh
```

然后创建一个 GitHub PAT，用于注册 self-hosted runner。

推荐用 fine-grained PAT：

- Repository access：只选 `TinyMaD/SpeedRunnersLab`
- Repository permissions：`Administration` 设为 `Read and write`

在 ECS 上启动 runner：

```bash
cd /root/home/srlab
bash scripts/setup-runner.sh
```

脚本会把 PAT 写到 `/root/srlab-runner.env`，权限是 `600`，不会放进 git 仓库目录。

## 日常发布

之后不需要开 `22`。

1. 本地 push 到 `master`
2. 打开 GitHub Actions
3. 运行 `Build & Deploy to Production`

deploy job 会调度到 ECS 上的 `speedrunners-prod` runner，然后执行：

```bash
bash scripts/deploy-prod.sh
```

## Runner 管理

```bash
docker ps --filter name=srlab-github-runner
docker logs --tail 100 srlab-github-runner
docker restart srlab-github-runner
```

停止 runner：

```bash
cd /root/home/srlab
docker compose -p srlab-runner --env-file /root/srlab-runner.env -f docker-compose.runner.yml down
```

## 权限控制

- `deploy` job 只跑在 `runs-on: [self-hosted, speedrunners-prod]`。
- workflow 只允许 `master` 分支部署。
- `production` environment 可在 GitHub Settings -> Environments 中配置 required reviewers。
- 不要让 pull request workflow 使用 `speedrunners-prod` label。

注意：runner 挂载了 `/var/run/docker.sock`，等价于可以控制宿主机 Docker。权限边界主要靠 GitHub 侧控制：谁能改 workflow、谁能 push `master`、谁能批准 `production`，谁就能部署生产。
