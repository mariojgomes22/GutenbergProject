FROM docker.io/library/node:22-alpine AS build
WORKDIR /src
COPY package.json package-lock.json ./
RUN npm ci
COPY . .

# Entra ID settings are compiled into the bundle, so they are passed as build args.
ARG MSAL_CLIENT_ID
ARG MSAL_TENANT_ID
ARG MSAL_REDIRECT_URI
RUN test -n "$MSAL_CLIENT_ID" && test -n "$MSAL_TENANT_ID" && test -n "$MSAL_REDIRECT_URI" \
 && printf "export const environment = {\n    production: true,\n    apiUrl: '/api',\n    msalConfig: {\n        clientId: '%s',\n        tenantId: '%s',\n        redirectUri: '%s'\n    },\n    apiScope: 'api://%s/access_as_user'\n};\n" \
      "$MSAL_CLIENT_ID" "$MSAL_TENANT_ID" "$MSAL_REDIRECT_URI" "$MSAL_CLIENT_ID" \
      > src/environments/environment.prod.ts \
 && cp src/environments/environment.prod.ts src/environments/environment.ts
RUN npx ng build --configuration production

FROM docker.io/nginxinc/nginx-unprivileged:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /src/dist/LibraryFrontend/browser /usr/share/nginx/html
EXPOSE 8080
