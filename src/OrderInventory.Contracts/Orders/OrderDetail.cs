namespace OrderInventory.Contracts.Orders;

public sealed class OrderDetail
{
    public long OrderId { get; init; }
    public string OrderNo { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string OrderStatus { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTimeOffset OrderDate { get; init; }
    public DateTimeOffset? CancelDate { get; init; }
    public string? CancelReason { get; init; }
    public List<OrderItemDetail> Items { get; init; } = new();
}
