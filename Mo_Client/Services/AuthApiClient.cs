using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mo_Client.Models;

namespace Mo_Client.Services;

public class AuthApiClient
{
    private readonly HttpClient _httpClient;
    public AuthApiClient(HttpClient httpClient, IOptions<ApiOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public Uri GetBaseAddress() => _httpClient.BaseAddress!;
    public System.Net.Http.Headers.AuthenticationHeaderValue? GetAuthHeader() => _httpClient.DefaultRequestHeaders.Authorization;

    public record LoginRequest(string Identifier, string Password, bool RememberMe);
    public record LoginResponse(string AccessToken, DateTime ExpiresAt, string? RefreshToken, List<string>? Roles);

    public record RegisterRequest(string Username, string Email, string Phone, string Password);
    public record RegisterResponse(bool Success, string? Message);

    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Token, string NewPassword);
    public async Task<LoginResponse?> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var resp = await _httpClient.PostAsJsonAsync("/api/auth/login", req, ct);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
    }

    public async Task<RegisterResponse?> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        var resp = await _httpClient.PostAsJsonAsync("/api/auth/register", req, ct);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<RegisterResponse>(cancellationToken: ct);
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest req, CancellationToken ct = default)
    {
        var resp = await _httpClient.PostAsJsonAsync("/api/account/forgot-password", req, ct);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest req, CancellationToken ct = default)
    {
        var resp = await _httpClient.PostAsJsonAsync("/api/account/reset-password", req, ct);
        return resp.IsSuccessStatusCode;
    }

    public async Task<List<ListAccountResponse>?> GetAllUsersAsync(CancellationToken ct = default)
    {
        var resp = await _httpClient.GetAsync("/api/account/Admin/GetAllAccount", ct);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<List<ListAccountResponse>>(cancellationToken: ct);
    }

    public async Task<bool> BanUserAsync(long userId, CancellationToken ct = default)
    {
        var resp = await _httpClient.PostAsync($"/api/account/admin/{userId}/banUser", null, ct);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> GrantSellerRoleAsync(long userId, CancellationToken ct = default)
    {
        var resp = await _httpClient.PostAsync($"/api/account/admin/{userId}/grant-seller", null, ct);
        return resp.IsSuccessStatusCode;
    }
    public async Task<ProfileResponse?> GetCurrentUserProfileAsync(CancellationToken ct = default)
    {
        var resp = await _httpClient.GetAsync("/api/account/profile", ct);
        if (!resp.IsSuccessStatusCode) return null;

        var result = await resp.Content.ReadFromJsonAsync<ApiResponse<ProfileResponse>>(cancellationToken: ct);
        return result?.Data;
    }

    public record ApiResponse<T>(bool Success, T? Data, string? Message);

    public record CreateShopRequest(string Name, string? Description);
    public record UpdateShopRequest(string Name, string? Description);

    public record ShopResponse(long Id, long AccountId, string Name, string? Description, int? ReportCount, bool? IsActive, DateTime? CreatedAt, DateTime? UpdatedAt, int TotalProducts, List<string>? CategoryNames);
    public record ShopStatisticsResponse(long ShopId, string ShopName, int TotalProducts, int TotalProductsSold, decimal TotalRevenue, int TotalOrders, decimal AverageRating, int TotalFeedbacks);

    // Product DTOs
    public record CreateProductRequest(long ShopId, string Name, string? ShortDescription, string? DetailedDescription, long SubCategoryId, decimal Price, int Stock, string? ImageUrl, decimal? Fee = null);

    public async Task<(bool Success, string? Message)> CreateShopAsync(CreateShopRequest req, CancellationToken ct = default)
    {
        var resp = await _httpClient.PostAsJsonAsync("/api/shop/create", req, ct);
        if (resp.IsSuccessStatusCode) return (true, null);

        // Map common statuses
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return (false, "Bạn cần đăng nhập để thực hiện thao tác này");
        if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            return (false, "Bạn không có quyền Seller để tạo shop");

        try
        {
            // Try our standard API envelope
            var envelope = await resp.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: ct);
            if (envelope != null && !string.IsNullOrWhiteSpace(envelope.Message))
                return (false, envelope.Message);
        }
        catch { /* ignore parse errors */ }

        try
        {
            // Try ProblemDetails
            var problem = await resp.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: ct);
            if (problem != null)
            {
                // Aggregate validation error messages if exist
                if (problem.Extensions != null && problem.Extensions.TryGetValue("errors", out var errsObj))
                {
                    if (errsObj is IDictionary<string, object> fieldsDict)
                    {
                        var messages = new List<string>();
                        foreach (var kv in fieldsDict)
                        {
                            if (kv.Value is IEnumerable<object> arr)
                            {
                                foreach (var item in arr)
                                {
                                    if (item is string s && !string.IsNullOrWhiteSpace(s)) messages.Add(s);
                                }
                            }
                        }
                        if (messages.Count > 0) return (false, string.Join("\n", messages));
                    }
                }
                var msg = problem.Detail ?? problem.Title;
                if (!string.IsNullOrWhiteSpace(msg)) return (false, msg);
            }
        }
        catch { /* ignore parse errors */ }

        return (false, resp.ReasonPhrase);
    }

    public async Task<ShopResponse?> GetMyShopAsync(CancellationToken ct = default)
    {
        var resp = await _httpClient.GetAsync("/api/shop/my-shop", ct);
        if (!resp.IsSuccessStatusCode) return null;
        var result = await resp.Content.ReadFromJsonAsync<ApiResponse<ShopResponse>>(cancellationToken: ct);
        return result?.Data;
    }

    // New: get all shops of current account
    public async Task<List<ShopResponse>?> GetMyShopsAsync(CancellationToken ct = default)
    {
        var resp = await _httpClient.GetAsync("/api/shop/my-shops", ct);
        if (!resp.IsSuccessStatusCode) return null;
        var result = await resp.Content.ReadFromJsonAsync<ApiResponse<List<ShopResponse>>>(cancellationToken: ct);
        return result?.Data;
    }

    public async Task<bool> UpdateShopAsync(long shopId, UpdateShopRequest req, CancellationToken ct = default)
    {
        var resp = await _httpClient.PutAsJsonAsync($"/api/shop/update/{shopId}", req, ct);
        return resp.IsSuccessStatusCode;
    }

    public async Task<ShopStatisticsResponse?> GetShopStatisticsAsync(CancellationToken ct = default)
    {
        var resp = await _httpClient.GetAsync("/api/shop/statistics", ct);
        if (!resp.IsSuccessStatusCode) return null;
        var result = await resp.Content.ReadFromJsonAsync<ApiResponse<ShopStatisticsResponse>>(cancellationToken: ct);
        return result?.Data;
    }

    // New: statistics by shop id
    public async Task<ShopStatisticsResponse?> GetShopStatisticsByIdAsync(long shopId, CancellationToken ct = default)
    {
        var resp = await _httpClient.GetAsync($"/api/shop/{shopId}/statistics", ct);
        if (!resp.IsSuccessStatusCode) return null;
        var result = await resp.Content.ReadFromJsonAsync<ApiResponse<ShopStatisticsResponse>>(cancellationToken: ct);
        return result?.Data;
    }

    // Generic helpers for feature pages
    public async Task<(bool Success, string? Message)> PostJsonAsync<T>(string url, object body, CancellationToken ct = default)
    {
        var resp = await _httpClient.PostAsJsonAsync(url, body, ct);
        if (resp.IsSuccessStatusCode) return (true, null);
        try
        {
            var envelope = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: ct);
            if (envelope != null && !string.IsNullOrWhiteSpace(envelope.Message)) return (false, envelope.Message);
        }
        catch { }
        return (false, resp.ReasonPhrase);
    }

    // Product Orders
    public record ProductOrderItem(long OrderId, long ProductId, string ProductName, string VariantName, int Quantity, decimal TotalAmount, string Status, string BuyerName);

    public async Task<List<ProductOrderItem>?> GetMyProductOrdersAsync(CancellationToken ct = default)
    {
        var resp = await _httpClient.GetAsync("/api/orders/product", ct);
        if (!resp.IsSuccessStatusCode) return null;
        var envelope = await resp.Content.ReadFromJsonAsync<ApiResponse<List<ProductOrderItem>>>(cancellationToken: ct);
        return envelope?.Data;
    }

    // Product management
    public record ProductSummary(long Id, string Name, string? Description, string? Details, long ShopId, string ShopName, DateTime? CreatedAt, DateTime? UpdatedAt, bool? IsActive);
    public record ProductDetail(long Id, string Name, string? Description, string? Details, decimal? Fee, long SubCategoryId, long ShopId, DateTime? CreatedAt, DateTime? UpdatedAt, bool? IsActive);

    public record UpdateProductRequest(long Id, string? Name, string? ShortDescription, string? DetailedDescription, decimal? Fee, bool? IsActive);

    public async Task<List<ProductSummary>?> GetMyProductsAsync(CancellationToken ct = default)
    {
        var resp = await _httpClient.GetAsync("/api/product/my", ct);
        if (!resp.IsSuccessStatusCode) return null;
        var env = await resp.Content.ReadFromJsonAsync<ApiResponse<List<ProductSummary>>>(cancellationToken: ct);
        return env?.Data;
    }

    public async Task<ProductDetail?> GetProductAsync(long id, CancellationToken ct = default)
    {
        var resp = await _httpClient.GetAsync($"/api/product/{id}", ct);
        if (!resp.IsSuccessStatusCode) return null;
        var env = await resp.Content.ReadFromJsonAsync<ApiResponse<ProductDetail>>(cancellationToken: ct);
        return env?.Data;
    }

    public async Task<bool> UpdateProductAsync(UpdateProductRequest req, CancellationToken ct = default)
    {
        var resp = await _httpClient.PutAsJsonAsync("/api/product", req, ct);
        return resp.IsSuccessStatusCode;
    }
}


