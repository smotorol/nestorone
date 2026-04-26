using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OrderInventory.Persistence.Contexts;

namespace OrderInventory.Persistence.Factories;

public sealed class OrderInventoryDbContextFactory : IDesignTimeDbContextFactory<OrderInventoryDbContext>
{
    public OrderInventoryDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING")
            ?? "User Id=app_user;Password=AppUser1234!;Data Source=localhost:1521/FREEPDB1";

        var builder = new DbContextOptionsBuilder<OrderInventoryDbContext>();
        builder.UseOracle(connectionString, options =>
        {
            options.MigrationsAssembly("OrderInventory.Migrations");
        });

        return new OrderInventoryDbContext(builder.Options);
    }
}
