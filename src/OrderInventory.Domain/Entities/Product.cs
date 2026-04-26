namespace OrderInventory.Domain.Entities;

public sealed class Product
{
    public long ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int CurrentStockQty { get; init; }
    public int SafetyStockQty { get; init; }
    public string StatusCode { get; init; } = string.Empty;
    public string CategoryCode { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
}
