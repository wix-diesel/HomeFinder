FROM node:20-alpine AS build
WORKDIR /app

COPY src/HomeFinder.UI/package.json ./
COPY src/HomeFinder.UI/pnpm-lock.yaml ./
RUN npm i -g pnpm && pnpm install --frozen-lockfile

COPY src/HomeFinder.UI/ ./
ARG VITE_API_BASE_URL=""
ARG VITE_AZURE_CLIENT_ID=""
ARG VITE_AZURE_TENANT_ID=""
ARG VITE_AZURE_REDIRECT_URI=""
ARG VITE_AZURE_SCOPES=""
ARG VITE_AZURE_API_SCOPE=""
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}
ENV VITE_AZURE_CLIENT_ID=${VITE_AZURE_CLIENT_ID}
ENV VITE_AZURE_TENANT_ID=${VITE_AZURE_TENANT_ID}
ENV VITE_AZURE_REDIRECT_URI=${VITE_AZURE_REDIRECT_URI}
ENV VITE_AZURE_SCOPES=${VITE_AZURE_SCOPES}
ENV VITE_AZURE_API_SCOPE=${VITE_AZURE_API_SCOPE}
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY src/HomeFinder.UI/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
COPY src/HomeFinder.UI/runtime-env.sh /usr/share/nginx/html/runtime-env.sh
RUN sed -i 's/\r$//' /usr/share/nginx/html/runtime-env.sh && chmod +x /usr/share/nginx/html/runtime-env.sh
EXPOSE 80
CMD ["/bin/sh", "/usr/share/nginx/html/runtime-env.sh"]