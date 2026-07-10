FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json .
RUN npm ci
COPY . .
RUN npm run build -- --configuration production

FROM nginxinc/nginx-unprivileged:alpine AS final
COPY --from=build /app/dist/LibraryFrontend/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
