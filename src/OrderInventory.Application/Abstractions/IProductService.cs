using OrderInventory.Application.Dtos.Products;

namespace OrderInventory.Application.Abstractions;

public interface IProductService
{
    Task<IReadOnlyList<ProductSummaryDto>> GetProductsAsync(string? keyword, CancellationToken cancellationToken);
    Task<ProductDetailDto?> GetProductByIdAsync(long productId, CancellationToken cancellationToken);
}
