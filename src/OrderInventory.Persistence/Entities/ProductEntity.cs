namespace OrderInventory.Persistence.Entities;

public sealed class ProductEntity
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int CurrentStockQty { get; set; }
    public int SafetyStockQty { get; set; }
    public string StatusCode { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CategoryEntity? Category { get; set; }
}