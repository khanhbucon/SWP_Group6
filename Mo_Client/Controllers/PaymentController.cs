using Microsoft.AspNetCore.Mvc;
using Mo_Client.Models;
using Mo_Client.Services;

namespace Mo_Client.Controllers
{
    public class PaymentController : Controller
    {

        private readonly PaymentService _paymentService;
        private readonly UserService _userService;
        public PaymentController(PaymentService paymentService, UserService userService)
        {
            _paymentService = paymentService;
            _userService = userService;
        }
        [HttpGet]
        public IActionResult Deposit()
        {
            return View(new DepositVm());
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeposit(DepositVm vm)
        {
            try
            {
                var token = Request.Cookies["accessToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login");
                }

                if (!ModelState.IsValid)
                {
                    return View("Deposit", vm);
                }

                _paymentService.SetToken(token);

                var request = new ClientDepositRequest 
                {
                    Amount = vm.Amount,
                    Content = vm.Content,
                    BankCode = "BIDV"
                };

                var response = await _paymentService.CreateVnPayDepositAsync(request);

                // Debug: Log response
                Console.WriteLine($"Response Success: {response.Success}");
                Console.WriteLine($"Response Message: {response.Message}");
                Console.WriteLine($"Response Data: {response.Data?.TransactionId}");

                if (response.Success)
                {
                    // Map ClientDepositData to VnpayDepositData
                    vm.DepositData = new VnpayDepositData
                    {
                        TransactionId = response.Data?.TransactionId ?? "",
                        BankAccount = response.Data?.BankAccount ?? "",
                        BankName = response.Data?.BankName ?? "",
                        Amount = response.Data?.Amount ?? 0,
                        Content = response.Data?.Content ?? "",
                        ExpiredAt = response.Data?.ExpiredAt ?? DateTime.Now,
                        QrCode = response.Data?.QrCode ?? ""
                    };
                    vm.Success = "Tạo yêu cầu nạp tiền thành công!";
                    ViewBag.Success = "Tạo yêu cầu nạp tiền thành công!";
                }
                else
                {
                    vm.Error = response.Message;
                    ViewBag.Error = response.Message;
                }

                return View("Deposit", vm);
            }
            catch (Exception ex)
            {
                vm.Error = "Có lỗi xảy ra: " + ex.Message;
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View("Deposit", vm);
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyDeposit([FromBody] ClientVerifyRequest request)
        {
            try
            {
                var token = Request.Cookies["accessToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập" });
                }

                _paymentService.SetToken(token);
                var response = await _paymentService.VerifyVnPayDepositAsync(request);

                return Json(new { success = response.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            try
            {
                var token = Request.Cookies["accessToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                _paymentService.SetToken(token);
                var history = await _paymentService.GetDepositHistoryAsync();
                
                return View(history);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(new List<ClientPaymentHistoryVm>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Balance()
        {
            try
            {
                var token = Request.Cookies["accessToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                _paymentService.SetToken(token);
                var balance = await _paymentService.GetCurrentBalanceAsync();
                
                return View(balance);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(new ClientPaymentBalanceVm());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Transactions()
        {
            try
            {
                var token = Request.Cookies["accessToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                _paymentService.SetToken(token);
                var transactions = await _paymentService.GetPaymentHistoryAsync();
                
                return View(transactions);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                return View(new List<ClientPaymentTransactionVm>());
            }
        }
    }
}
