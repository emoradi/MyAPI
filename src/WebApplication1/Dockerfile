FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

WORKDIR /app

EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ["WebApplication1.csproj", "./"]

RUN dotnet restore "./WebApplication1.csproj"

COPY . .

RUN dotnet build "./WebApplication1.csproj" \
    -c Release \
    -o /app/build \
    --no-restore


FROM build AS publish

RUN dotnet publish "./WebApplication1.csproj" \
    -c Release \
    -o /app/publish \
    --no-build


FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "WebApplication1.dll"]
