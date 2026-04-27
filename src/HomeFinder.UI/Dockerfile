FROM node:20-alpine AS build
WORKDIR /app

COPY src/HomeFinder.UI/package*.json ./
RUN npm ci

COPY src/HomeFinder.UI/ ./

ARG VITE_API_BASE_URL=""
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}

RUN npm run build

FROM nginx:1.27-alpine AS final

COPY src/HomeFinder.UI/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
