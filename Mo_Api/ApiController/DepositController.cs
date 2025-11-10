using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo_Api.Extensions;
using Mo_DataAccess.Services.Interface;
using Mo_Api.Services;
using Mo_Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Mo_Api.ApiController;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DepositController : ControllerBase
{
    private readonly IPaymentTransactionServices _paymentTransactionServices;
    private readonly IAccountServices _accountServices;
    private readonly VietQrService _vietQrService;
    private readonly SePayService _sePayService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DepositController> _logger;
    private readonly SwpGroup6Context _context;

    public DepositController(
        IPaymentTransactionServices paymentTransactionServices,
        IAccountServices accountServices,
        VietQrService vietQrService,
        SePayService sePayService,
        IConfiguration configuration,
        ILogger<DepositController> logger,
        SwpGroup6Context context)
    {
        _paymentTransactionServices = paymentTransactionServices;
        _accountServices = accountServices;
        _vietQrService = vietQrService;
        _sePayService = sePayService;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

   
    [HttpPost("create")]
    public async Task<IActionResult> CreateDeposit([FromBody] CreateDepositRequest request)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin người dùng" });
            }

            // Validate amount
            if (request.Amount <= 0 || request.Amount > 100000000) // Max 100 triệu
            {
                return BadRequest(new { Success = false, Message = "Số tiền không hợp lệ (1 - 100,000,000 VNĐ)" });
            }

            // Lấy thông tin tài khoản ngân hàng từ config
            var accountNumber = _configuration["SePay:BIDVAccount"] ?? "4271054453";
            var accountName = _configuration["SePay:BIDVAccountName"] ?? "VI DUY KHANH";

            // Tạo transaction
            var description = $"Nạp tiền vào tài khoản - {request.Amount:N0} VNĐ";
            var transaction = await _paymentTransactionServices.CreateDepositTransactionAsync(
                userId.Value,
                request.Amount,
                description
            );

            // Save transaction to database
            await _paymentTransactionServices.CreateAsync(transaction);

            // Tạo nội dung QR
            var qrContent = _vietQrService.GenerateQrContent(transaction.Id.ToString(), userId.Value.ToString());

            // Tạo URL QR code
            var qrCodeUrl = _vietQrService.GenerateQrCodeUrl(accountNumber, accountName, request.Amount, qrContent);

            return Ok(new
            {
                Success = true,
                Data = new
                {
                    TransactionId = transaction.Id,
                    amount = request.Amount,
                    QrCodeUrl = qrCodeUrl,
                    QrContent = qrContent,
                    AccountNumber = accountNumber,
                    AccountName = accountName,
                    Message = "Vui lòng quét mã QR để thanh toán. Giao dịch sẽ được xử lý tự động sau khi nhận được xác nhận từ ngân hàng."
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deposit");
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi tạo yêu cầu nạp tiền" });
        }
    }

    /// <summary>
    /// Kiểm tra trạng thái giao dịch nạp tiền
    /// </summary>
    [HttpGet("check/{transactionId}")]
    public async Task<IActionResult> CheckDepositStatus(long transactionId)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin người dùng" });
            }

            var transaction = await _paymentTransactionServices.GetByIdAsync(transactionId);
            if (transaction == null || transaction.UserId != userId.Value)
            {
                return NotFound(new { Success = false, Message = "Giao dịch không tồn tại" });
            }

            return Ok(new
            {
                Success = true,
                Data = new
                {
                    TransactionId = transaction.Id,
                    Amount = transaction.Amount,
                    Status = transaction.Status,
                    CreatedAt = transaction.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking deposit status");
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi kiểm tra trạng thái" });
        }
    }

}

public class CreateDepositRequest
{
    public decimal Amount { get; set; }
}

