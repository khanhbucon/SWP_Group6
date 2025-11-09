using Mo_Entities.ModelRequest;
using Mo_Entities.ModelResponse;

namespace Mo_DataAccess.Services.Interface;

public interface IOrderProductServices :IGenericRepository<OrderProduct>
{
    Task<OrderHistoryListResponse> GetUserOrdersAsync(long userId, string? status = null);
    Task<OrderHistoryResponse?> GetOrderDetailAsync(long orderId, long userId);

    Task<PurchaseResponse> PurchaseProductAsync(long userId, PurchaseRequest request);

    // Queue-based flow
    Task<(bool Success, string Message, long OrderId, string IdempotencyKey)> PreparePurchaseAsync(long userId, PurchaseRequest request);
    Task ProcessPurchaseJobAsync(long userId, long orderId, PurchaseRequest request, string idempotencyKey);

    Task<bool> ReleaseSellerPayoutAsync(long orderId);
}