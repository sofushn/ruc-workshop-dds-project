# See https://aka.ms/custimizecontainer to learn how to custimize and how VS uses this Dockerfile to build your images for faster debugging.

# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
USER $APP_UID
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["api.csproj", "."]
RUN dotnet restore
COPY . .

RUN dotnet build "api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "api.dll"]
