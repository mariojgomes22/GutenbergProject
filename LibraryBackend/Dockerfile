FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY LibraryBackend.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# OpenShift runs containers with a random UID in group 0, so the SQLite
# folder must be group-writable.
RUN mkdir /data && chgrp -R 0 /data && chmod -R g=u /data

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__DefaultConnection="Data Source=/data/library.db"

USER 1001
EXPOSE 8080
ENTRYPOINT ["dotnet", "LibraryBackend.dll"]
