using Mo_Entities.Models;

namespace Mo_DataAccess.Services.Interface;

public interface IProductServices : IGenericRepository<Product>
{
    /// <summary>
    /// Get products that belong to shops owned by the specified account (seller).
    /// Includes the Shop navigation to avoid null references.
    /// </summary>
    Task<List<Product>> GetBySellerAccountIdAsync(long accountId);

    /// <summary>
    /// Delete a product if it belongs to the given account and it has no orders.
    /// Returns true if deleted, false if not permitted or not found.
    /// </summary>
    Task<bool> DeleteIfOwnedAsync(long productId, long accountId);

    /// <summary>
    /// Compute total stock (sum of variant stock) and total sold quantity (confirmed orders) for a product.
    /// </summary>
    Task<(int totalStock, int totalSold)> GetStockAndSoldAsync(long productId);

    /// <summary>
    /// Get min and max price across all variants of a product.
    /// </summary>
    Task<(decimal? minPrice, decimal? maxPrice)> GetPriceRangeAsync(long productId);
}
