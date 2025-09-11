# Etapa base (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Etapa build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# TIP: si tienes ProjectReference a Application/Domain/etc, o copias el .sln y todo,
# o copias los .csproj referenciados. Lo más simple: copia todo y restaura.
COPY . .
WORKDIR "/src/apps/frontend/src/Morning.Value.Web.Site"
RUN dotnet restore "Morning.Value.Web.Site.csproj"
RUN dotnet build "Morning.Value.Web.Site.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Etapa publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/apps/frontend/src/Morning.Value.Web.Site"
RUN dotnet publish "Morning.Value.Web.Site.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Para evitar líos con HTTPS en contenedor, sirve HTTP en 8080.
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Morning.Value.Web.Site.dll"]
