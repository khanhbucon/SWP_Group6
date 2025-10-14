using Mo_Entities.Models;

namespace Mo_DataAccess.Services.Interface;

public interface IProductServices : IGenericRepository<Product>
{
    /// <summary>
    /// Get products that belong to shops owned by the specified account (seller).
    /// Includes the Shop navigation to avoid null references.
    /// </summary>
    Task<List<Product>> GetBySellerAccountIdAsync(long accountId);
}
