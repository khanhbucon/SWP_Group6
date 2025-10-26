using Microsoft.AspNetCore.Mvc;
using Mo_Client.Services;
using Mo_Client.Models;

namespace Mo_Client.Controllers
{
    public class NotificationController : Controller
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(bool? isRead = null)
        {
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem thông báo.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var notifications = await _notificationService.GetMyNotificationsAsync(isRead);
                var unreadCount = await _notificationService.GetUnreadCountAsync();

                ViewBag.IsReadFilter = isRead;
                ViewBag.UnreadCount = unreadCount ?? 0;
                ViewBag.TotalCount = notifications?.Count ?? 0;

                return View(notifications ?? new List<Notification>());
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra khi tải thông báo.";
                return View(new List<Notification>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(long notificationId)
        {
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            try
            {
                var success = await _notificationService.MarkAsReadAsync(notificationId);
                return Json(new { success = success, message = success ? "Đã đánh dấu đã đọc" : "Có lỗi xảy ra" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi đánh dấu đã đọc" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            try
            {
                var success = await _notificationService.MarkAllAsReadAsync();
                return Json(new { success = success, message = success ? "Đã đánh dấu tất cả đã đọc" : "Có lỗi xảy ra" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi đánh dấu tất cả đã đọc" });
            }
        }
    }
}
