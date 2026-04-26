using OrderInventory.Application.Abstractions;
using OrderInventory.Application.Dtos.Products;

namespace OrderInventory.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<IReadOnlyList<ProductSummaryDto>> GetProductsAsync(string? keyword, CancellationToken cancellationToken)
        => _productRepository.GetProductsAsync(keyword, cancellationToken);

    public Task<ProductDetailDto?> GetProductByIdAsync(long productId, CancellationToken cancellationToken)
        => _productRepository.GetProductByIdAsync(productId, cancellationToken);
}
