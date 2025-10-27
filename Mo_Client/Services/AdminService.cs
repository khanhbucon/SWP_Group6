using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Mo_Client.Models;
using Mo_Client.Models.Admin;
using Mo_Entities.ModelResponse;

namespace Mo_Client.Services
{
    public class AdminService
    {
        private readonly HttpClient _httpClient;

        public AdminService(HttpClient httpClient, IOptions<ApiOptions> options)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
        }

        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // User Management Methods
        public async Task<List<ListAccountVm>?> GetAllUsersAsync(CancellationToken ct = default)
        {
            var resp = await _httpClient.GetAsync("/api/account/Admin/GetAllAccount", ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<List<ListAccountVm>>(cancellationToken: ct);
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

        // Dashboard Stats
        public async Task<DashboardVm?> GetDashboardStatsAsync(CancellationToken ct = default)
        {
            try
            {
                var resp = await _httpClient.GetAsync("/api/Admin/dashboard-stats", ct);
                if (!resp.IsSuccessStatusCode) return null;
                
                var result = await resp.Content.ReadFromJsonAsync<ApiResponse<DashboardVm>>(cancellationToken: ct);
                return result?.Data;
            }
            catch
            {
                return null;
            }
        }

        // Admin Shop management methods wired to API
        public async Task<List<AdminShopListItem>?> GetShopsAsync(string? search = null, CancellationToken ct = default)
        {
            var url = "/api/shop/admin/list" + (string.IsNullOrWhiteSpace(search) ? string.Empty : $"?search={Uri.EscapeDataString(search)}");
            var resp = await _httpClient.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return null;
            var env = await resp.Content.ReadFromJsonAsync<ApiEnvelope<List<AdminShopListItem>>>(cancellationToken: ct);
            return env?.Data;
        }

        public async Task<bool> ApproveShopAsync(long shopId, CancellationToken ct = default)
        {
            var resp = await _httpClient.PostAsync($"/api/shop/admin/{shopId}/approve", null, ct);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> ActivateShopAsync(long shopId, CancellationToken ct = default)
        {
            var resp = await _httpClient.PostAsync($"/api/shop/admin/{shopId}/activate", null, ct);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> SuspendShopAsync(long shopId, CancellationToken ct = default)
        {
            var resp = await _httpClient.PostAsync($"/api/shop/admin/{shopId}/suspend", null, ct);
            return resp.IsSuccessStatusCode;
        }

        // Admin Product management methods wired to API
        public async Task<List<AdminProductListItem>?> GetProductsAsync(string? search = null, CancellationToken ct = default)
        {
            var url = "/api/product/admin/list" + (string.IsNullOrWhiteSpace(search) ? string.Empty : $"?search={Uri.EscapeDataString(search)}");
            var resp = await _httpClient.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return null;
            var env = await resp.Content.ReadFromJsonAsync<ApiEnvelope<List<AdminProductListItem>>>(cancellationToken: ct);
            return env?.Data;
        }

        public async Task<bool> ApproveProductAsync(long productId, CancellationToken ct = default)
        {
            var resp = await _httpClient.PostAsync($"/api/product/admin/{productId}/approve", null, ct);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> ActivateProductAsync(long productId, CancellationToken ct = default)
        {
            var resp = await _httpClient.PostAsync($"/api/product/admin/{productId}/activate", null, ct);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> SuspendProductAsync(long productId, CancellationToken ct = default)
        {
            var resp = await _httpClient.PostAsync($"/api/product/admin/{productId}/suspend", null, ct);
            return resp.IsSuccessStatusCode;
        }

        private record ApiEnvelope<T>(bool Success, T? Data, string? Message);
        public record ApiResponse<T>(bool Success, T? Data, string? Message);
    }
}
