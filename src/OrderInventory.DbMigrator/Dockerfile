FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/OrderInventory.DbMigrator/OrderInventory.DbMigrator.csproj", "src/OrderInventory.DbMigrator/"]
RUN dotnet restore "src/OrderInventory.DbMigrator/OrderInventory.DbMigrator.csproj"

COPY . .
RUN dotnet publish "src/OrderInventory.DbMigrator/OrderInventory.DbMigrator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY db/migrations_sql /app/db/migrations_sql
ENTRYPOINT ["dotnet", "OrderInventory.DbMigrator.dll"]
