using System.Net.Http.Json;
using OrderInventory.Contracts.Common;
using OrderInventory.Contracts.Orders;
using OrderInventory.Contracts.Products;

namespace OrderInventory.Client.WinForms.Services;

public sealed class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient()
    {
        var baseUrl = Environment.GetEnvironmentVariable("ORDERINVENTORY_API_BASE_URL")
            ?? "http://localhost:8080/";

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public async Task<IReadOnlyList<ProductSummary>> GetProductsAsync(string? keyword, CancellationToken cancellationToken)
    {
        var url = string.IsNullOrWhiteSpace(keyword) ? "api/products" : $"api/products?keyword={Uri.EscapeDataString(keyword)}";
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<ProductSummary>>>(url, cancellationToken);
        return response?.Data ?? new List<ProductSummary>();
    }

    public async Task<ApiResponse<CreateOrderResult>?> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/orders", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CreateOrderResult>>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<OrderSummary>> GetOrdersAsync(string? keyword, string? status, CancellationToken cancellationToken)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(keyword)) query.Add($"keyword={Uri.EscapeDataString(keyword)}");
        if (!string.IsNullOrWhiteSpace(status)) query.Add($"status={Uri.EscapeDataString(status)}");
        var url = query.Count == 0 ? "api/orders" : $"api/orders?{string.Join("&", query)}";
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<OrderSummary>>>(url, cancellationToken);
        return response?.Data ?? new List<OrderSummary>();
    }

    public async Task<OrderDetail?> GetOrderByIdAsync(long orderId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<OrderDetail>>($"api/orders/{orderId}", cancellationToken);
        return response?.Data;
    }

    public async Task<ApiResponse<CancelOrderResult>?> CancelOrderAsync(long orderId, CancelOrderRequest request, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync($"api/orders/{orderId}/cancel", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CancelOrderResult>>(cancellationToken: cancellationToken);
    }
}
