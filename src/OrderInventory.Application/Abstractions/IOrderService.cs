using OrderInventory.Contracts.Orders;

namespace OrderInventory.Application.Abstractions;

public interface IOrderService
{
    Task<CreateOrderResult> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<CancelOrderResult> CancelOrderAsync(long orderId, CancelOrderRequest request, CancellationToken cancellationToken);
}

