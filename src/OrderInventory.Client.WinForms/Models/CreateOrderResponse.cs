namespace OrderInventory.Client.WinForms.Models;

public sealed class CreateOrderResponse
{
    public long OrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string ResultCode { get; set; } = string.Empty;
    public string ResultMessage { get; set; } = string.Empty;
}
