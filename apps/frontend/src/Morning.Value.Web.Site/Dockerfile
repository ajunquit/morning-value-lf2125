# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["apps/frontend/src/Morning.Value.Web.Site/Morning.Value.Web.Site.csproj", "apps/frontend/src/Morning.Value.Web.Site/"]
COPY ["apps/backend/src/Morning.Value.Application/Morning.Value.Application.csproj", "apps/backend/src/Morning.Value.Application/"]
COPY ["apps/backend/src/Morning.Value.Domain/Morning.Value.Domain.csproj", "apps/backend/src/Morning.Value.Domain/"]
COPY ["apps/backend/src/Morning.Value.Infrastructure/Morning.Value.Infrastructure.csproj", "apps/backend/src/Morning.Value.Infrastructure/"]
RUN dotnet restore "./apps/frontend/src/Morning.Value.Web.Site/Morning.Value.Web.Site.csproj"
COPY . .
WORKDIR "/src/apps/frontend/src/Morning.Value.Web.Site"
RUN dotnet build "./Morning.Value.Web.Site.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Morning.Value.Web.Site.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Morning.Value.Web.Site.dll"]