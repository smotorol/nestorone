namespace OrderInventory.Contracts.Orders;

public sealed class OrderItemDetail
{
    public long OrderItemId { get; init; }
    public long ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int OrderQty { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineAmount { get; init; }
}
