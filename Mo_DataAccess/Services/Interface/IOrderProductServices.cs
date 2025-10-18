namespace Mo_DataAccess.Services.Interface;

public interface IOrderProductServices :IGenericRepository<OrderProduct>
{
    Task<IEnumerable<OrderProduct>> GetOrdersByAccountIdAsync(long accountId);
    Task UpdateStatusAsync(long orderId, string status);
}