using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo_Api.Extensions;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.ModelResponse;

namespace Mo_Api.ApiController;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly IPaymentTransactionServices _paymentTransactionServices;
    private readonly IAccountServices _accountServices;
    
    public TransactionController(IPaymentTransactionServices paymentTransactionServices, IAccountServices accountServices)
    {
        _paymentTransactionServices = paymentTransactionServices;
        _accountServices = accountServices;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(long id)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Success = false, Message = "Invalid token - User ID not found" });
            }

            var transaction = await _paymentTransactionServices.GetByIdAsync(id);
            if (transaction == null || transaction.UserId != userId.Value)
            {
                return NotFound(new { Success = false, Message = "Giao dịch không tồn tại" });
            }

            return Ok(new { 
                Success = true, 
                Data = new {
                    Id = transaction.Id,
                    Type = transaction.Type,
                    Amount = transaction.Amount,
                    Description = transaction.PaymentDescription,
                    Status = transaction.Status,
                    CreatedAt = transaction.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Success = false, 
                Message = "Có lỗi xảy ra khi lấy thông tin giao dịch" 
            });
        }
    }

    [HttpGet("my-history")]
    public async Task<IActionResult> GetMyTransactionHistory()
    {
        try
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng." });
            }
            
            var transactionHistory = await _paymentTransactionServices.GetUserTransactionHistoryAsync(userId.Value);
            
            if (transactionHistory == null)
            {
                return NotFound(new { message = "Không tìm thấy lịch sử giao dịch." });
            }

            return Ok(new { 
                Success = true, 
                Data = transactionHistory 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Success = false, 
                Message = "Có lỗi xảy ra khi lấy lịch sử giao dịch." 
            });
        }
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllTransactions()
    {
        try
        {
            var transactionHistory = await _paymentTransactionServices.GetAllTransactionsAsync();
            
            if (transactionHistory == null)
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy giao dịch nào." });
            }

            return Ok(new { 
                Success = true, 
                Data = transactionHistory 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Success = false, 
                Message = "Có lỗi xảy ra khi lấy danh sách giao dịch." 
            });
        }
    }

    [HttpPost("admin/{transactionId}/update-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTransactionStatus(long transactionId, [FromBody] UpdateTransactionStatusRequest request)
    {
        try
        {
            var transaction = await _paymentTransactionServices.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                return NotFound(new { Success = false, Message = "Giao dịch không tồn tại." });
            }

            // Chỉ cho phép cập nhật trạng thái cho giao dịch rút tiền
            if (!string.Equals(transaction.Type, "RutTien", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { Success = false, Message = $"Chỉ có thể cập nhật trạng thái cho giao dịch rút tiền. Transaction Type: {transaction.Type}" });
            }

            // Validate status
            if (string.IsNullOrEmpty(request.Status) || 
                (!string.Equals(request.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase) && 
                 !string.Equals(request.Status, "FAILED", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { Success = false, Message = $"Trạng thái không hợp lệ. Chỉ chấp nhận COMPLETED hoặc FAILED. Received: {request.Status}" });
            }

            // Call service to update transaction status with balance handling
            var (success, message) = await _paymentTransactionServices.UpdateWithdrawTransactionStatusAsync(transactionId, request.Status);
            
            if (!success)
            {
                return BadRequest(new { Success = false, Message = message });
            }

            return Ok(new { 
                Success = true, 
                Message = message 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Success = false, 
                Message = $"Có lỗi xảy ra khi cập nhật trạng thái giao dịch: {ex.Message}" 
            });
        }
    }

    [HttpPost("withdraw")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> CreateWithdraw([FromBody] CreateWithdrawRequest request)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Success = false, Message = "Invalid token - User ID not found" });
            }

            // Validate amount
            if (request.Amount <= 0)
            {
                return BadRequest(new { Success = false, Message = "Số tiền rút phải lớn hơn 0" });
            }

            if (request.Amount < 10000)
            {
                return BadRequest(new { Success = false, Message = "Số tiền rút tối thiểu là 10,000 VNĐ" });
            }

            // Get user balance from Account
            var account = await _accountServices.GetByIdAsync(userId.Value);
            if (account == null)
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy tài khoản" });
            }

            var currentBalance = account.Balance ?? 0;
            if (currentBalance < request.Amount)
            {
                return BadRequest(new { Success = false, Message = "Số dư không đủ để rút tiền" });
            }

            // Create withdraw transaction
            var description = string.IsNullOrEmpty(request.Description) 
                ? $"Rút tiền {request.Amount:N0} VNĐ" 
                : request.Description;

            var transaction = await _paymentTransactionServices.CreateWithdrawTransactionAsync(
                userId.Value, 
                request.Amount, 
                description
            );
            
            await _paymentTransactionServices.CreateAsync(transaction);

            return Ok(new
            {
                Success = true,
                Data = new
                {
                    TransactionId = transaction.Id,
                    Amount = transaction.Amount,
                    Status = transaction.Status,
                    Description = transaction.PaymentDescription,
                    CreatedAt = transaction.CreatedAt,
                    Message = "Yêu cầu rút tiền đã được gửi. Vui lòng chờ Admin duyệt."
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Success = false, 
                Message = "Có lỗi xảy ra khi tạo yêu cầu rút tiền." 
            });
        }
    }

    public class UpdateTransactionStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class CreateWithdrawRequest
    {
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}

    