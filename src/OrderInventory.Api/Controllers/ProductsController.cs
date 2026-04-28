using Microsoft.AspNetCore.Mvc;
using OrderInventory.Application.Abstractions;
using OrderInventory.Contracts.Common;
using OrderInventory.Contracts.Products;

namespace OrderInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] string? keyword, CancellationToken cancellationToken)
    {
        var items = await _productService.GetProductsAsync(keyword, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ProductSummary>>.Ok(items, "상품 목록 조회 성공", HttpContext.TraceIdentifier, path: HttpContext.Request.Path));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetProductById(long id, CancellationToken cancellationToken)
    {
        var item = await _productService.GetProductByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound(ApiResponse<object>.Fail("ERR_PRODUCT_NOT_FOUND", "상품을 찾을 수 없습니다.", HttpContext.TraceIdentifier, HttpContext.Request.Path));
        }

        return Ok(ApiResponse<ProductDetail>.Ok(item, "상품 상세 조회 성공", HttpContext.TraceIdentifier, path: HttpContext.Request.Path));
    }
}

