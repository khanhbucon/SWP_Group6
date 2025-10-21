using Mo_Client.Models;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Mo_Client.Services;

public class CategoryService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CategoryService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<CategoryResponse>> GetAllCategoriesAsync(string? searchTerm = null)
    {
        try
        {
            var token = GetTokenFromStorage();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Token không tồn tại");
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var apiUrl = $"{_configuration["Api:BaseUrl"]}/api/Category";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                apiUrl += $"?search={Uri.EscapeDataString(searchTerm)}";
            }
            
            var response = await _httpClient.GetAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<List<CategoryResponse>>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data ?? new List<CategoryResponse>();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi khi lấy danh sách danh mục: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi lấy danh sách danh mục: {ex.Message}");
        }
    }

    public async Task<CategoryResponse> GetCategoryByIdAsync(long id)
    {
        try
        {
            var token = GetTokenFromStorage();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Token không tồn tại");
            }

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"{_configuration["Api:BaseUrl"]}/api/Category/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<CategoryResponse>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data ?? throw new Exception("Không tìm thấy danh mục");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("Không tìm thấy danh mục");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn");
            }
            else
            {
                throw new Exception($"Lỗi khi lấy thông tin danh mục: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi lấy thông tin danh mục: {ex.Message}");
        }
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
    {
        try
        {
            var token = GetTokenFromStorage();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Token không tồn tại");
            }

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_configuration["Api:BaseUrl"]}/api/Category", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<CategoryResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data ?? throw new Exception("Tạo danh mục thất bại");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResult = JsonSerializer.Deserialize<ApiResponse<object>>(errorContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                throw new Exception(errorResult?.Message ?? "Dữ liệu không hợp lệ");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn");
            }
            else
            {
                throw new Exception($"Lỗi khi tạo danh mục: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi tạo danh mục: {ex.Message}");
        }
    }

    public async Task<CategoryResponse> UpdateCategoryAsync(long id, UpdateCategoryRequest request)
    {
        try
        {
            var token = GetTokenFromStorage();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Token không tồn tại");
            }

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_configuration["Api:BaseUrl"]}/api/Category/{id}", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<CategoryResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data ?? throw new Exception("Cập nhật danh mục thất bại");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("Không tìm thấy danh mục");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResult = JsonSerializer.Deserialize<ApiResponse<object>>(errorContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                throw new Exception(errorResult?.Message ?? "Dữ liệu không hợp lệ");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn");
            }
            else
            {
                throw new Exception($"Lỗi khi cập nhật danh mục: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi cập nhật danh mục: {ex.Message}");
        }
    }

    public async Task<bool> DeleteCategoryAsync(long id)
    {
        try
        {
            var token = GetTokenFromStorage();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Token không tồn tại");
            }

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.DeleteAsync($"{_configuration["Api:BaseUrl"]}/api/Category/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("Không tìm thấy danh mục");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResult = JsonSerializer.Deserialize<ApiResponse<object>>(errorContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                throw new Exception(errorResult?.Message ?? "Không thể xóa danh mục");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn");
            }
            else
            {
                throw new Exception($"Lỗi khi xóa danh mục: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi xóa danh mục: {ex.Message}");
        }
    }

    public async Task<CategoryResponse?> CreateCategoryAsync(string name)
    {
        try
        {
            var request = new CreateCategoryRequest { Name = name };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var token = GetTokenFromStorage();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Không tìm thấy token xác thực");
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var apiUrl = $"{_configuration["Api:BaseUrl"]}/api/Category";
            System.Diagnostics.Debug.WriteLine($"API URL: {apiUrl}");
            System.Diagnostics.Debug.WriteLine($"Request JSON: {json}");
            
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(apiUrl, content);
            }
            catch (HttpRequestException ex)
            {
                // Thử URL khác nếu API server không chạy
                System.Diagnostics.Debug.WriteLine($"Primary API failed: {ex.Message}");
                var fallbackUrl = "https://localhost:7234/api/Category";
                System.Diagnostics.Debug.WriteLine($"Trying fallback URL: {fallbackUrl}");
                response = await _httpClient.PostAsync(fallbackUrl, content);
            }
            
            System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");
            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Response Content: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<CategoryResponse>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Success == true && result.Data != null)
                {
                    return result.Data;
                }
                else
                {
                    throw new Exception(result?.Message ?? "Không thể tạo danh mục");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception($"API endpoint không tìm thấy. URL: {apiUrl}");
            }
            else
            {
                throw new Exception($"Lỗi khi tạo danh mục: {response.StatusCode} - {responseContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Lỗi kết nối API: {ex.Message}. Vui lòng kiểm tra API server có đang chạy không.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi tạo danh mục: {ex.Message}");
        }
    }

   

    private string? GetTokenFromStorage()
    {
        // Lấy token từ cookie
        // Token được lưu trong cookie "accessToken"
        return _httpContextAccessor.HttpContext?.Request?.Cookies?["accessToken"];
    }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}
