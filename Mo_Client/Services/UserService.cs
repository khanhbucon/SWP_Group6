using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Mo_Client.Models;

namespace Mo_Client.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient, IOptions<ApiOptions> options)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
        }

        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // Request/Response Models
        public record UpdateProfileRequest(string Username, string Email, string? Phone, string? IdentificationF, string? IdentificationB);
        public record ApiResponse<T>(bool Success, T? Data, string? Message);

        // User Profile Methods
        public async Task<ProfileResponse?> GetCurrentUserProfileAsync(CancellationToken ct = default)
        {
            var resp = await _httpClient.GetAsync("/api/account/profile", ct);
            if (!resp.IsSuccessStatusCode) return null;

            var result = await resp.Content.ReadFromJsonAsync<ApiResponse<ProfileResponse>>(cancellationToken: ct);
            return result?.Data;
        }

        public async Task<bool> UpdateProfileAsync(UpdateProfileRequest req, CancellationToken ct = default)
        {
            var resp = await _httpClient.PutAsJsonAsync("/api/account/update-profile", req, ct);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> UploadKYCAsync(IFormFile identificationF, IFormFile identificationB, CancellationToken ct = default)
        {
            using var formData = new MultipartFormDataContent();
            formData.Add(new StreamContent(identificationF.OpenReadStream()), "identificationF", identificationF.FileName);
            formData.Add(new StreamContent(identificationB.OpenReadStream()), "identificationB", identificationB.FileName);

            var resp = await _httpClient.PostAsync("/api/account/upload-kyc", formData, ct);
            return resp.IsSuccessStatusCode;
        }
    }
}
