using Microsoft.EntityFrameworkCore;
using Mo_Entities.Models;
using Mo_DataAccess.Services.Interface;
using Mo_DataAccess.Repo;

namespace Mo_DataAccess.Services;

public class NotificationService : GenericRepository<Notification>, INotificationService
{
    public NotificationService(SwpGroup6Context context) : base(context)
    {
    }

    public async Task CreateOrderNotificationAsync(long userId, long orderId, string orderStatus)
    {
        var title = orderStatus switch
        {
            "PENDING" => "Đơn hàng đang xử lý",
            "CONFIRMED" => "Đơn hàng đã được xác nhận",
            "COMPLETED" => "Đơn hàng đã hoàn thành",
            "CANCELLED" => "Đơn hàng đã hủy",
            _ => "Cập nhật đơn hàng"
        };

        var content = orderStatus switch
        {
            "PENDING" => $"Đơn hàng #{orderId} của bạn đang được xử lý.",
            "CONFIRMED" => $"Đơn hàng #{orderId} đã được xác nhận. Mã sản phẩm đã được gửi cho bạn.",
            "COMPLETED" => $"Đơn hàng #{orderId} đã hoàn thành. Cảm ơn bạn đã mua hàng!",
            "CANCELLED" => $"Đơn hàng #{orderId} đã bị hủy.",
            _ => $"Đơn hàng #{orderId} đã được cập nhật."
        };

        var notification = new Notification
        {
            UserId = userId,
            Type = "Order",
            Title = title,
            Content = content,
            RelatedEntityType = "OrderProduct",
            RelatedEntityId = orderId,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        await _context.Set<Notification>().AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task CreatePaymentNotificationAsync(long userId, long paymentTransactionId, string transactionType, decimal amount)
    {
        var (title, content) = transactionType switch
        {
            "MuaHang" => (
                "Thanh toán mua hàng",
                $"Bạn đã thanh toán {amount:N0} VNĐ cho đơn hàng."
            ),
            "NapTien" => (
                "Nạp tiền thành công",
                $"Bạn đã nạp {amount:N0} VNĐ vào tài khoản."
            ),
            "BanHang" => (
                "Nhận tiền bán hàng",
                $"Bạn đã nhận {amount:N0} VNĐ từ việc bán hàng."
            ),
            "RutTien" => (
                "Rút tiền",
                $"Bạn đã rút {amount:N0} VNĐ từ tài khoản."
            ),
            _ => (
                "Giao dịch mới",
                $"Giao dịch {amount:N0} VNĐ đã được thực hiện."
            )
        };

        var notification = new Notification
        {
            UserId = userId,
            Type = "Payment",
            Title = title,
            Content = content,
            RelatedEntityType = "PaymentTransaction",
            RelatedEntityId = paymentTransactionId,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        await _context.Set<Notification>().AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(long notificationId, long userId)
    {
        var notification = await _context.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(long userId)
    {
        var notifications = await _context.Set<Notification>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(long userId)
    {
        return await _context.Set<Notification>()
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    // Admin/System notifications
    public async Task CreateSellerApprovalNotificationAsync(long userId)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = "Admin",
            Title = "Được cấp quyền Seller",
            Content = "Chúc mừng! Bạn đã được admin cấp quyền làm seller. Bạn có thể tạo shop và bán sản phẩm ngay bây giờ.",
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        await _context.Set<Notification>().AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task CreateShopApprovalNotificationAsync(long userId, long shopId)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = "Shop",
            Title = "Shop đã được duyệt",
            Content = $"Shop của bạn đã được admin duyệt. Bạn có thể bắt đầu bán sản phẩm ngay!",
            RelatedEntityType = "Shop",
            RelatedEntityId = shopId,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        await _context.Set<Notification>().AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task CreateProductApprovalNotificationAsync(long userId, long productId)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = "Product",
            Title = "Sản phẩm đã được duyệt",
            Content = $"Sản phẩm của bạn đã được admin duyệt. Sản phẩm đã có sẵn trên hệ thống.",
            RelatedEntityType = "Product",
            RelatedEntityId = productId,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        await _context.Set<Notification>().AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task CreateSystemNotificationAsync(long userId, string type, string title, string content, string? relatedEntityType = null, long? relatedEntityId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Content = content,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        await _context.Set<Notification>().AddAsync(notification);
        await _context.SaveChangesAsync();
    }
}

