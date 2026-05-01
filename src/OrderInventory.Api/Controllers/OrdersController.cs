using Microsoft.AspNetCore.Mvc;
using OrderInventory.Application.Abstractions;
using OrderInventory.Contracts.Common;
using OrderInventory.Contracts.Orders;

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

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] string? keyword, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var items = await _orderService.GetOrdersAsync(keyword, status, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<OrderSummary>>.Ok(items, "주문 목록 조회 성공", HttpContext.TraceIdentifier, path: HttpContext.Request.Path));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetOrderById(long id, CancellationToken cancellationToken)
    {
        var item = await _orderService.GetOrderByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound(ApiResponse<object>.Fail("ERR_ORDER_NOT_FOUND", "주문을 찾을 수 없습니다.", HttpContext.TraceIdentifier, HttpContext.Request.Path));
        }

        return Ok(ApiResponse<OrderDetail>.Ok(item, "주문 상세 조회 성공", HttpContext.TraceIdentifier, path: HttpContext.Request.Path));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateOrderAsync(request, cancellationToken);
        return Ok(ApiResponse<CreateOrderResult>.Ok(result, result.ResultMessage, HttpContext.TraceIdentifier, result.ResultCode, HttpContext.Request.Path));
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id, [FromBody] CancelOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.CancelOrderAsync(id, request, cancellationToken);
        return Ok(ApiResponse<CancelOrderResult>.Ok(result, result.ResultMessage, HttpContext.TraceIdentifier, result.ResultCode, HttpContext.Request.Path));
    }
}
