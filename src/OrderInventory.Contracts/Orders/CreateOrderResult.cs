namespace OrderInventory.Contracts.Orders;

public sealed class CreateOrderResult
{
    public long OrderId { get; init; }
    public string OrderNo { get; init; } = string.Empty;
    public string ResultCode { get; init; } = string.Empty;
    public string ResultMessage { get; init; } = string.Empty;
}

