# =========================
# Build Stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy project file first
# This allows Docker to cache the restore layer
COPY ["WebApplication1.csproj", "./"]

# Restore NuGet packages
RUN dotnet restore "./WebApplication1.csproj"

# Copy source code
COPY . .

# Build + Publish
RUN dotnet publish "./WebApplication1.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore


# =========================
# Runtime Stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# ASP.NET Core listens on port 8080
EXPOSE 8080

# Start application
ENTRYPOINT ["dotnet", "WebApplication1.dll"]
