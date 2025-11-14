using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Mo_Api.Services;

public class SePayService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SePayService> _logger;

    public SePayService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<SePayService> logger)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        // SePay thường dùng my.sepay.vn
        var apiUrl = _configuration["SePay:ApiUrl"] ?? "https://my.sepay.vn";
        _httpClient.BaseAddress = new Uri(apiUrl);
        _logger = logger;
        _logger.LogInformation($"SePay Service initialized with BaseAddress: {apiUrl}");
    }

    /// Kiểm tra giao dịch từ SEpay theo transaction ID
    public async Task<SePayTransactionResponse?> CheckTransactionAsync(string transactionId)
    {
        try
        {
            var apiKey = _configuration["SePay:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("SePay API Key is not configured");
                return null;
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.GetAsync($"/api/v1/transactions/{transactionId}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SePayTransactionResponse>();
                return result;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"SePay API Error: {response.StatusCode} - {errorContent}");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking SePay transaction");
            return null;
        }
    }

    /// Lấy danh sách giao dịch gần đây từ SEpay để tìm giao dịch khớp
    public async Task<List<SePayTransactionData>> GetRecentTransactionsAsync(DateTime? fromDate = null, int limit = 50)
    {
        try
        {
            var apiKey = _configuration["SePay:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("SePay API Key is not configured");
                return new List<SePayTransactionData>();
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var from = fromDate ?? DateTime.UtcNow.AddHours(-24); // Mặc định 24 giờ gần đây
            // Format ISO 8601 với timezone
            var fromStr = from.ToString("yyyy-MM-ddTHH:mm:ssZ");
            
            // Endpoint đúng của SePay là /userapi/transactions/list
            var url = $"/userapi/transactions/list?from={Uri.EscapeDataString(fromStr)}&limit={limit}";
            
            _logger.LogInformation($"SePay API URL: {_httpClient.BaseAddress}{url}");

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation($"SePay API Response (Status {response.StatusCode}): {content}");
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // Parse response với format thực tế của SePay
                    var result = await response.Content.ReadFromJsonAsync<SePayTransactionsListResponse>();
                    
                    if (result != null && result.Transactions != null)
                    {
                        // Map từ format SePay sang format internal
                        var mappedTransactions = new List<SePayTransactionData>();
                        foreach (var t in result.Transactions)
                        {
                            try
                            {
                                var amount = decimal.Parse(t.AmountIn ?? "0");
                                var mapped = new SePayTransactionData
                                {
                                    TransactionId = t.Id,
                                    Amount = amount,
                                    Status = "SUCCESS", // SePay chỉ trả về giao dịch thành công
                                    Description = t.TransactionContent,
                                    CreatedAt = DateTime.TryParse(t.TransactionDate, out var date) ? date : null,
                                    AccountNumber = t.AccountNumber,
                                    AccountName = null // SePay không trả về account name
                                };
                                mappedTransactions.Add(mapped);
                                
                                // Log first few transactions for debugging
                                if (mappedTransactions.Count <= 3)
                                {
                                    _logger.LogInformation($"Mapped transaction: Raw AmountIn='{t.AmountIn}', Parsed Amount={mapped.Amount}, Description='{mapped.Description}'");
                                }
                            }
                            catch (Exception mapEx)
                            {
                                _logger.LogError(mapEx, $"Error mapping transaction {t.Id}: AmountIn='{t.AmountIn}', TransactionContent='{t.TransactionContent}'");
                            }
                        }
                        
                        _logger.LogInformation($"Successfully parsed {mappedTransactions.Count} transactions from SePay");
                        return mappedTransactions;
                    }
                }
                catch (Exception parseEx)
                {
                    _logger.LogError(parseEx, $"Error parsing SePay response. Content: {content}");
                }
            }
            else
            {
                _logger.LogError($"SePay API Error: {response.StatusCode} - {content}");
            }
            
            return new List<SePayTransactionData>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent transactions from SePay");
            return new List<SePayTransactionData>();
        }
    }

    
    /// Tìm giao dịch khớp với amount và description
    public async Task<SePayTransactionData?> FindMatchingTransactionAsync(decimal amount, string description, DateTime? transactionCreatedAt = null)
    {
        try
        {
            var transactions = await GetRecentTransactionsAsync();
            
            _logger.LogInformation($"Searching for transaction: Amount={amount}, Description={description}");
            _logger.LogInformation($"Found {transactions.Count} recent transactions from SePay");
            
           
            
            // Tìm theo amount 
            var amountMatches = transactions.Where(t => Math.Abs(t.Amount - amount) < 0.01m).ToList();
            _logger.LogInformation($"Found {amountMatches.Count} transactions matching amount {amount}");
            
            // Nếu có description, tìm trong các giao dịch khớp amount
            
            if (!string.IsNullOrEmpty(description))
            {
                string? transactionIdStr = null;
                if (description.Contains("_"))
                {
                    var parts = description.Split('_');
                    if (parts.Length >= 2)
                    {
                        transactionIdStr = parts[1]; // Lấy transaction ID (ví dụ: "8013" từ "NAPTIEN_8013_1")
                    }
                }
                else if (description.StartsWith("NAPTIEN", StringComparison.OrdinalIgnoreCase))
                {
                    // Nếu description đã là format SePay (NAPTIEN{TransactionId}), extract transaction ID
                    transactionIdStr = description.Substring(7); // Bỏ "NAPTIEN" prefix
                }
                
                _logger.LogInformation($"Searching for description. Original: {description}, Extracted TransactionId: {transactionIdStr}");
                
                // Ưu tiên 1: Tìm exact match với format SePay "NAPTIEN{TransactionId}" (ví dụ: "NAPTIEN8013")
                if (!string.IsNullOrEmpty(transactionIdStr))
                {
                    var sepayFormat = $"NAPTIEN{transactionIdStr}"; // Format chính xác của SePay
                    _logger.LogInformation($"Priority search: Looking for exact match '{sepayFormat}'");
                    
                    var exactMatch = amountMatches.FirstOrDefault(t => 
                        !string.IsNullOrEmpty(t.Description) && 
                        t.Description.Equals(sepayFormat, StringComparison.OrdinalIgnoreCase));
                    
                    if (exactMatch != null)
                    {
                        _logger.LogInformation($"✓ Found exact match: SePay ID={exactMatch.TransactionId}, Amount={exactMatch.Amount}, Description={exactMatch.Description}");
                        return exactMatch;
                    }
                    
                    // Thử với space: "NAPTIEN {TransactionId}"
                    var sepayFormatWithSpace = $"NAPTIEN {transactionIdStr}";
                    exactMatch = amountMatches.FirstOrDefault(t => 
                        !string.IsNullOrEmpty(t.Description) && 
                        t.Description.Equals(sepayFormatWithSpace, StringComparison.OrdinalIgnoreCase));
                    
                    if (exactMatch != null)
                    {
                        _logger.LogInformation($"✓ Found exact match (with space): SePay ID={exactMatch.TransactionId}, Amount={exactMatch.Amount}, Description={exactMatch.Description}");
                        return exactMatch;
                    }
                }
                
                // Ưu tiên 2: Tìm với Contains (fallback)
                var descriptionVariants = new List<string>();
                
                // Thêm description gốc
                descriptionVariants.Add(description); // NAPTIEN_{TransactionId}_{UserId}
                descriptionVariants.Add(description.Replace("_", "")); // NAPTIEN{TransactionId}{UserId}
                descriptionVariants.Add(description.Replace("_", " ")); // NAPTIEN {TransactionId} {UserId}
                
                // Nếu có transaction ID, tạo format SePay
                if (!string.IsNullOrEmpty(transactionIdStr))
                {
                    descriptionVariants.Add($"NAPTIEN{transactionIdStr}"); // NAPTIEN{TransactionId} - format SePay
                    descriptionVariants.Add($"NAPTIEN {transactionIdStr}"); // NAPTIEN {TransactionId}
                    descriptionVariants.Add(transactionIdStr); // Chỉ transaction ID
                }
                
                _logger.LogInformation($"Fallback search: Trying variants with Contains: {string.Join(", ", descriptionVariants.Distinct().Take(5))}");
                
                // Tìm trong các giao dịch khớp amount với Contains
                foreach (var variant in descriptionVariants.Distinct())
                {
                    var matching = amountMatches.FirstOrDefault(t => 
                        !string.IsNullOrEmpty(t.Description) && 
                        t.Description.Contains(variant, StringComparison.OrdinalIgnoreCase));
                    
                    if (matching != null)
                    {
                        _logger.LogInformation($"✓ Found match by Contains variant '{variant}': SePay ID={matching.TransactionId}, Amount={matching.Amount}, Description={matching.Description}");
                        return matching;
                    }
                }
                
                
            }
            
            // KHÔNG match chỉ theo amount để tránh match với giao dịch cũ
            // Phải có description khớp mới được match
            // Nếu có transactionCreatedAt, chỉ match giao dịch SePay sau thời điểm tạo transaction
            if (transactionCreatedAt.HasValue)
            {
                var timeFilteredMatches = amountMatches.Where(t => 
                    t.CreatedAt.HasValue &&
                    t.CreatedAt.Value >= transactionCreatedAt.Value.AddMinutes(-2) && // Cho phép sai lệch 2 phút
                    (t.Status == "SUCCESS" || t.Status == "COMPLETED")).ToList();
                
                if (timeFilteredMatches.Any())
                {
                    _logger.LogInformation($"Found {timeFilteredMatches.Count} transactions matching amount and time, but no description match. Transaction created at: {transactionCreatedAt.Value}");
                }
            }
            
            // Không match chỉ theo amount - phải có description khớp
            _logger.LogInformation("No transaction found matching both amount and description. Returning null to avoid matching old transactions.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding matching transaction");
            return null;
        }
    }

   
    }


public class SePayTransactionResponse
{
    public bool Success { get; set; }
    public SePayTransactionData? Data { get; set; }
    public string? Message { get; set; }
}

public class SePayTransactionData
{
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
}

// Response format thực tế từ SePay API
public class SePayTransactionsListResponse
{
    public int Status { get; set; }
    public string? Error { get; set; }
    public SePayMessages? Messages { get; set; }
    public List<SePayTransactionRaw>? Transactions { get; set; }
}

public class SePayMessages
{
    public bool Success { get; set; }
}

// Format raw transaction từ SePay API
public class SePayTransactionRaw
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("bank_brand_name")]
    public string? BankBrandName { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("account_number")]
    public string? AccountNumber { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("transaction_date")]
    public string? TransactionDate { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("amount_out")]
    public string? AmountOut { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("amount_in")]
    public string? AmountIn { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("accumulated")]
    public string? Accumulated { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("transaction_content")]
    public string? TransactionContent { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("reference_number")]
    public string? ReferenceNumber { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("code")]
    public string? Code { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("sub_account")]
    public string? SubAccount { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("bank_account_id")]
    public string? BankAccountId { get; set; }
}

