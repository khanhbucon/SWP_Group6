using Mo_Entities.Models;

namespace Mo_DataAccess.Services.Interface;

public interface INotificationService : IGenericRepository<Notification>
{
    // Order notifications
    Task CreateOrderNotificationAsync(long userId, long orderId, string orderStatus);
    
    // Payment notifications
    Task CreatePaymentNotificationAsync(long userId, long paymentTransactionId, string transactionType, decimal amount);
    
    // Admin/System notifications
    Task CreateSellerApprovalNotificationAsync(long userId);
    Task CreateShopApprovalNotificationAsync(long userId, long shopId);
    Task CreateProductApprovalNotificationAsync(long userId, long productId);
    
    // Generic notification
    Task CreateSystemNotificationAsync(long userId, string type, string title, string content, string? relatedEntityType = null, long? relatedEntityId = null);
    
    // Read notifications
    Task MarkAsReadAsync(long notificationId, long userId);
    Task MarkAllAsReadAsync(long userId);
    Task<int> GetUnreadCountAsync(long userId);
}

