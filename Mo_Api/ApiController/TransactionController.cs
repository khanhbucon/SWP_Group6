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
    
    public TransactionController(IPaymentTransactionServices paymentTransactionServices)
    {
        _paymentTransactionServices = paymentTransactionServices;
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
}

