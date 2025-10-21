using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Mo_Client.Models;
using AdminShopListItemDto = Mo_Entities.ModelResponse.AdminShopListItem;

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
        public async Task<List<Mo_Client.Models.ListAccountResponse>?> GetAllUsersAsync(CancellationToken ct = default)
        {
            var resp = await _httpClient.GetAsync("/api/account/Admin/GetAllAccount", ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<List<Mo_Client.Models.ListAccountResponse>>(cancellationToken: ct);
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

        // Admin shop management
        public async Task<List<AdminShopListItemDto>?> GetShopsAsync(string? search = null, CancellationToken ct = default)
        {
            var url = "/api/shop/admin/list" + (string.IsNullOrWhiteSpace(search) ? string.Empty : $"?search={Uri.EscapeDataString(search)}");
            var resp = await _httpClient.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return null;
            var env = await resp.Content.ReadFromJsonAsync<ApiEnvelope<List<AdminShopListItemDto>>>(cancellationToken: ct);
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

        private record ApiEnvelope<T>(bool Success, T? Data, string? Message);
    }
}
