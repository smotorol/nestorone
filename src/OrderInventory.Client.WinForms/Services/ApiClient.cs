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
}

