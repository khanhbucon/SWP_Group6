using System.Text.Json;
using System.Text;
using Mo_Client.Models;
using Microsoft.Extensions.Options;

namespace Mo_Client.Services
{
    public class PaymentService
    {
        private readonly HttpClient _httpClient;
        private string? _token;

        public PaymentService(HttpClient httpClient, IOptions<ApiOptions> options)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
        }

        public void SetToken(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<ClientDepositResponse> CreateVnPayDepositAsync(ClientDepositRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/payment/deposit/vnpay", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ClientDepositResponse>(responseContent) ?? new ClientDepositResponse();
        }

        public async Task<ClientDepositResponse> VerifyVnPayDepositAsync(ClientVerifyRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/payment/deposit/verify", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ClientDepositResponse>(responseContent) ?? new ClientDepositResponse();
        }

        public async Task<List<ClientPaymentHistoryVm>> GetDepositHistoryAsync()
        {
            var response = await _httpClient.GetAsync("/api/payment/deposit/history");
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ClientApiResponse<List<ClientPaymentHistoryVm>>>(responseContent);
            return result?.Data ?? new List<ClientPaymentHistoryVm>();
        }

        public async Task<ClientPaymentBalanceVm> GetCurrentBalanceAsync()
        {
            var response = await _httpClient.GetAsync("/api/payment/balance");
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ClientApiResponse<ClientPaymentBalanceVm>>(responseContent);
            return result?.Data ?? new ClientPaymentBalanceVm();
        }

        public async Task<List<ClientPaymentTransactionVm>> GetPaymentHistoryAsync()
        {
            var response = await _httpClient.GetAsync("/api/payment/transactions");
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ClientApiResponse<List<ClientPaymentTransactionVm>>>(responseContent);
            return result?.Data ?? new List<ClientPaymentTransactionVm>();
        }
    }
}
