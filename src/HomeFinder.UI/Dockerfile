FROM nginx:1.27-alpine AS final

COPY src/HomeFinder.UI/nginx.conf /etc/nginx/conf.d/default.conf
COPY src/HomeFinder.UI/dist /usr/share/nginx/html
COPY src/HomeFinder.UI/runtime-env.sh /usr/share/nginx/html/runtime-env.sh
RUN sed -i 's/\r$//' /usr/share/nginx/html/runtime-env.sh && chmod +x /usr/share/nginx/html/runtime-env.sh

EXPOSE 80
CMD ["/bin/sh", "/usr/share/nginx/html/runtime-env.sh"]
