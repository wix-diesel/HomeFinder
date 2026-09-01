FROM node:22-alpine AS builder

WORKDIR /app

COPY src/HomeFinder.UI/package.json src/HomeFinder.UI/pnpm-lock.yaml ./
RUN corepack enable && corepack prepare pnpm@10.8.1 --activate && pnpm install --frozen-lockfile

COPY src/HomeFinder.UI/ .
RUN pnpm run build

FROM nginx:1.27-alpine AS final

COPY src/HomeFinder.UI/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=builder /app/dist /usr/share/nginx/html
COPY src/HomeFinder.UI/runtime-env.sh /usr/share/nginx/html/runtime-env.sh
RUN sed -i 's/\r$//' /usr/share/nginx/html/runtime-env.sh && chmod +x /usr/share/nginx/html/runtime-env.sh

EXPOSE 80
CMD ["/bin/sh", "/usr/share/nginx/html/runtime-env.sh"]
