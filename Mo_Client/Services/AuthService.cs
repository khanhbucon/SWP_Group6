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
        public record ForgotPasswordRequest(string Email);
        public record ResetPasswordRequest(string Token, string NewPassword, string ConfirmPassword);

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


        public async Task<(bool Success, string? Message)> ForgotPasswordAsync(ForgotPasswordRequest req, CancellationToken ct = default)
        {
            try
            {
                var resp = await _httpClient.PostAsJsonAsync("/api/account/forgot-password", req, ct);
                
                if (resp.IsSuccessStatusCode)
                {
                    var result = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>(cancellationToken: ct);
                    var success = result?.GetValueOrDefault("success")?.ToString() == "True";
                    var message = result?.GetValueOrDefault("message")?.ToString();
                    return (success == true, message);
                }
                
                var errorContent = await resp.Content.ReadAsStringAsync(ct);
                return (false, "Có lỗi xảy ra khi gửi email đặt lại mật khẩu");
            }
            catch (Exception ex)
            {
                return (false, "Không thể kết nối đến server");
            }
        }

    public async Task<(bool Success, string? Message)> ResetPasswordAsync(ResetPasswordRequest req, CancellationToken ct = default)
    {
        try
        {
            var resp = await _httpClient.PostAsJsonAsync("/api/account/reset-password", req, ct);

            if (resp.IsSuccessStatusCode)
            {
                var result = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>(cancellationToken: ct);
                var success = result?.GetValueOrDefault("success")?.ToString() == "True";
                var message = result?.GetValueOrDefault("message")?.ToString();
                return (success == true, message);
            }

            var errorContent = await resp.Content.ReadAsStringAsync(ct);
            return (false, "Có lỗi xảy ra khi đặt lại mật khẩu");
        }
        catch (Exception)
        {
            return (false, "Không thể kết nối đến server");
        }
    }

        public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest req, CancellationToken ct = default)
        {
            var resp = await _httpClient.PostAsJsonAsync("/api/account/change-password", req, ct);
            return resp.IsSuccessStatusCode;
        }
    }
}
