using Microsoft.EntityFrameworkCore;
using Mo_DataAccess.Repo;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services;

public class ProductServices : GenericRepository<Product>, IProductServices
{
    public ProductServices(SwpGroup6Context context) : base(context)
    {
    }

    public async Task<List<Product>> GetBySellerAccountIdAsync(long accountId)
    {
        return await Context.Products
            .Include(p => p.Shop)
            .Where(p => p.Shop.AccountId == accountId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
