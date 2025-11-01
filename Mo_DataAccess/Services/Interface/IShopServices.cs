using Mo_Entities.ModelRequest;
using Mo_Entities.ModelResponse;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services.Interface;

public interface IShopServices : IGenericRepository<Shop>
{
    Task<Shop> CreateShopAsync(long accountId, CreateShopRequest request);
    Task<Shop?> GetShopByAccountIdAsync(long accountId);
    Task<Shop> UpdateShopAsync(long shopId, long accountId, UpdateShopRequest request);
    // Delete shop by id and account id 
    Task<bool> DeleteShopAsync(long shopId, long accountId);
    // Backward compatible single-shop helpers
    Task<ShopResponse?> GetShopResponseByAccountIdAsync(long accountId);
    Task<ShopStatisticsResponse?> GetShopStatisticsAsync(long accountId);
    // New multi-shop support
    Task<List<ShopResponse>> GetShopsResponseByAccountIdAsync(long accountId);
    Task<ShopResponse?> GetShopResponseByIdAsync(long shopId, long accountId);
    Task<ShopStatisticsResponse?> GetShopStatisticsAsync(long shopId, long accountId);

    // Admin operations
    Task<List<AdminShopListItem>> AdminListShopsAsync(string? search);
    Task<bool> AdminApproveShopAsync(long shopId);
    Task<bool> AdminActivateShopAsync(long shopId);
    Task<bool> AdminSuspendShopAsync(long shopId);

    // Seller operations on status (only when not pending)
    Task<bool> SellerSetActiveAsync(long shopId, long accountId, bool isActive);

    // Public operations
    Task<List<Shop>> GetAllShopsAsync();
}