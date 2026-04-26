using Microsoft.EntityFrameworkCore;
using OrderInventory.Persistence.Entities;

namespace OrderInventory.Persistence.Contexts;

public sealed class OrderInventoryDbContext : DbContext
{
    public OrderInventoryDbContext(DbContextOptions<OrderInventoryDbContext> options) : base(options)
    {
    }

    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    public DbSet<ProductEntity> Products => Set<ProductEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderInventoryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
