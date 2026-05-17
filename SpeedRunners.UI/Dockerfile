# 多阶段构建：build 阶段跑 yarn build:prod，runtime 阶段只带 nginx + dist
# CI 构建命令：docker build -t srlab-ui ./SpeedRunners.UI

# ===== Build stage =====
FROM node:16.14.0-alpine3.15 AS build
WORKDIR /app

# 先只复制依赖描述，命中 yarn install 缓存
COPY package.json yarn.lock ./
RUN yarn install --frozen-lockfile --network-timeout 600000

# 再复制源码并构建
COPY . .

# vue-cli 在 node 16+ webpack 4 上需要 legacy provider
ENV NODE_OPTIONS=--openssl-legacy-provider
RUN yarn build:prod

# ===== Runtime stage =====
FROM nginx:stable-alpine AS runtime

# 把构建产物复制到 nginx 默认根目录
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
EXPOSE 443

CMD ["nginx", "-g", "daemon off;"]
