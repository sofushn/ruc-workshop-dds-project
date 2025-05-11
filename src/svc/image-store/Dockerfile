FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
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
COPY --from=publish /app/publish .
COPY Images /images

USER root
RUN chmod 777 /images
RUN chown -R $APP_UID /images

USER $APP_UID

ENTRYPOINT ["dotnet", "ImageStoreAPI.dll"]
