using OrderInventory.Contracts.Products;

namespace OrderInventory.Application.Abstractions;

public interface IProductRepository
{
    Task<IReadOnlyList<ProductSummary>> GetProductsAsync(string? keyword, CancellationToken cancellationToken);
    Task<ProductDetail?> GetProductByIdAsync(long productId, CancellationToken cancellationToken);
}

