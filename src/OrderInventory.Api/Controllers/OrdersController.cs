using Microsoft.AspNetCore.Mvc;
using OrderInventory.Api.Contracts.Common;
using OrderInventory.Application.Abstractions;
using OrderInventory.Application.Dtos.Orders;

namespace OrderInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateOrderAsync(request, cancellationToken);
        return Ok(ApiResponse<CreateOrderResultDto>.Ok(result, result.ResultMessage, HttpContext.TraceIdentifier, result.ResultCode, HttpContext.Request.Path));
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id, [FromBody] CancelOrderRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _orderService.CancelOrderAsync(id, request, cancellationToken);
        return Ok(ApiResponse<CancelOrderResultDto>.Ok(result, result.ResultMessage, HttpContext.TraceIdentifier, result.ResultCode, HttpContext.Request.Path));
    }
}
