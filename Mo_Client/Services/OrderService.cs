using Mo_Entities.ModelResponse;
using System.Text.Json;

namespace Mo_Client.Services
{
    public class OrderService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies["accessToken"];
        }

        public async Task<OrderHistoryListResponse?> GetMyOrdersAsync(string? status = null)
        {
            try
            {
                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var queryString = string.IsNullOrEmpty(status) ? "" : $"?status={status}";
                var apiUrl = $"{_configuration["ApiOptions:BaseUrl"]}/api/order/my-orders{queryString}";
                
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    
                    // Handle both wrapped and unwrapped responses
                    using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                    {
                        JsonElement root = doc.RootElement;
                        
                        // If root has a "value" property (ASP.NET Core ActionResult wrapping)
                        if (root.TryGetProperty("value", out JsonElement valueElement))
                        {
                            var result = JsonSerializer.Deserialize<OrderHistoryListResponse>(valueElement.GetRawText(), new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            return result;
                        }
                        // Otherwise, try to parse the root directly
                        else
                        {
                            var result = JsonSerializer.Deserialize<OrderHistoryListResponse>(jsonContent, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Error getting orders
            }

            return null;
        }

        public async Task<OrderHistoryResponse?> GetOrderDetailAsync(long orderId)
        {
            try
            {
                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var apiUrl = $"{_configuration["ApiOptions:BaseUrl"]}/api/order/{orderId}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<OrderHistoryResponse>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return result;
                }
            }
            catch (Exception ex)
            {
                // Error getting order detail
            }

            return null;
        }

        public async Task<object?> GetOrderStatsAsync()
        {
            try
            {
                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var apiUrl = $"{_configuration["ApiOptions:BaseUrl"]}/api/order/stats";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<object>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return result;
                }
            }
            catch (Exception ex)
            {
                // Error getting order stats
            }

            return null;
        }

        public async Task<PurchaseResponse?> PurchaseAsync(long productVariantId, int quantity)
        {
            try
            {
                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    return new PurchaseResponse 
                    { 
                        Success = false, 
                        Message = "Bạn cần đăng nhập để mua hàng" 
                    };
                }

                // Set authorization header (remove existing if any)
                if (_httpClient.DefaultRequestHeaders.Authorization != null)
                {
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                }
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                // Add Accept header if not already present
                if (!_httpClient.DefaultRequestHeaders.Accept.Any(a => a.MediaType == "application/json"))
                {
                    _httpClient.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                }

                var apiUrl = $"{_configuration["ApiOptions:BaseUrl"]}/api/order/purchase";
                
                var requestBody = new
                {
                    ProductVariantId = productVariantId,
                    Quantity = quantity
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, httpContent);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        // Try to parse as PurchaseResponse directly first
                        PurchaseResponse? result = null;
                        
                        // Check if response is wrapped in a "value" or "data" property (ASP.NET Core ActionResult wrapping)
                        using (JsonDocument doc = JsonDocument.Parse(responseContent))
                        {
                            JsonElement root = doc.RootElement;
                            
                            // If root has a "value" property, it's wrapped
                            if (root.TryGetProperty("value", out JsonElement valueElement))
                            {
                                result = JsonSerializer.Deserialize<PurchaseResponse>(valueElement.GetRawText(), new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                            }
                            // If root has a "data" property, it's wrapped
                            else if (root.TryGetProperty("data", out JsonElement dataElement))
                            {
                                result = JsonSerializer.Deserialize<PurchaseResponse>(dataElement.GetRawText(), new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                            }
                            // Otherwise, try to parse the root directly
                            else
                            {
                                result = JsonSerializer.Deserialize<PurchaseResponse>(responseContent, new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                            }
                        }

                        if (result == null)
                        {
                            return new PurchaseResponse 
                            { 
                                Success = false, 
                                Message = "Không thể xử lý phản hồi từ server" 
                            };
                        }

                        // If response status is success but Success field is not set or false, 
                        // check if we have valid data (OrderId > 0 means purchase was successful)
                        if (response.IsSuccessStatusCode)
                        {
                            // If we got an OrderId, it means purchase was successful
                            if (result.OrderId > 0)
                            {
                                result.Success = true;
                            }
                            // If Success is not explicitly false and we have data, treat as success
                            else if (result.Success == false && string.IsNullOrEmpty(result.Message))
                            {
                                result.Success = true;
                                result.Message = "Mua hàng thành công!";
                            }
                        }

                        return result;
                    }
                    catch (JsonException jsonEx)
                    {
                        return new PurchaseResponse 
                        { 
                            Success = false, 
                            Message = $"Lỗi định dạng dữ liệu từ server: {jsonEx.Message}" 
                        };
                    }
                    catch (Exception ex)
                    {
                        return new PurchaseResponse 
                        { 
                            Success = false, 
                            Message = $"Lỗi xử lý phản hồi: {ex.Message}" 
                        };
                    }
                }
                else
                {
                    // Try to parse error message
                    try
                    {
                        var errorResult = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        string errorMessage = "Có lỗi xảy ra khi mua hàng";
                        if (errorResult != null)
                        {
                            if (errorResult.ContainsKey("message"))
                            {
                                errorMessage = errorResult["message"]?.ToString() ?? errorMessage;
                            }
                            else if (errorResult.ContainsKey("Message"))
                            {
                                errorMessage = errorResult["Message"]?.ToString() ?? errorMessage;
                            }
                        }
                        
                        return new PurchaseResponse 
                        { 
                            Success = false, 
                            Message = errorMessage
                        };
                    }
                    catch (Exception parseEx)
                    {
                        return new PurchaseResponse 
                        { 
                            Success = false, 
                            Message = $"Lỗi từ server (Status: {response.StatusCode}): {responseContent}" 
                        };
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                return new PurchaseResponse 
                { 
                    Success = false, 
                    Message = $"Lỗi kết nối: {httpEx.Message}" 
                };
            }
            catch (Exception ex)
            {
                return new PurchaseResponse 
                { 
                    Success = false, 
                    Message = $"Lỗi: {ex.Message}" 
                };
            }
        }

        public async Task<OrderHistoryListResponse?> GetSellerOrdersAsync(string? status = null)
        {
            try
            {
                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var queryString = string.IsNullOrEmpty(status) ? "" : $"?status={status}";
                var apiUrl = $"{_configuration["ApiOptions:BaseUrl"]}/api/order/seller-orders{queryString}";
                
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    
                    // Handle both wrapped and unwrapped responses
                    using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                    {
                        JsonElement root = doc.RootElement;
                        
                        // If root has a "value" property (ASP.NET Core ActionResult wrapping)
                        if (root.TryGetProperty("value", out JsonElement valueElement))
                        {
                            var result = JsonSerializer.Deserialize<OrderHistoryListResponse>(valueElement.GetRawText(), new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            return result;
                        }
                        // Otherwise, try to parse the root directly
                        else
                        {
                            var result = JsonSerializer.Deserialize<OrderHistoryListResponse>(jsonContent, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Error getting seller orders
            }

            return null;
        }

        public async Task<OrderHistoryResponse?> GetSellerOrderDetailAsync(long orderId)
        {
            try
            {
                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var apiUrl = $"{_configuration["ApiOptions:BaseUrl"]}/api/order/seller-orders/{orderId}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<OrderHistoryResponse>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return result;
                }
            }
            catch (Exception ex)
            {
                // Error getting seller order detail
            }

            return null;
        }
    }
}

