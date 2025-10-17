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
    //Xoa sp neu sp thuoc ve tai khoan va chua co don hang nao
    //•	Không cho xóa nếu sản phẩm đã phát sinh đơn (có OrderProducts qua các ProductVariants).
    public async Task<bool> DeleteIfOwnedAsync(long productId, long accountId)
    {
        var product = await Context.Products
            .Include(p => p.Shop)
            .FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return false;
        if (product.Shop.AccountId != accountId) return false; // forbid

        // Guard: if any orders exist for variants of this product, disallow delete
        var hasOrders = await Context.OrderProducts
            .AnyAsync(o => Context.ProductVariants
                .Where(v => v.ProductId == productId)
                .Select(v => v.Id)
                .Contains(o.ProductVariantId));
        if (hasOrders) return false;

        Context.Products.Remove(product);
        await Context.SaveChangesAsync();
        return true;
    }
    // Tinh tong so luong ton kho (tong so luong cac variant) va tong so luong da ban (don da xac nhan) cho 1 san pham
    public async Task<(int totalStock, int totalSold)> GetStockAndSoldAsync(long productId)
    {
        var variantIds = await Context.ProductVariants
            .Where(v => v.ProductId == productId)
            .Select(v => new { v.Id, v.Stock })
            .ToListAsync();

        var totalStock = variantIds.Sum(v => v.Stock ?? 0);

        var ids = variantIds.Select(v => v.Id).ToList();
        int totalSold = 0;
        if (ids.Count > 0)
        {
            totalSold = await Context.OrderProducts
                .Where(o => ids.Contains(o.ProductVariantId) && o.Status == "CONFIRMED")
                .SumAsync(o => (int?)o.Quantity) ?? 0;
        }
        return (totalStock, totalSold);
    }
    // Lấy giá min và max trên tất cả các variant của một sản phẩm
    public async Task<(decimal? minPrice, decimal? maxPrice)> GetPriceRangeAsync(long productId)
    {
        var prices = await Context.ProductVariants
            .Where(v => v.ProductId == productId)
            .Select(v => (decimal?)v.Price)
            .ToListAsync();
        if (prices.Count == 0) return (null, null);
        return (prices.Min(), prices.Max());
    }
}
