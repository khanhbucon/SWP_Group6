using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mo_Entities.ModelRequest;
using Mo_Entities.ModelResponse;
using System.Text.Json;

namespace Mo_DataAccess.Services;

public class VnpayTransactionServices:GenericRepository<VnpayTransaction>,IVnpayTransactionServices
{
    private readonly IAccountServices _accountServices;
    private readonly IPaymentTransactionServices _paymentTransactionServices;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public VnpayTransactionServices(
        SwpGroup6Context context,
        IAccountServices accountServices,
        IPaymentTransactionServices paymentTransactionServices,
        HttpClient httpClient,
        IConfiguration configuration) : base(context)
    {
        _accountServices = accountServices;
        _paymentTransactionServices = paymentTransactionServices;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<VnpayDepositResponse> CreateDepositRequestAsync(long userId, VnpayDepositRequest request)
    {
        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Tạo transaction ID duy nhất
            var transactionId = $"VNPAY_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";

            // Tạo PaymentTransaction
            var paymentTransaction = await _paymentTransactionServices.CreateDepositTransactionAsync(
                userId,
                request.Amount,
                $"Nạp tiền qua VnPay - {request.Content}"
            );

            // Tạo VnpayTransaction
            var vnpayTransaction = new VnpayTransaction
            {
                PaymentTransactionId = paymentTransaction.Id,
                Date = DateTime.UtcNow,
                Content = request.Content,
                BankName = "BIDV",
                PaymentAccount = _configuration["VnPay:BIDVAccount"] ?? "1234567890", // Số tài khoản BIDV của bạn
                PaymentNumber = transactionId,
                Value = request.Amount
            };

            await _context.AddAsync(vnpayTransaction);
            await transaction.CommitAsync();

            // Tạo QR Code content
            var qrContent = GenerateQRContent(vnpayTransaction);

            return new VnpayDepositResponse
            {
                Success = true,
                Message = "Tạo yêu cầu nạp tiền thành công",
                Data = new VnpayDepositData
                {
                    TransactionId = transactionId,
                    BankAccount = vnpayTransaction.PaymentAccount,
                    BankName = vnpayTransaction.BankName,
                    Amount = request.Amount,
                    Content = request.Content,
                    ExpiredAt = DateTime.UtcNow.AddMinutes(15), // Hết hạn sau 15 phút
                    QrCode = qrContent
                }
            };
        }
        catch (Exception ex)
        {
            return new VnpayDepositResponse
            {
                Success = false,
                Message = $"Có lỗi xảy ra: {ex.Message}"
            };
        }
    }

    public async Task<bool> VerifyDepositAsync(string transactionId, decimal amount)
    {
        try
        {
            // Gọi API VnPay để verify transaction (hoặc SePay nếu bạn dùng SePay)
            var verifyRequest = new
            {
                TransactionId = transactionId,
                Amount = amount,
                ApiKey = _configuration["VnPay:ApiKey"]
            };

            var json = JsonSerializer.Serialize(verifyRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_configuration["VnPay:VerifyUrl"], content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<VnPayVerifyResponse>(responseContent);
                return result?.Success == true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ProcessDepositAsync(long userId, decimal amount, string transactionId)
    {
        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Cập nhật số dư tài khoản
            var account = await _accountServices.GetAccountsByIdAsync(userId);
            if (account == null) return false;

            account.Balance = (account.Balance ?? 0) + amount;
            account.UpdateAt = DateTime.UtcNow;

            // Cập nhật trạng thái PaymentTransaction
            var vnpayTransaction = await _context.VnpayTransactions
                .FirstOrDefaultAsync(vt => vt.PaymentNumber == transactionId);

            if (vnpayTransaction != null)
            {
                await _paymentTransactionServices.UpdateTransactionStatusAsync(
                    vnpayTransaction.PaymentTransactionId,
                    "COMPLETED"
                );
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateQRContent(VnpayTransaction transaction)
    {
        // Tạo nội dung QR code theo chuẩn VietQR
        var qrContent = $"https://img.vietqr.io/image/{transaction.BankName}-{transaction.PaymentAccount}-compact2.jpg?amount={transaction.Value}&addInfo={Uri.EscapeDataString(transaction.Content)}&accountName=YOUR_ACCOUNT_NAME";
        return qrContent;
    }
}

public class VnPayVerifyResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

