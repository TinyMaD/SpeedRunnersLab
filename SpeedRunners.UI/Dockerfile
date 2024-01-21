FROM node:16.14.0-alpine3.15 as build-stage

WORKDIR /app

COPY package*.json ./

RUN yarn install --production

COPY . .

RUN yarn build:stage

FROM nginx:stable-alpine as production-stage

COPY --from=build-stage /app/dist /usr/share/nginx/html

COPY ./nginx/default.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx","-g","daemon off;"]  