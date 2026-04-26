namespace OrderInventory.Application.Dtos.Orders;

public sealed class CancelOrderResultDto
{
    public long OrderId { get; init; }
    public string ResultCode { get; init; } = string.Empty;
    public string ResultMessage { get; init; } = string.Empty;
}
