using System.Net.Http.Json;
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
}


