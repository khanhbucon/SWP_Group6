using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Mo_Api.Extensions;
using Mo_DataAccess.Services;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.ModelRequest;
using Mo_Entities.ModelResponse;
using Mo_Entities.Models;
using static Mo_Client.Controllers.ProductController;

namespace Mo_Api.ApiController;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductServices _products;
    private readonly IProductVariantServices _variants;
    private readonly IShopServices _shops;
    private readonly IProductStoreServices _stores;
    private readonly IFeedbackServices _feedbacks;
    private readonly SwpGroup6Context _db;

    public ProductController(IProductServices products, IProductVariantServices variants, IShopServices shops, IProductStoreServices stores, IFeedbackServices feedbacks, SwpGroup6Context db)
    {
        _products = products;
        _variants = variants;
        _shops = shops;
        _stores = stores;
        _feedbacks = feedbacks;
        _db = db;
    }

    [HttpGet("my")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetMyProducts([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        if (page < 1) page = 1;
        if (pageSize <= 0 || pageSize > 100) pageSize = 10;

        var list = await _products.GetBySellerAccountIdAsync(userId.Value);
        var query = list.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => (p.Name != null && p.Name.ToLower().Contains(term)) ||
                                     (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        var total = query.Count();
        var items = query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = items.Select(p => new
        {
            p.Id,
            p.Name,
            p.Description,
            p.Details,
            p.ShopId,
            ShopName = p.Shop.Name,
            p.CreatedAt,
            p.UpdatedAt,
            p.IsActive
        }).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        return Ok(new { Success = true, Data = result, Total = total, Page = page, PageSize = pageSize, TotalPages = totalPages });
    }

    [HttpGet("{id:long}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetById(long id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product == null) return NotFound(new { Success = false, Message = "Product not found" });
        // Get total stock and sold
        var (totalStock, totalSold) = await _products.GetStockAndSoldAsync(id);
        // Get min and max price        
        var (minPrice, maxPrice) = await _products.GetPriceRangeAsync(id);
        return Ok(new
        {
            Success = true,
            Data = new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Details,
                product.Fee,
                product.SubCategoryId,
                product.ShopId,
                product.CreatedAt,
                product.UpdatedAt,
                product.IsActive,
                TotalStock = totalStock,
                TotalSold = totalSold,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            }
        });
    }

    [HttpGet("{id:long}/image")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImage(long id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product == null) return NotFound();

        if (product.Image == null || product.Image.Length == 0)
            return NotFound();

        // Best-effort mime type detection (PNG header) else default jpeg
        string contentType = (product.Image.Length > 8 &&
                              product.Image[0] == 0x89 && product.Image[1] == 0x50 && product.Image[2] == 0x4E && product.Image[3] == 0x47)
            ? "image/png"
            : "image/jpeg";
        return File(product.Image, contentType);
    }

    [HttpGet("{productId:long}/variants")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetVariants(long productId)
    {
        // Verify ownership
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();
        var product = await _products.GetByIdAsync(productId);
        if (product == null) return NotFound(new { Success = false, Message = "Product not found" });
        var shop = await _shops.GetByIdAsync(product.ShopId);
        if (shop == null || shop.AccountId != userId.Value) return Forbid();

        var variants = await _db.ProductVariants.Where(v => v.ProductId == productId)
            .OrderBy(v => v.Price)
            .Select(v => new { v.Id, v.Name, v.Price, v.Stock })
            .ToListAsync();
        return Ok(new { Success = true, Data = variants });
    }

    public class BulkImportRequest
    {
        public List<string> Lines { get; set; } = new();
        public bool IgnoreDuplicates { get; set; } = true;
    }

    public class LineResult
    {
        public int Index { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    [HttpPost("{productId:long}/import/{variantId:long}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> ImportToVariant(long productId, long variantId, [FromBody] BulkImportRequest request)
    {
        Console.WriteLine($"API ImportToVariant called: ProductId={productId}, VariantId={variantId}, Lines={request?.Lines?.Count ?? 0}");
        
        if (request?.Lines == null || request.Lines.Count == 0)
            return BadRequest(new { Success = false, Message = "No data provided" });

        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();
        
        Console.WriteLine($"User ID: {userId.Value}");

        // Validate product ownership and variant relation
        var product = await _products.GetByIdAsync(productId);
        if (product == null) return NotFound(new { Success = false, Message = "Product not found" });
        var shop = await _shops.GetByIdAsync(product.ShopId);
        if (shop == null || shop.AccountId != userId.Value) return Forbid();

        var variant = await _db.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId);
        if (variant == null) return NotFound(new { Success = false, Message = "Variant not found for product" });

        var results = new List<LineResult>();
        int ok = 0, fail = 0, idx = 0;

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // Set timeout for large operations
            _db.Database.SetCommandTimeout(300); // 5 minutes
            
            foreach (var raw in request.Lines)
            {
                idx++;
                var line = raw?.Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    fail++; results.Add(new LineResult { Index = idx, Content = raw ?? string.Empty, Success = false, Error = "Empty line" });
                    continue;
                }

                // duplicate check by value within same variant
                var exists = await _db.ProductStores.AnyAsync(s => s.ProductVariantId == variantId && s.Value == line);
                if (exists && !request.IgnoreDuplicates)
                {
                    fail++; results.Add(new LineResult { Index = idx, Content = line, Success = false, Error = "Duplicate" });
                    continue;
                }
                if (!exists)
                {
                    var store = new ProductStore
                    {
                        ProductVariantId = variantId,
                        Content = $"{product.Name} {variant.Name}",
                        Value = line,
                        Status = "AVAILABLE",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _db.ProductStores.Add(store);
                    ok++; results.Add(new LineResult { Index = idx, Content = line, Success = true });
                }
                else
                {
                    // ignored duplicate silently counted as fail
                    fail++; results.Add(new LineResult { Index = idx, Content = line, Success = false, Error = "Duplicate (ignored)" });
                }
            }

            // Update stock by number of newly inserted rows
            if (ok > 0)
            {
                variant.Stock = (variant.Stock ?? 0) + ok;
                _db.ProductVariants.Update(variant);
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            try
            {
                await tx.RollbackAsync();
            }
            catch (Exception rollbackEx)
            {
                // Log rollback exception but don't throw
                Console.WriteLine($"Rollback failed: {rollbackEx.Message}");
            }
            
            // Log the original exception
            Console.WriteLine($"Import failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            return StatusCode(500, new { Success = false, Message = $"Import failed: {ex.Message}" });
        }

        var total = ok + fail;
        return Ok(new { Success = true, Summary = $"TOTAL:{total} | SUCCESS:{ok} | ERROR:{fail}", Results = results });
    }

    [HttpPost]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        // Ensure shop belongs to current seller
        var shop = await _shops.GetByIdAsync(request.ShopId);
        if (shop == null || shop.AccountId != userId.Value)
            return Forbid();

        // Create product with pending status (IsActive = null)
        var product = new Product
        {
            ShopId = request.ShopId,
            SubCategoryId = request.SubCategoryId,
            Name = request.Name,
            Description = request.ShortDescription,
            Details = request.DetailedDescription,
            Fee = request.Fee ?? 5m, // default 5% if not provided
            IsActive = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // Convert base64 image (if provided) into byte[]
        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            try
            {
                var base64 = request.ImageUrl;
                var commaIdx = base64.IndexOf(',');
                if (base64.StartsWith("data:") && commaIdx > -1)
                {
                    base64 = base64[(commaIdx + 1)..];
                }
                product.Image = Convert.FromBase64String(base64);
            }
            catch { }
        }

        await _products.CreateAsync(product);
        // ensure stays Pending in case DB default flips it
        product.IsActive = null;
        await _products.UpdateAsync(product);

        // default variant (price/stock)
        await _variants.CreateAsync(new ProductVariant
        {
            ProductId = product.Id,
            Name = string.IsNullOrWhiteSpace(request.VariantName) ? "Default" : request.VariantName!,
            Price = request.Price,
            Stock = request.Stock,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        return Ok(new { Success = true, Id = product.Id });
    }

    [HttpPut]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Update([FromBody] UpdateProductRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var product = await _products.GetByIdAsync(request.Id);
        if (product == null) return NotFound(new { Success = false, Message = "Product not found" });

        if (!string.IsNullOrWhiteSpace(request.Name)) product.Name = request.Name;
        if (request.ShortDescription != null) product.Description = request.ShortDescription;
        if (request.DetailedDescription != null) product.Details = request.DetailedDescription;
        if (request.Fee.HasValue) product.Fee = request.Fee;

        // Harden rule: Seller cannot change status while pending (IsActive == null)
        if (request.IsActive.HasValue)
        {
            product.IsActive = request.IsActive;
        }

        // Image update
        if (request.RemoveImage == true)
        {
            product.Image = null;
        }
        else if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            try
            {
                var base64 = request.ImageUrl;
                var commaIdx = base64.IndexOf(',');
                if (base64.StartsWith("data:") && commaIdx > -1)
                {
                    base64 = base64[(commaIdx + 1)..];
                }
                product.Image = Convert.FromBase64String(base64);
            }
            catch
            {
                return BadRequest(new { Success = false, Message = "Ảnh không hợp lệ (không phải base64)" });
            }
            product.IsActive = request.IsActive;
        }

        product.UpdatedAt = DateTime.UtcNow;
        await _products.UpdateAsync(product);
        return Ok(new { Success = true });
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Delete(long id)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var product = await _products.GetByIdAsync(id);
        if (product == null)
            return NotFound(new { Success = false, Message = "Sản phẩm không tồn tại" });
        if (product.ShopId == 0)
            return BadRequest(new { Success = false, Message = "Sản phẩm không hợp lệ" });

        var shop = await _shops.GetByIdAsync(product.ShopId);
        if (shop == null || shop.AccountId != userId.Value)
            return Forbid();

        var ok = await _products.DeleteIfOwnedAsync(id, userId.Value);
        if (!ok)
            return BadRequest(new { Success = false, Message = "Không thể xoá sản phẩm (có thể sản phẩm đã phát sinh đơn hàng)" });
        return Ok(new { Success = true });
    }

    // ADMIN endpoints
    [HttpGet("admin/list")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminList([FromQuery] string? search)
    {
        var list = await _products.AdminListAsync(search);
        return Ok(new { Success = true, Data = list });
    }

    [HttpPost("admin/{productId:long}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminApprove(long productId)
    {
        var ok = await _products.AdminApproveAsync(productId);
        if (!ok) return NotFound(new { Success = false, Message = "Sản phẩm không tồn tại" });
        return Ok(new { Success = true, Message = "Duyệt sản phẩm thành công" });
    }

    [HttpPost("admin/{productId:long}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminActivate(long productId)
    {
        var ok = await _products.AdminActivateAsync(productId);
        if (!ok) return NotFound(new { Success = false, Message = "Sản phẩm không tồn tại" });
        return Ok(new { Success = true, Message = "Kích hoạt sản phẩm thành công" });
    }

    [HttpPost("admin/{productId:long}/suspend")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminSuspend(long productId)
    {
        var ok = await _products.AdminSuspendAsync(productId);
        if (!ok) return NotFound(new { Success = false, Message = "Sản phẩm không tồn tại" });
        return Ok(new { Success = true, Message = "Tạm dừng sản phẩm thành công" });
    }

    // New: create a variant for an existing product
    public record CreateVariantRequest(string Name, decimal Price, int Stock);

    [HttpPost("{productId:long}/variants")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> CreateVariant(long productId, [FromBody] CreateVariantRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || req.Price < 0 || req.Stock < 0)
            return BadRequest(new { Success = false, Message = "Dữ liệu biến thể không hợp lệ" });

        var userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var product = await _products.GetByIdAsync(productId);
        if (product == null) return NotFound(new { Success = false, Message = "Product not found" });
        var shop = await _shops.GetByIdAsync(product.ShopId);
        if (shop == null || shop.AccountId != userId.Value) return Forbid();

        // Unique name within product
        var exists = await _db.ProductVariants.AnyAsync(v => v.ProductId == productId && v.Name.ToLower() == req.Name.Trim().ToLower());
        if (exists) return BadRequest(new { Success = false, Message = "Tên biến thể đã tồn tại trong sản phẩm" });

        var variant = new ProductVariant
        {
            ProductId = productId,
            Name = req.Name.Trim(),
            Price = req.Price,
            Stock = req.Stock,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _variants.CreateAsync(variant);
        return Ok(new { Success = true, Id = variant.Id });
    }

    [HttpGet("GetAllProducts")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 12, [FromQuery] long? categoryId = null, [FromQuery] long? subCategoryId = null)
    {
        try
        {
            var products = await _products.GetAllProductsAsync();

            // Filter only active products with valid data
            var activeProducts = products
                .Where(p => p.IsActive == true 
                    && p.Shop != null 
                    && p.SubCategory != null 
                    && p.SubCategory.Category != null 
                    && p.ProductVariants != null 
                    && p.ProductVariants.Any())
                .ToList();

            // Filter by subCategoryId (takes priority)
            if (subCategoryId.HasValue)
            {
                activeProducts = activeProducts
                    .Where(p => p.SubCategoryId == subCategoryId.Value)
                    .ToList();
            }
            // Otherwise filter by categoryId if provided
            else if (categoryId.HasValue)
            {
                activeProducts = activeProducts
                    .Where(p => p.SubCategory.CategoryId == categoryId.Value)
                    .ToList();
            }

            // Pagination
            var totalItems = activeProducts.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var pagedProducts = activeProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Image = p.Image != null ? Convert.ToBase64String(p.Image) : null,
                    ShopName = p.Shop.Name,
                    ShopId = p.ShopId,
                    CategoryName = p.SubCategory.Category.Name,
                    SubCategoryName = p.SubCategory.Name,
                    MinPrice = p.ProductVariants.Min(v => v.Price),
                    MaxPrice = p.ProductVariants.Max(v => v.Price),
                    CreatedAt = p.CreatedAt
                })
                .ToList();

            return Ok(new
            {
                Success = true,
                Data = pagedProducts,
                Pagination = new
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalItems = totalItems
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("details/{id:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductDetails(long id)
    {
        try
        {
            // Load product with all necessary navigation properties
            var product = await _db.Products
                .Include(p => p.Shop)
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null || product.IsActive != true)
                return NotFound(new { Success = false, Message = "Sản phẩm không tồn tại hoặc chưa được kích hoạt" });

            // Get variants
            var variants = await _db.ProductVariants
                .Where(v => v.ProductId == id)
                .OrderBy(v => v.Price)
                .Select(v => new
                {
                    v.Id,
                    v.Name,
                    v.Price,
                    v.Stock
                })
                .ToListAsync();

            // Get total stock and sold
            var (totalStock, totalSold) = await _products.GetStockAndSoldAsync(id);

            // Get rating stats
            var (averageRating, totalFeedbacks) = await _feedbacks.GetProductRatingStatsAsync(id);

            return Ok(new
            {
                Success = true,
                Data = new
                {
                    product.Id,
                    product.Name,
                    product.Description,
                    product.Details,
                    Image = product.Image != null ? Convert.ToBase64String(product.Image) : null,
                    ShopName = product.Shop != null ? product.Shop.Name : null,
                    ShopId = product.ShopId,
                    CategoryName = product.SubCategory != null && product.SubCategory.Category != null ? product.SubCategory.Category.Name : null,
                    SubCategoryName = product.SubCategory != null ? product.SubCategory.Name : null,
                    SubCategoryId = product.SubCategoryId,
                    product.Fee,
                    TotalStock = totalStock,
                    TotalSold = totalSold,
                    AverageRating = averageRating,
                    TotalFeedbacks = totalFeedbacks,
                    Variants = variants,
                    product.CreatedAt,
                    product.UpdatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("{productId:long}/feedbacks")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductFeedbacks(long productId)
    {
        try
        {
            var feedbacks = await _feedbacks.GetProductFeedbacksAsync(productId);

            var feedbackList = feedbacks.Select(f => new
            {
                f.Id,
                f.Rating,
                f.Comment,
                f.CreatedAt,
                UserName = f.Account?.Username ?? "Người dùng ẩn danh",
                UserEmail = f.Account?.Email,
                Replies = (f.Replies ?? new List<Reply>()).Select(r => new
                {
                    r.Id,
                    Comment = r.Comment,
                    r.CreatedAt,
                    ShopName = r.Shop?.Name ?? "Cửa hàng"
                }).ToList()
            }).ToList();

            return Ok(new { Success = true, Data = feedbackList });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }
   
}
