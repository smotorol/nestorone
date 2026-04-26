using Microsoft.EntityFrameworkCore;
using OrderInventory.Application.Abstractions;
using OrderInventory.Application.Dtos.Products;
using OrderInventory.Persistence.Contexts;

namespace OrderInventory.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly OrderInventoryDbContext _dbContext;

    public ProductRepository(OrderInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductSummaryDto>> GetProductsAsync(string? keyword, CancellationToken cancellationToken)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .Where(x => x.StatusCode == "ACTIVE")
            .Select(x => new ProductSummaryDto
            {
                ProductId = (long)x.ProductId,
                ProductCode = x.ProductCode,
                ProductName = x.ProductName,
                UnitPrice = x.UnitPrice,
                CurrentStockQty = x.CurrentStockQty,
                StatusCode = x.StatusCode,
                CategoryName = x.Category != null ? x.Category.CategoryName : string.Empty
            });

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.ProductName.Contains(keyword));
        }

        return await query
            .OrderBy(x => x.ProductId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(long productId, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .Select(x => new ProductDetailDto
            {
                ProductId = (long)x.ProductId,
                ProductCode = x.ProductCode,
                ProductName = x.ProductName,
                UnitPrice = x.UnitPrice,
                CurrentStockQty = x.CurrentStockQty,
                SafetyStockQty = x.SafetyStockQty,
                StatusCode = x.StatusCode,
                CategoryCode = x.Category != null ? x.Category.CategoryCode : string.Empty,
                CategoryName = x.Category != null ? x.Category.CategoryName : string.Empty
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}
