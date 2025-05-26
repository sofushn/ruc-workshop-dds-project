FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source
COPY . .

RUN dotnet restore
RUN dotnet publish --no-restore -o /app

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=build /app .

USER $APP_UID
ENTRYPOINT ["dotnet", "Setup.dll"]
