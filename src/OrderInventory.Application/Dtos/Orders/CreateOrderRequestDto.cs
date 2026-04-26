using System.ComponentModel.DataAnnotations;

namespace OrderInventory.Application.Dtos.Orders;

public sealed class CreateOrderRequestDto
{
    [Required]
    public string CustomerName { get; init; } = string.Empty;

    [Required]
    public string CreatedBy { get; init; } = string.Empty;

    [MinLength(1)]
    public List<CreateOrderItemDto> Items { get; init; } = new();
}
