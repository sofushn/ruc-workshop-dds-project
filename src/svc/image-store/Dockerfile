FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ImageStoreAPI.csproj", "."]
RUN dotnet restore "./ImageStoreAPI.csproj"
COPY . .
RUN dotnet build "./ImageStoreAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ImageStoreAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ARG APP_USER=appuser
ARG APP_UID=10001
RUN useradd -u ${APP_UID} ${APP_USER}

COPY --from=publish /app/publish .
COPY Images /app/Images

RUN chown -R ${APP_USER}:${APP_USER} /app/Images

USER ${APP_USER}

ENTRYPOINT ["dotnet", "ImageStoreAPI.dll"]
