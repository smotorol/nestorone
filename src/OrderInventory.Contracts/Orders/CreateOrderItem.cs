using System.ComponentModel.DataAnnotations;

namespace OrderInventory.Contracts.Orders;

public sealed class CreateOrderItem
{
    [Range(1, long.MaxValue)]
    public long ProductId { get; init; }

    [Range(1, int.MaxValue)]
    public int OrderQty { get; init; }
}

