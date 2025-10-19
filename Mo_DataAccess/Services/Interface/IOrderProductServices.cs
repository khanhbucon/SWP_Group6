using Mo_Entities.Models;

namespace Mo_DataAccess.Services.Interface
{
    public interface IOrderProductServices
    {
        Task<IEnumerable<OrderProduct>> GetAllAsync();
        Task<OrderProduct?> GetByIdAsync(long id);
        Task<OrderProduct> CreateAsync(OrderProduct order);
        Task<OrderProduct?> UpdateAsync(long id, OrderProduct order);
        Task<bool> DeleteAsync(long id);
    }
}
