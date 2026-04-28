using System.Net;
using System.Text.Json;
using Oracle.ManagedDataAccess.Client;
using OrderInventory.Contracts.Common;

namespace OrderInventory.Api.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OracleException ex)
        {
            _logger.LogError(ex, "Oracle exception at {Path}. Code={Code}", context.Request.Path, ex.Number);
            await WriteErrorAsync(context, MapStatusCode(ex.Number), MapCode(ex.Number), MapMessage(ex.Number));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception at {Path}", context.Request.Path);
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "ERR_INTERNAL", "서버 처리 중 오류가 발생했습니다.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, string code, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Fail(
            code,
            message,
            context.TraceIdentifier,
            context.Request.Path,
            DateTimeOffset.UtcNow);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static HttpStatusCode MapStatusCode(int oracleCode)
        => oracleCode switch
        {
            20001 => HttpStatusCode.BadRequest,
            20002 => HttpStatusCode.BadRequest,
            20003 => HttpStatusCode.NotFound,
            20004 => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };

    private static string MapCode(int oracleCode)
        => oracleCode switch
        {
            20001 => "ERR_STOCK_SHORTAGE",
            20002 => "ERR_INVALID_PRODUCT",
            20003 => "ERR_ORDER_NOT_FOUND",
            20004 => "ERR_ALREADY_CANCELLED",
            _ => "ERR_ORACLE"
        };

    private static string MapMessage(int oracleCode)
        => oracleCode switch
        {
            20001 => "재고가 부족합니다.",
            20002 => "유효하지 않은 상품입니다.",
            20003 => "주문을 찾을 수 없습니다.",
            20004 => "이미 취소된 주문입니다.",
            _ => "데이터베이스 처리 중 오류가 발생했습니다."
        };
}
