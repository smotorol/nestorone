using Dapper;
using OrderInventory.Application.Abstractions;
using OrderInventory.Contracts.Products;
using OrderInventory.Infrastructure.Persistence;

namespace OrderInventory.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly IOracleConnectionFactory _connectionFactory;

    public ProductRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ProductSummary>> GetProductsAsync(string? keyword, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT p.product_id        AS ProductId,
       p.product_code      AS ProductCode,
       p.product_name      AS ProductName,
       p.unit_price        AS UnitPrice,
       p.current_stock_qty AS CurrentStockQty,
       p.status_code       AS StatusCode,
       c.category_name     AS CategoryName
  FROM products p
  JOIN categories c
    ON c.category_id = p.category_id
 WHERE p.status_code = 'ACTIVE'
   AND (:keyword IS NULL OR p.product_name LIKE '%' || :keyword || '%')
 ORDER BY p.product_id";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { keyword }, cancellationToken: cancellationToken);
        var items = await connection.QueryAsync<ProductSummary>(command);
        return items.AsList();
    }

    public async Task<ProductDetail?> GetProductByIdAsync(long productId, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT p.product_id         AS ProductId,
       p.product_code       AS ProductCode,
       p.product_name       AS ProductName,
       p.unit_price         AS UnitPrice,
       p.current_stock_qty  AS CurrentStockQty,
       p.safety_stock_qty   AS SafetyStockQty,
       p.status_code        AS StatusCode,
       c.category_code      AS CategoryCode,
       c.category_name      AS CategoryName
  FROM products p
  JOIN categories c
    ON c.category_id = p.category_id
 WHERE p.product_id = :productId";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { productId }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ProductDetail>(command);
    }
}

