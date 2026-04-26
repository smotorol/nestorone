namespace OrderInventory.Client.WinForms.Models;

public sealed class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string? TraceId { get; set; }
}
