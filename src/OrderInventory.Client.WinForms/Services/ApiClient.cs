using System.Net.Http.Json;
using OrderInventory.Client.WinForms.Models;

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

    public async Task<IReadOnlyList<ProductViewModel>> GetProductsAsync(string? keyword, CancellationToken cancellationToken)
    {
        var url = string.IsNullOrWhiteSpace(keyword) ? "api/products" : $"api/products?keyword={Uri.EscapeDataString(keyword)}";
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<ProductViewModel>>>(url, cancellationToken);
        return response?.Data ?? new List<ProductViewModel>();
    }

    public async Task<ApiResponse<CreateOrderResponse>?> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/orders", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CreateOrderResponse>>(cancellationToken: cancellationToken);
    }
}
