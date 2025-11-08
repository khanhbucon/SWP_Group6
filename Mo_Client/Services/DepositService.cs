using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Mo_Client.Services;

public class DepositService
{
    private readonly HttpClient _httpClient;

    public DepositService(HttpClient httpClient, IOptions<ApiOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    // Request/Response Models
    public record CreateDepositRequest(decimal Amount);
    
    public record CreateDepositResponse(bool Success, DepositData? Data, string? Message);
    
    public record DepositData(
        long TransactionId,
        decimal Amount,
        string QrCodeUrl,
        string QrContent,
        string AccountNumber,
        string AccountName,
        string Message
    );

    public record CheckDepositResponse(bool Success, DepositStatusData? Data, string? Message);
    
    public record DepositStatusData(
        long TransactionId,
        decimal Amount,
        string Status,
        DateTime CreatedAt
    );

    // API Methods
    public async Task<CreateDepositResponse?> CreateDepositAsync(CreateDepositRequest request, CancellationToken ct = default)
    {
        var resp = await _httpClient.PostAsJsonAsync("/api/deposit/create", request, ct);
        var result = await resp.Content.ReadFromJsonAsync<CreateDepositResponse>(cancellationToken: ct);
        return result;
    }

    public async Task<CheckDepositResponse?> CheckDepositStatusAsync(long transactionId, CancellationToken ct = default)
    {
        var resp = await _httpClient.GetAsync($"/api/deposit/check/{transactionId}", ct);
        if (!resp.IsSuccessStatusCode) return null;
        var result = await resp.Content.ReadFromJsonAsync<CheckDepositResponse>(cancellationToken: ct);
        return result;
    }
}

