# 多阶段构建：build 阶段跑 yarn build:prod，runtime 阶段只带 nginx + dist
# CI 构建命令：docker build -t srlab-ui ./SpeedRunners.UI

# ===== Build stage =====
# 用 node 14 而非 16：node-sass 4.9 + vue-cli 3 + webpack 4 是 node 14 时代的栈
# node 16+ 上 node-sass 4.x 编译困难，且需要 --openssl-legacy-provider；node 14 都不用
FROM node:14.21.3-alpine3.16 AS build
WORKDIR /app

# node-gyp 编译原生模块（node-sass / deasync）需要的工具链
# 老版 node-gyp 只识别 python/python2，不认 python3，所以建符号链接
RUN apk add --no-cache python3 make g++ && \
    ln -sf /usr/bin/python3 /usr/bin/python

# 先只复制依赖描述，命中 yarn install 缓存
COPY package.json yarn.lock ./

RUN yarn install --frozen-lockfile --ignore-engines --network-timeout 600000

# 再复制源码并构建
COPY . .
RUN yarn build:prod

# ===== Runtime stage =====
FROM nginx:stable-alpine AS runtime

# 把构建产物复制到 nginx 默认根目录
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
EXPOSE 443

CMD ["nginx", "-g", "daemon off;"]
