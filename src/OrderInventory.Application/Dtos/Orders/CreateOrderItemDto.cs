using System.ComponentModel.DataAnnotations;

namespace OrderInventory.Application.Dtos.Orders;

public sealed class CreateOrderItemDto
{
    [Range(1, long.MaxValue)]
    public long ProductId { get; init; }

    [Range(1, int.MaxValue)]
    public int OrderQty { get; init; }
}
