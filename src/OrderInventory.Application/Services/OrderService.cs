using OrderInventory.Application.Abstractions;
using OrderInventory.Contracts.Orders;

namespace OrderInventory.Application.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public Task<CreateOrderResult> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
        => _orderRepository.CreateOrderAsync(request, cancellationToken);

    public Task<CancelOrderResult> CancelOrderAsync(long orderId, CancelOrderRequest request, CancellationToken cancellationToken)
        => _orderRepository.CancelOrderAsync(orderId, request, cancellationToken);
}

