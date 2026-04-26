using OrderInventory.Application.Abstractions;
using OrderInventory.Application.Dtos.Orders;

namespace OrderInventory.Application.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public Task<CreateOrderResultDto> CreateOrderAsync(CreateOrderRequestDto request, CancellationToken cancellationToken)
        => _orderRepository.CreateOrderAsync(request, cancellationToken);

    public Task<CancelOrderResultDto> CancelOrderAsync(long orderId, CancelOrderRequestDto request, CancellationToken cancellationToken)
        => _orderRepository.CancelOrderAsync(orderId, request, cancellationToken);
}
