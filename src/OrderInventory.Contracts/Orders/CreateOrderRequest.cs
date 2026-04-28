using System.ComponentModel.DataAnnotations;

namespace OrderInventory.Contracts.Orders;

public sealed class CreateOrderRequest
{
    [Required]
    public string CustomerName { get; init; } = string.Empty;

    [Required]
    public string CreatedBy { get; init; } = string.Empty;

    [MinLength(1)]
    public List<CreateOrderItem> Items { get; init; } = new();
}

