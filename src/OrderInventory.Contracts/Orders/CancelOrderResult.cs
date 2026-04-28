namespace OrderInventory.Contracts.Orders;

public sealed class CancelOrderResult
{
    public long OrderId { get; init; }
    public string ResultCode { get; init; } = string.Empty;
    public string ResultMessage { get; init; } = string.Empty;
}

