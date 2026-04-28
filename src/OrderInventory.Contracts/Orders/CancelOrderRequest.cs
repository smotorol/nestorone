using System.ComponentModel.DataAnnotations;

namespace OrderInventory.Contracts.Orders;

public sealed class CancelOrderRequest
{
    [Required]
    public string CancelReason { get; init; } = string.Empty;

    [Required]
    public string UpdatedBy { get; init; } = string.Empty;
}

