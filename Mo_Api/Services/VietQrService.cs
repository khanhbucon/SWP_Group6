using System.Text;

namespace Mo_Api.Services;

public class VietQrService
{
    private readonly IConfiguration _configuration;

    public VietQrService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Tạo URL QR code VietQR cho nạp tiền
    /// </summary>
    public string GenerateQrCodeUrl(string accountNumber, string accountName, decimal amount, string content)
    {
        // BIDV bank code (970418)
        var bankCode = "970418";
        
        // Tạo URL QR code image từ VietQR API
        // Format: https://img.vietqr.io/image/{BANK_CODE}-{ACCOUNT_NUMBER}-compact2.png?amount={AMOUNT}&addInfo={CONTENT}&accountName={ACCOUNT_NAME}
        var qrUrl = $"https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.png?amount={amount:0}&addInfo={Uri.EscapeDataString(content)}&accountName={Uri.EscapeDataString(accountName)}";
        
        return qrUrl;
    }

    /// <summary>
    /// Tạo nội dung chuyển khoản theo chuẩn VietQR
    /// </summary>
    public string GenerateQrContent(string transactionId, string userId)
    {
        // Format: NAPTIEN_{TransactionId}_{UserId}
        return $"NAPTIEN_{transactionId}_{userId}";
    }
}

