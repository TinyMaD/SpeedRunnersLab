# FROM node:16.14.0-alpine3.15 as build-stage

# WORKDIR /app

# COPY package*.json ./

# RUN yarn config set registry https://registry.yarnpkg.com
# RUN yarn install

# COPY . .

# RUN yarn build:stage

FROM nginx:stable-alpine as production-stage

COPY /dist /usr/share/nginx/html

COPY ./nginx/default.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx","-g","daemon off;"]  