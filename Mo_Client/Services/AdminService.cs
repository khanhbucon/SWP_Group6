using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Mo_Client.Models;
using Mo_Client.Models.Admin;
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

        // TODO: Thêm các method khác cho admin khi có API
        // public async Task<List<ShopResponse>?> GetAllShopsAsync(CancellationToken ct = default)
        // {
        //     var resp = await _httpClient.GetAsync("/api/admin/shops", ct);
        //     if (!resp.IsSuccessStatusCode) return null;
        //     return await resp.Content.ReadFromJsonAsync<List<ShopResponse>>(cancellationToken: ct);
        // }
        
        // public async Task<bool> ApproveShopAsync(long shopId, CancellationToken ct = default)
        // {
        //     var resp = await _httpClient.PostAsync($"/api/admin/shops/{shopId}/approve", null, ct);
        //     return resp.IsSuccessStatusCode;
        // }
        
        // public async Task<bool> SuspendShopAsync(long shopId, CancellationToken ct = default)
        // {
        //     var resp = await _httpClient.PostAsync($"/api/admin/shops/{shopId}/suspend", null, ct);
        //     return resp.IsSuccessStatusCode;
        // }
    }
}
