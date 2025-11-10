using Mo_Client.Models;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Mo_Client.Services;

public class CategoryService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CategoryService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IOptions<ApiOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<CategoryVm>> GetAllCategoriesAsync(string? searchTerm = null)
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

            var apiUrl = "/api/Category";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                apiUrl += $"?search={Uri.EscapeDataString(searchTerm)}";
            }
            
            var response = await _httpClient.GetAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<List<CategoryVm>>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data ?? new List<CategoryVm>();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn");
            }
            else
            {
                throw new Exception($"Lỗi khi lấy danh sách danh mục: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi lấy danh sách danh mục: {ex.Message}");
        }
    }

    public async Task<CategoryVm> GetCategoryByIdAsync(long id)
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

            var response = await _httpClient.GetAsync($"/api/Category/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<CategoryVm>>(content, new JsonSerializerOptions
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

    public async Task<CategoryVm> CreateCategoryAsync(CreateCategoryVm request)
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

            var response = await _httpClient.PostAsync("/api/Category", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<CategoryVm>>(responseContent, new JsonSerializerOptions
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

    public async Task<CategoryVm> UpdateCategoryAsync(long id, UpdateCategoryVm request)
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

            var response = await _httpClient.PutAsync($"/api/Category/{id}", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<CategoryVm>>(responseContent, new JsonSerializerOptions
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

            var response = await _httpClient.DeleteAsync($"/api/Category/{id}");
            
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

    public async Task<CategoryVm?> CreateCategoryAsync(string name)
    {
        try
        {
            var request = new CreateCategoryVm { Name = name };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var token = GetTokenFromStorage();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Không tìm thấy token xác thực");
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var apiUrl = "/api/Category";
            
            var response = await _httpClient.PostAsync(apiUrl, content);
            
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<CategoryVm>>(responseContent, new JsonSerializerOptions
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
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorResult = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
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

   

    private string? GetTokenFromStorage()
    {
        // Lấy token từ cookie
        // Token được lưu trong cookie "accessToken"
        return _httpContextAccessor.HttpContext?.Request?.Cookies?["accessToken"];
    }

    public async Task<SubCategoryVm?> CreateSubCategoryAsync(long categoryId, string name)
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

            var request = new { CategoryId = categoryId, Name = name };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/SubCategory", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<SubCategoryVm>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result?.Data;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResult = JsonSerializer.Deserialize<ApiResponse<object>>(errorContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                throw new Exception(errorResult?.Message ?? "Không thể tạo danh mục con");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn");
            }
            else
            {
                throw new Exception($"Lỗi khi tạo danh mục con: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi tạo danh mục con: {ex.Message}");
        }
    }

    public async Task<SubCategoryVm?> UpdateSubCategoryAsync(long id, string name, bool isActive = true)
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

            var request = new { Name = name, IsActive = isActive };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/SubCategory/{id}", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<SubCategoryVm>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result?.Data;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResult = JsonSerializer.Deserialize<ApiResponse<object>>(errorContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                throw new Exception(errorResult?.Message ?? "Không thể cập nhật danh mục con");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("Không tìm thấy danh mục con");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn");
            }
            else
            {
                throw new Exception($"Lỗi khi cập nhật danh mục con: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi cập nhật danh mục con: {ex.Message}");
        }
    }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}
