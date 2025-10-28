using Microsoft.EntityFrameworkCore;
using Mo_DataAccess.Repo;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;
using Mo_Entities.ModelResponse;

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

        // Bảo vệ: nếu có bất kỳ đơn đặt hàng nào cho các biến thể của sản phẩm này, không cho phép xóa
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

    //Danh sách quản trị với các phương pháp tìm kiếm và kiểm duyệt
    public async Task<List<AdminProductListItem>> AdminListAsync(string? search)
    {
        var q = Context.Products
            .Include(p => p.Shop)
            .Include(p => p.SubCategory)
                .ThenInclude(sc => sc.Category)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            q = q.Where(p => p.Name.ToLower().Contains(term) || p.Shop.Name.ToLower().Contains(term));
        }

        var items = await q.OrderByDescending(p => p.CreatedAt)
            .Select(p => new AdminProductListItem
            {
                Id = p.Id,
                Name = p.Name,
                ShopName = p.Shop.Name,
                Category = p.SubCategory != null && p.SubCategory.Category != null ? p.SubCategory.Category.Name : string.Empty,
                Price = Context.ProductVariants.Where(v => v.ProductId == p.Id).Select(v => v.Price).FirstOrDefault(),
                SoldCount = 0, // compute below if necessary
                Status = p.IsActive == true ? "Active" : (p.IsActive == false ? "Inactive" : "Pending"),
                CreatedAt = p.CreatedAt,
                Description = p.Description
            }).ToListAsync();

        // compute SoldCount
        if (items.Count > 0)
        {
            var productIds = items.Select(i => i.Id).ToList();
            var variantGroups = await Context.ProductVariants
                .Where(v => productIds.Contains(v.ProductId))
                .Select(v => new { v.ProductId, v.Id })
                .ToListAsync();
            var variantIds = variantGroups.Select(v => v.Id).ToList();
            var sold = await Context.OrderProducts
                .Where(o => variantIds.Contains(o.ProductVariantId) && o.Status == "CONFIRMED")
                .GroupBy(o => o.ProductVariantId)
                .Select(g => new { VariantId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToListAsync();
            var soldByVariant = sold.ToDictionary(x => x.VariantId, x => x.Qty);
            var soldByProduct = variantGroups
                .GroupBy(v => v.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(v => soldByVariant.TryGetValue(v.Id, out var q2) ? q2 : 0));
            foreach (var it in items)
            {
                if (soldByProduct.TryGetValue(it.Id, out var qty)) it.SoldCount = qty;
            }
        }

        return items;
    }

    public async Task<bool> AdminApproveAsync(long productId)
    {
        var p = await Context.Products.FirstOrDefaultAsync(x => x.Id == productId);
        if (p == null) return false;
        p.IsActive = true;
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AdminActivateAsync(long productId)
    {
        var p = await Context.Products.FirstOrDefaultAsync(x => x.Id == productId);
        if (p == null) return false;
        p.IsActive = true;
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AdminSuspendAsync(long productId)
    {
        var p = await Context.Products.FirstOrDefaultAsync(x => x.Id == productId);
        if (p == null) return false;
        p.IsActive = false;
        await Context.SaveChangesAsync();
        return true;
    }
}