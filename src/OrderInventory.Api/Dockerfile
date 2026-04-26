FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["src/OrderInventory.Api/OrderInventory.Api.csproj", "src/OrderInventory.Api/"]
COPY ["src/OrderInventory.Application/OrderInventory.Application.csproj", "src/OrderInventory.Application/"]
COPY ["src/OrderInventory.Domain/OrderInventory.Domain.csproj", "src/OrderInventory.Domain/"]
COPY ["src/OrderInventory.Infrastructure/OrderInventory.Infrastructure.csproj", "src/OrderInventory.Infrastructure/"]
COPY ["src/OrderInventory.Persistence/OrderInventory.Persistence.csproj", "src/OrderInventory.Persistence/"]
COPY ["src/OrderInventory.Migrations/OrderInventory.Migrations.csproj", "src/OrderInventory.Migrations/"]
RUN dotnet restore "src/OrderInventory.Migrations/OrderInventory.Migrations.csproj"
RUN dotnet restore "src/OrderInventory.Api/OrderInventory.Api.csproj"

COPY . .
RUN dotnet build "src/OrderInventory.Migrations/OrderInventory.Migrations.csproj" -c Release
RUN dotnet publish "src/OrderInventory.Api/OrderInventory.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "OrderInventory.Api.dll"]