FROM nginx@sha256:4c41e2d0a3a55b325ed572cd0200bdb72af1433c3daa2bca6ef8eea68ce504a2

COPY nginx.conf /etc/nginx/nginx.conf

USER nginx
EXPOSE 8080 8443

CMD ["nginx", "-g", "daemon off;"]
