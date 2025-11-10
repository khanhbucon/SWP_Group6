using Microsoft.Extensions.Options;
using Mo_Entities.ModelResponse;
using Mo_Client.Models;
using System.Text.Json;

namespace Mo_Client.Services
{
    public class NotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiOptions _apiOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationService(HttpClient httpClient, IOptions<ApiOptions> apiOptions, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _apiOptions = apiOptions.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["accessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<Notification>?> GetMyNotificationsAsync(bool? isRead = null)
        {
            try
            {
                AddAuthorizationHeader();
                var queryString = isRead.HasValue ? $"?isRead={isRead.Value.ToString().ToLower()}" : "";
                var apiUrl = $"{_apiOptions.BaseUrl}/api/Notification/my-notifications{queryString}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<Notification>>>(jsonContent, new JsonSerializerOptions
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

        public async Task<int?> GetUnreadCountAsync()
        {
            try
            {
                AddAuthorizationHeader();
                var apiUrl = $"{_apiOptions.BaseUrl}/api/Notification/unread-count";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<UnreadCountResponse>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result?.Data?.UnreadCount;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> MarkAsReadAsync(long notificationId)
        {
            try
            {
                AddAuthorizationHeader();
                var apiUrl = $"{_apiOptions.BaseUrl}/api/Notification/{notificationId}/read";
                var response = await _httpClient.PostAsync(apiUrl, null);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> MarkAllAsReadAsync()
        {
            try
            {
                AddAuthorizationHeader();
                var apiUrl = $"{_apiOptions.BaseUrl}/api/Notification/mark-all-read";
                var response = await _httpClient.PostAsync(apiUrl, null);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

}
