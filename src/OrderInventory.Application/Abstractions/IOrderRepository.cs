using OrderInventory.Application.Dtos.Orders;

namespace OrderInventory.Application.Abstractions;

public interface IOrderRepository
{
    Task<CreateOrderResultDto> CreateOrderAsync(CreateOrderRequestDto request, CancellationToken cancellationToken);
    Task<CancelOrderResultDto> CancelOrderAsync(long orderId, CancelOrderRequestDto request, CancellationToken cancellationToken);
}
