namespace OrderInventory.Client.WinForms.Models;

public sealed class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "winforms-user";
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public sealed class CreateOrderItemRequest
{
    public long ProductId { get; set; }
    public int OrderQty { get; set; }
}
