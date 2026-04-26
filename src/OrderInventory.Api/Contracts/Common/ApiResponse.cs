namespace OrderInventory.Api.Contracts.Common;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public string? TraceId { get; init; }
    public string? Path { get; init; }
    public DateTimeOffset TimestampUtc { get; init; }

    public static ApiResponse<T> Ok(
        T data,
        string message,
        string? traceId = null,
        string code = "SUCCESS",
        string? path = null,
        DateTimeOffset? timestampUtc = null)
        => new()
        {
            Success = true,
            Code = code,
            Message = message,
            Data = data,
            TraceId = traceId,
            Path = path,
            TimestampUtc = timestampUtc ?? DateTimeOffset.UtcNow
        };

    public static ApiResponse<T> Fail(
        string code,
        string message,
        string? traceId = null,
        string? path = null,
        DateTimeOffset? timestampUtc = null)
        => new()
        {
            Success = false,
            Code = code,
            Message = message,
            TraceId = traceId,
            Path = path,
            TimestampUtc = timestampUtc ?? DateTimeOffset.UtcNow
        };
}
