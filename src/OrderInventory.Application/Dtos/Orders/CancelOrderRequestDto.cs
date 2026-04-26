using System.ComponentModel.DataAnnotations;

namespace OrderInventory.Application.Dtos.Orders;

public sealed class CancelOrderRequestDto
{
    [Required]
    public string CancelReason { get; init; } = string.Empty;

    [Required]
    public string UpdatedBy { get; init; } = string.Empty;
}
