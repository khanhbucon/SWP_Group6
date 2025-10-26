using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mo_Api.Extensions;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;

namespace Mo_Api.ApiController;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly SwpGroup6Context _context;

    public NotificationController(INotificationService notificationService, SwpGroup6Context context)
    {
        _notificationService = notificationService;
        _context = context;
    }

    /// <summary>
    /// Lấy danh sách thông báo của user
    /// </summary>
    [HttpGet("my-notifications")]
    public async Task<IActionResult> GetMyNotifications(bool? isRead = null)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin user" });
            }

            var query = _context.Set<Notification>()
                .Where(n => n.UserId == userId.Value);

            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(new { Success = true, Data = notifications });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Lỗi khi lấy thông báo", Error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy số lượng thông báo chưa đọc
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin user" });
            }

            var count = await _notificationService.GetUnreadCountAsync(userId.Value);

            return Ok(new { Success = true, Data = new { UnreadCount = count } });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Lỗi khi lấy số lượng thông báo", Error = ex.Message });
        }
    }

    /// <summary>
    /// Đánh dấu 1 thông báo đã đọc
    /// </summary>
    [HttpPost("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(long notificationId)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin user" });
            }

            await _notificationService.MarkAsReadAsync(notificationId, userId.Value);

            return Ok(new { Success = true, Message = "Đã đánh dấu đã đọc" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Lỗi khi đánh dấu đã đọc", Error = ex.Message });
        }
    }

    /// <summary>
    /// Đánh dấu tất cả thông báo đã đọc
    /// </summary>
    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin user" });
            }

            await _notificationService.MarkAllAsReadAsync(userId.Value);

            return Ok(new { Success = true, Message = "Đã đánh dấu tất cả đã đọc" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Lỗi khi đánh dấu tất cả đã đọc", Error = ex.Message });
        }
    }
}

