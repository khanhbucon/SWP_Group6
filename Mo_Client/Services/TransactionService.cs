using Mo_Client.Models;
using Mo_Entities.ModelResponse;
using System.Text.Json;

namespace Mo_Client.Services
{
    public class TransactionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransactionService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TransactionHistoryListResponse?> GetTransactionHistoryAsync()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["accessToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var apiUrl = $"{_configuration["ApiOptions:BaseUrl"]}/api/Transaction/my-history";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<TransactionHistoryListResponse>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return result?.Data;
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool Success, string Message, WithdrawResponse? Data)> CreateWithdrawAsync(decimal amount, string? description = null)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["accessToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return (false, "Bạn cần đăng nhập để thực hiện thao tác này", null);
                }

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var apiUrl = $"{_configuration["ApiOptions:BaseUrl"]}/api/Transaction/withdraw";
                var request = new { Amount = amount, Description = description };
                var jsonContent = System.Text.Json.JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<WithdrawResponse>>(responseContent, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (response.IsSuccessStatusCode && result?.Success == true)
                {
                    return (true, result.Message ?? "Yêu cầu rút tiền đã được gửi thành công", result.Data);
                }
                else
                {
                    return (false, result?.Message ?? "Có lỗi xảy ra khi tạo yêu cầu rút tiền", null);
                }
            }
            catch (Exception ex)
            {
                return (false, "Có lỗi xảy ra: " + ex.Message, null);
            }
        }

        public class WithdrawResponse
        {
            public long TransactionId { get; set; }
            public decimal Amount { get; set; }
            public string Status { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime? CreatedAt { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }
    }

}
