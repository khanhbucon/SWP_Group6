using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo_Api.Extensions;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.ModelRequest;

namespace Mo_Api.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IVnpayTransactionServices _vnpayTransactionServices;
        private readonly IAccountServices _accountServices;
        private readonly IPaymentTransactionServices _paymentTransactionServices;

        public PaymentController(IVnpayTransactionServices vnpayTransactionServices, IAccountServices accountServices, IPaymentTransactionServices paymentTransactionServices)
        {
            _vnpayTransactionServices = vnpayTransactionServices;
            _accountServices = accountServices;
            _paymentTransactionServices = paymentTransactionServices;
        }


        [HttpPost("deposit/vnpay")]
        [Authorize]
        public async Task<IActionResult> CreateVnPayDeposit([FromBody] VnpayDepositRequest request)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { Success = false, Message = "Invalid token - User ID not found" });
                }

                if (!ModelState.IsValid)
                {
                    return ValidationProblem(ModelState);
                }

                var result = await _vnpayTransactionServices.CreateDepositRequestAsync(userId.Value, request);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi tạo yêu cầu nạp tiền" });
            }
        }

        [HttpPost("deposit/verify")]
        [Authorize]
        public async Task<IActionResult> VerifyVnPayDeposit([FromBody] VerifyDepositRequest request)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { Success = false, Message = "Invalid token - User ID not found" });
                }

                var isValid = await _vnpayTransactionServices.VerifyDepositAsync(request.TransactionId, request.Amount);

                if (isValid)
                {
                    var success = await _vnpayTransactionServices.ProcessDepositAsync(userId.Value, request.Amount, request.TransactionId);

                    if (success)
                    {
                        return Ok(new { Success = true, Message = "Nạp tiền thành công" });
                    }
                    else
                    {
                        return BadRequest(new { Success = false, Message = "Có lỗi xảy ra khi xử lý nạp tiền" });
                    }
                }
                else
                {
                    return BadRequest(new { Success = false, Message = "Giao dịch không hợp lệ" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi xác minh giao dịch" });
            }
        }

        [HttpGet("balance")]
        [Authorize]
        public async Task<IActionResult> GetCurrentBalance()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { Success = false, Message = "Invalid token" });
                }

                var account = await _accountServices.GetAccountsByIdAsync(userId.Value);
                if (account == null)
                {
                    return NotFound(new { Success = false, Message = "Account not found" });
                }

                return Ok(new { Success = true, Data = new { CurrentBalance = account.Balance ?? 0 } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi lấy số dư" });
            }
        }

        [HttpGet("transactions")]
        [Authorize]
        public async Task<IActionResult> GetPaymentTransactions()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { Success = false, Message = "Invalid token" });
                }

                var transactions = await _paymentTransactionServices.GetUserTransactionsAsync(userId.Value);
                return Ok(new { Success = true, Data = transactions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi lấy lịch sử giao dịch" });
            }
        }
    }
}

