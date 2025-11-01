using Microsoft.AspNetCore.Mvc;
using Mo_Client.Services;
using Mo_Entities.ModelResponse;

namespace Mo_Client.Controllers
{
    public class TransactionController : Controller
    {
        private readonly TransactionService _transactionService;

        public TransactionController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public async Task<IActionResult> History()
        {
            // Check if user is authenticated by checking token in cookie
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem lịch sử giao dịch.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var transactionHistory = await _transactionService.GetTransactionHistoryAsync();
                
                if (transactionHistory == null)
                {
                    ViewBag.Error = "Không thể tải lịch sử giao dịch. Vui lòng đăng nhập lại.";
                    return View(new TransactionHistoryListResponse());
                }

                return View(transactionHistory);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra khi tải lịch sử giao dịch.";
                return View(new TransactionHistoryListResponse());
            }
        }
    }
}