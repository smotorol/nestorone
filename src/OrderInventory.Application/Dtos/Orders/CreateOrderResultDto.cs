namespace OrderInventory.Application.Dtos.Orders;

public sealed class CreateOrderResultDto
{
    public long OrderId { get; init; }
    public string OrderNo { get; init; } = string.Empty;
    public string ResultCode { get; init; } = string.Empty;
    public string ResultMessage { get; init; } = string.Empty;
}
