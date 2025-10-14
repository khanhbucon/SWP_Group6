using Microsoft.AspNetCore.Authorization;
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

        // Map: short -> description, detailed -> details, optional fee and image
        var product = new Product
        {
            ShopId = request.ShopId,
            SubCategoryId = request.SubCategoryId,
            Name = request.Name,
            Description = request.ShortDescription,
            Details = request.DetailedDescription,
            Fee = request.Fee,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // Convert base64 image (if provided) into byte[]
        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            try
            {
                var base64 = request.ImageUrl;
                // Strip data URL prefix if present
                var commaIdx = base64.IndexOf(',');
                if (base64.StartsWith("data:") && commaIdx > -1)
                {
                    base64 = base64[(commaIdx + 1)..];
                }
                product.Image = Convert.FromBase64String(base64);
            }
            catch
            {
                // ignore image parse errors; keep null
            }
        }

        await _products.CreateAsync(product);

        // default variant (price/stock)
        await _variants.CreateAsync(new ProductVariant
        {
            ProductId = product.Id,
            Name = "Default",
            Price = request.Price,
            Stock = request.Stock,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        return Ok(new { Success = true, Id = product.Id });
    }
}
