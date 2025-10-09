using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Mo_Client.Services;

public class AuthApiClient
{
    private readonly HttpClient _httpClient;
    public AuthApiClient(HttpClient httpClient, IOptions<ApiOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
    }

    public record LoginRequest(string Identifier, string Password, bool RememberMe);
    public record LoginResponse(string AccessToken, DateTime ExpiresAt, string? RefreshToken, List<string>? Roles);
    
    public record RegisterRequest(string Username, string Email, string Phone, string Password);
    public record RegisterResponse(bool Success, string? Message);

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

  
}


