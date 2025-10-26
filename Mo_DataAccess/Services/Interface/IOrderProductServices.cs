using Mo_Entities.ModelResponse;

namespace Mo_DataAccess.Services.Interface;

public interface IOrderProductServices :IGenericRepository<OrderProduct>
{
    Task<OrderHistoryListResponse> GetUserOrdersAsync(long userId, string? status = null);
    Task<OrderHistoryResponse?> GetOrderDetailAsync(long orderId, long userId);
}