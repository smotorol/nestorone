using OrderInventory.Application.Abstractions;
using OrderInventory.Contracts.Products;

namespace OrderInventory.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<IReadOnlyList<ProductSummary>> GetProductsAsync(string? keyword, CancellationToken cancellationToken)
        => _productRepository.GetProductsAsync(keyword, cancellationToken);

    public Task<ProductDetail?> GetProductByIdAsync(long productId, CancellationToken cancellationToken)
        => _productRepository.GetProductByIdAsync(productId, cancellationToken);
}

