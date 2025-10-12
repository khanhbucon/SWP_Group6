using Mo_Entities.ModelRequest;
using Mo_Entities.ModelResponse;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services.Interface;

public interface IShopServices : IGenericRepository<Shop>
{
    Task<Shop> CreateShopAsync(long accountId, CreateShopRequest request);
    Task<Shop?> GetShopByAccountIdAsync(long accountId);
    Task<Shop> UpdateShopAsync(long shopId, long accountId, UpdateShopRequest request);
    Task<ShopResponse?> GetShopResponseByAccountIdAsync(long accountId);
    Task<ShopStatisticsResponse?> GetShopStatisticsAsync(long accountId);
}