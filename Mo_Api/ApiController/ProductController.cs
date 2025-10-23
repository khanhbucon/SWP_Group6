using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Mo_Api.Extensions;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.ModelRequest;
using Mo_Entities.Models;

namespace Mo_Api.ApiController;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductServices _products;
    private readonly IProductVariantServices _variants;
    private readonly IShopServices _shops;

    public ProductController(IProductServices products, IProductVariantServices variants, IShopServices shops)
    {
        _products = products;
        _variants = variants;
        _shops = shops;
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
            Fee = request.Fee,
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
            if (product.IsActive == null)
            {
                return BadRequest(new { Success = false, Message = "Sản phẩm đang chờ duyệt, không thể thay đổi trạng thái" });
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
}
