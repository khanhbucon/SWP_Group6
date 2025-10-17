using Microsoft.EntityFrameworkCore;
using Mo_DataAccess.Repo;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.ModelRequest;
using Mo_Entities.ModelResponse;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services;

public class ShopServices : GenericRepository<Shop>, IShopServices
{
    public ShopServices(SwpGroup6Context context) : base(context)
    {
    }

    public async Task<Shop> CreateShopAsync(long accountId, CreateShopRequest request)
    {
        // chuẩn hóa đầu vào
        var name = request.Name?.Trim();
        var description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description!.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Tên shop không được để trống");
        }

        // Ensure account exists
        var accountExists = await _context.Set<Account>().AnyAsync(a => a.Id == accountId);
        if (!accountExists)
        {
            throw new InvalidOperationException("Tài khoản không tồn tại");
        }

        // Limit: account can create up to 5 shops
        var currentCount = await _context.Set<Shop>().AsNoTracking().CountAsync(s => s.AccountId == accountId);
        if (currentCount >= 5)
        {
            throw new InvalidOperationException("Mỗi tài khoản chỉ được tạo tối đa 5 gian hàng");
        }

        var shop = new Shop
        {
            AccountId = accountId,
            Name = name!,
            Description = description,
            ReportCount = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Set<Shop>().Add(shop);
        await _context.SaveChangesAsync();
        return shop;
    }
// xoa shop
    public async Task<bool> DeleteShopAsync(long shopId, long accountId)
    {
        // Ensure the shop exists and belongs to the account
        var shop = await _context.Set<Shop>()
            .Include(s => s.Products)
                .ThenInclude(p => p.ProductVariants)
            .FirstOrDefaultAsync(s => s.Id == shopId && s.AccountId == accountId);
        if (shop == null)
        {
            return false;
        }

        // Guard: do not allow deletion if any order exists for any variant of this shop
        if (shop.Products != null && shop.Products.Any())
        {
            var variantIds = shop.Products
                .SelectMany(p => p.ProductVariants ?? new List<ProductVariant>())
                .Select(v => v.Id)
                .ToList();
            if (variantIds.Count > 0)
            {
                var hasOrders = await _context.Set<OrderProduct>()
                    .AnyAsync(o => variantIds.Contains(o.ProductVariantId));
                if (hasOrders) return false;
            }
        }

        // Remove child entities first to satisfy FK constraints if cascade isn't configured
        if (shop.Products != null && shop.Products.Any())
        {
            var variants = shop.Products
                .SelectMany(p => p.ProductVariants ?? new List<ProductVariant>())
                .ToList();
            if (variants.Count > 0)
            {
                _context.Set<ProductVariant>().RemoveRange(variants);
            }
            _context.Set<Product>().RemoveRange(shop.Products);
        }

        _context.Set<Shop>().Remove(shop);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Shop?> GetShopByAccountIdAsync(long accountId)
    {
        return await _context.Set<Shop>()
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.AccountId == accountId);
    }

    public async Task<Shop> UpdateShopAsync(long shopId, long accountId, UpdateShopRequest request)
    {
        var shop = await _context.Set<Shop>().FirstOrDefaultAsync(s => s.Id == shopId && s.AccountId == accountId);
        if (shop == null)
        {
            throw new InvalidOperationException("Shop không tồn tại hoặc không thuộc tài khoản này");
        }

        var name = request.Name?.Trim();
        var description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description!.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Tên shop không được để trống");
        }

        shop.Name = name!;
        shop.Description = description;
        shop.UpdatedAt = DateTime.UtcNow;

        _context.Set<Shop>().Update(shop);
        await _context.SaveChangesAsync();
        return shop;
    }

    public async Task<ShopResponse?> GetShopResponseByAccountIdAsync(long accountId)
    {
        var shop = await _context.Set<Shop>()
            .Include(s => s.Products)            
                .ThenInclude(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
            .FirstOrDefaultAsync(s => s.AccountId == accountId);

        if (shop == null) return null;

        var response = new ShopResponse
        {
            Id = shop.Id,
            AccountId = shop.AccountId,
            Name = shop.Name,
            Description = shop.Description,
            ReportCount = shop.ReportCount,
            IsActive = shop.IsActive,
            CreatedAt = shop.CreatedAt,
            UpdatedAt = shop.UpdatedAt,
            TotalProducts = shop.Products?.Count ?? 0
        };
        if (shop.Products != null)
        {
            response.CategoryNames = shop.Products
                .Where(p => p.SubCategory != null && p.SubCategory.Category != null)
                .Select(p => p.SubCategory.Category.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();
        }
        return response;
    }

    // New: list all shops of an account
    public async Task<List<ShopResponse>> GetShopsResponseByAccountIdAsync(long accountId)
    {
        var shops = await _context.Set<Shop>()
            .Include(s => s.Products)
                .ThenInclude(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
            .Where(s => s.AccountId == accountId)
            .ToListAsync();

        return shops.Select(shop =>
        {
            var dto = new ShopResponse
            {
                Id = shop.Id,
                AccountId = shop.AccountId,
                Name = shop.Name,
                Description = shop.Description,
                ReportCount = shop.ReportCount,
                IsActive = shop.IsActive,
                CreatedAt = shop.CreatedAt,
                UpdatedAt = shop.UpdatedAt,
                TotalProducts = shop.Products?.Count ?? 0,
                CategoryNames = (shop.Products ?? new List<Product>())
                    .Where(p => p.SubCategory != null && p.SubCategory.Category != null)
                    .Select(p => p.SubCategory.Category.Name)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList()
            };
            return dto;
        }).ToList();
    }

    // New: get a single shop of account by id
    public async Task<ShopResponse?> GetShopResponseByIdAsync(long shopId, long accountId)
    {
        var shop = await _context.Set<Shop>()
            .Include(s => s.Products)
                .ThenInclude(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
            .FirstOrDefaultAsync(s => s.Id == shopId && s.AccountId == accountId);

        if (shop == null) return null;

        var dto2 = new ShopResponse
        {
            Id = shop.Id,
            AccountId = shop.AccountId,
            Name = shop.Name,
            Description = shop.Description,
            ReportCount = shop.ReportCount,
            IsActive = shop.IsActive,
            CreatedAt = shop.CreatedAt,
            UpdatedAt = shop.UpdatedAt,
            TotalProducts = shop.Products?.Count ?? 0,
            CategoryNames = (shop.Products ?? new List<Product>())
                .Where(p => p.SubCategory != null && p.SubCategory.Category != null)
                .Select(p => p.SubCategory.Category.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList()
        };
        return dto2;
    }

    public async Task<ShopStatisticsResponse?> GetShopStatisticsAsync(long accountId)
    {
        var shop = await _context.Set<Shop>()
            .Include(s => s.Products)
            .ThenInclude(p => p.ProductVariants)
            .ThenInclude(pv => pv.OrderProducts)
            .Include(s => s.Replies)
            .ThenInclude(r => r.Feedback)
            .FirstOrDefaultAsync(s => s.AccountId == accountId);

        if (shop == null) return null;

        var totalProducts = shop.Products?.Count ?? 0;

        // Total products sold
        var totalProductsSold = 0;
        decimal totalRevenue = 0;
        var totalOrders = 0;

        if (shop.Products != null)
        {
            foreach (var product in shop.Products)
            {
                if (product.ProductVariants != null)
                {
                    foreach (var variant in product.ProductVariants)
                    {
                        if (variant.OrderProducts != null)
                        {
                            foreach (var order in variant.OrderProducts.Where(o => o.Status == "CONFIRMED"))
                            {
                                totalProductsSold += order.Quantity;
                                totalRevenue += order.TotalAmount;
                                totalOrders++;
                            }
                        }
                    }
                }
            }
        }

        // Average rating
        var feedbacks = shop.Replies?.Select(r => r.Feedback).Where(f => f != null).ToList() ?? new List<Feedback>();
        var totalFeedbacks = feedbacks.Count;
        var averageRating = totalFeedbacks > 0 ? feedbacks.Average(f => f.Rating) : 0;

        return new ShopStatisticsResponse
        {
            ShopId = shop.Id,
            ShopName = shop.Name,
            TotalProducts = totalProducts,
            TotalProductsSold = totalProductsSold,
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            AverageRating = (decimal)averageRating,
            TotalFeedbacks = totalFeedbacks
        };
    }

    // New: stats for a specific shop of the account
    public async Task<ShopStatisticsResponse?> GetShopStatisticsAsync(long shopId, long accountId)
    {
        var shop = await _context.Set<Shop>()
            .Include(s => s.Products)
            .ThenInclude(p => p.ProductVariants)
            .ThenInclude(pv => pv.OrderProducts)
            .Include(s => s.Replies)
            .ThenInclude(r => r.Feedback)
            .FirstOrDefaultAsync(s => s.Id == shopId && s.AccountId == accountId);

        if (shop == null) return null;

        var totalProducts = shop.Products?.Count ?? 0;

        var totalProductsSold = 0;
        decimal totalRevenue = 0;
        var totalOrders = 0;

        if (shop.Products != null)
        {
            foreach (var product in shop.Products)
            {
                if (product.ProductVariants != null)
                {
                    foreach (var variant in product.ProductVariants)
                    {
                        if (variant.OrderProducts != null)
                        {
                            foreach (var order in variant.OrderProducts.Where(o => o.Status == "CONFIRMED"))
                            {
                                totalProductsSold += order.Quantity;
                                totalRevenue += order.TotalAmount;
                                totalOrders++;
                            }
                        }
                    }
                }
            }
        }

        var feedbacks = shop.Replies?.Select(r => r.Feedback).Where(f => f != null).ToList() ?? new List<Feedback>();
        var totalFeedbacks = feedbacks.Count;
        var averageRating = totalFeedbacks > 0 ? feedbacks.Average(f => f.Rating) : 0;

        return new ShopStatisticsResponse
        {
            ShopId = shop.Id,
            ShopName = shop.Name,
            TotalProducts = totalProducts,
            TotalProductsSold = totalProductsSold,
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            AverageRating = (decimal)averageRating,
            TotalFeedbacks = totalFeedbacks
        };
    }
}
