# See https://aka.ms/customizecontainer

# Base runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# ✅ FIXED PATH (IMPORTANT)
COPY ["InventoryHub.csproj", "./"]

RUN dotnet restore "InventoryHub.csproj"

# Copy full source
COPY . .

WORKDIR "/src"
RUN dotnet build "InventoryHub.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InventoryHub.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "InventoryHub.dll"]