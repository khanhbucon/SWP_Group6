using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Mo_Entities.ModelRequest;
using Mo_Entities.Models;

namespace Mo_Client.Services
{
    public class OrderProductApiClient
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static string _baseUrl = "https://localhost:7234/"; // Hardcode base URL, phù hợp với Mo_Api

        // Constructor mặc định (không tham số)
        public OrderProductApiClient()
        {
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        // Constructor với IOptions<ApiOptions> (giữ lại cho tương lai nếu dùng DI)
        public OrderProductApiClient(IOptions<ApiOptions> options)
        {
            _baseUrl = options.Value.BaseUrl ?? "https://localhost:7234/";
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        // 🟢 Gửi yêu cầu tạo đơn hàng
        public async Task<bool> CreateOrderAsync(OrderProductRequest req, CancellationToken ct = default)
        {
            var resp = await _httpClient.PostAsJsonAsync("/api/OrderProduct/Create", req, ct);
            return resp.IsSuccessStatusCode;
        }

        // 🟢 Lấy danh sách đơn hàng theo AccountId
        public async Task<List<OrderProduct>?> GetOrdersByAccountAsync(long accountId, CancellationToken ct = default)
        {
            var resp = await _httpClient.GetAsync($"/api/OrderProduct/ByAccount/{accountId}", ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<List<OrderProduct>>(cancellationToken: ct);
        }

        // 🟢 Cập nhật trạng thái đơn hàng
        public async Task<bool> UpdateStatusAsync(long orderId, string newStatus, CancellationToken ct = default)
        {
            var resp = await _httpClient.PutAsJsonAsync($"/api/OrderProduct/UpdateStatus/{orderId}", newStatus, ct);
            return resp.IsSuccessStatusCode;
        }

        // Destructor (tùy chọn)
        ~OrderProductApiClient()
        {
            _httpClient?.Dispose();
        }
    }
}