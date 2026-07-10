FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY LibraryBackend.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/data \
    && chgrp -R 0 /app \
    && chmod -R g=u /app
EXPOSE 5214
ENTRYPOINT ["dotnet", "LibraryBackend.dll"]
