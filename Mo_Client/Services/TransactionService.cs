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

                var apiUrl = $"{_configuration["Api:BaseUrl"]}/api/account/transaction-history";
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting transaction history: {ex.Message}");
            }

            return null;
        }

    }

}
