using Microsoft.EntityFrameworkCore;
using OrderInventory.Persistence.Contexts;

namespace OrderInventory.Migrations;

public static class MigrationRunner
{
    public static async Task ApplyMigrationsAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderInventoryDbContext>();
        optionsBuilder.UseOracle(connectionString, options => options.MigrationsAssembly(typeof(MigrationRunner).Assembly.FullName));

        await using var dbContext = new OrderInventoryDbContext(optionsBuilder.Options);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
