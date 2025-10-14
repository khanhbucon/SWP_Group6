using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Mo_Client.Models;

namespace Mo_Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient, IOptions<ApiOptions> options)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
        }

        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // Request/Response Models
        public record LoginRequest(string Identifier, string Password, bool RememberMe);
        public record LoginResponse(string AccessToken, DateTime ExpiresAt, string? RefreshToken, List<string>? Roles);
        public record RegisterRequest(string Username, string Email, string Phone, string Password);
        public record RegisterResponse(bool Success, string? Message);
        public record ForgotPasswordRequest(string Email);
        public record ResetPasswordRequest(string Token, string NewPassword);

        // Authentication Methods
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
    }
}
