using Microsoft.AspNetCore.Mvc;
using Mo_Client.Services;
using Mo_Entities.ModelResponse;

namespace Mo_Client.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> MyOrders(string? status = null)
        {
            // Check if user is authenticated
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem đơn hàng của bạn.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var orders = await _orderService.GetMyOrdersAsync(status);
                
                if (orders == null)
                {
                    ViewBag.Error = "Không thể tải đơn hàng. Vui lòng đăng nhập lại.";
                    return View(new OrderHistoryListResponse());
                }

                return View(orders);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra khi tải đơn hàng.";
                return View(new OrderHistoryListResponse());
            }
        }

        public async Task<IActionResult> Detail(long id)
        {
            // Check if user is authenticated
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem chi tiết đơn hàng.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var order = await _orderService.GetOrderDetailAsync(id);
                
                if (order == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("MyOrders");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải chi tiết đơn hàng.";
                return RedirectToAction("MyOrders");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(long variantId, int quantity)
        {
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { Success = false, Message = "Bạn cần đăng nhập để mua hàng" });
            }

            if (variantId <= 0)
            {
                return Json(new { Success = false, Message = "Variant ID không hợp lệ" });
            }

            if (quantity <= 0)
            {
                return Json(new { Success = false, Message = "Số lượng phải lớn hơn 0" });
            }

            try
            {
                var result = await _orderService.PurchaseAsync(variantId, quantity);
                
                if (result?.Success == true)
                {
                    return Json(new 
                    { 
                        Success = true, 
                        OrderId = result.OrderId,
                        Message = result.Message ?? "Mua hàng thành công!",
                        TotalAmount = result.TotalAmount,
                        ProductCodes = result.ProductCodes
                    });
                }
                else
                {
                    return Json(new 
                    { 
                        Success = false, 
                        Message = result?.Message ?? "Có lỗi xảy ra khi mua hàng" 
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}

