using OrderInventory.Contracts.Orders;

namespace OrderInventory.Application.Abstractions;

public interface IOrderService
{
    Task<IReadOnlyList<OrderSummary>> GetOrdersAsync(string? keyword, string? status, CancellationToken cancellationToken);
    Task<OrderDetail?> GetOrderByIdAsync(long orderId, CancellationToken cancellationToken);
    Task<CreateOrderResult> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<CancelOrderResult> CancelOrderAsync(long orderId, CancelOrderRequest request, CancellationToken cancellationToken);
}
