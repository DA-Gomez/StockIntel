# syntax=docker/dockerfile:1.7
#there are two stages 1. the build and 2.runtime

# -------- build --------
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy  csproj files first to use dockers layer caching.
# Restore depends only on these, if source changes but deps don't, we skip the slow restore step.
COPY *.sln ./
COPY src/StockIntel.Domain/*.csproj src/StockIntel.Domain/
COPY src/StockIntel.Application/*.csproj src/StockIntel.Application/
COPY src/StockIntel.Infrastructure/*.csproj src/StockIntel.Infrastructure/
COPY src/StockIntel.Api/*.csproj src/StockIntel.Api/
COPY tests/StockIntel.Domain.UnitTests/*.csproj tests/StockIntel.Domain.UnitTests/
COPY tests/StockIntel.Application.UnitTests/*.csproj tests/StockIntel.Application.UnitTests/
COPY tests/StockIntel.Api.IntegrationTests/*.csproj tests/StockIntel.Api.IntegrationTests/

RUN dotnet restore src/StockIntel.Api/StockIntel.Api.csproj

# copy everything else (source tree) and publish
COPY . .
RUN dotnet publish src/StockIntel.Api/StockIntel.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# -------- runtime --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Run as non-root user, for security
# RUN addgroup -S app && adduser -S app -G app #alpine already does this
USER app

COPY --from=build --chown=app:app /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "StockIntel.Api.dll"]

# --no-restore -> docker expects the packages restored in a previous layer to still be available
# relates to Layer caching