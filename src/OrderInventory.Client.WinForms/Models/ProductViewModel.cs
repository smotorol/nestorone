namespace OrderInventory.Client.WinForms.Models;

public sealed class ProductViewModel
{
    public long ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int CurrentStockQty { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}
