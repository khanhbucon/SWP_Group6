using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.ModelRequest;
using Mo_Api.Extensions;

namespace Mo_Api.ApiController;

[Route("api/[controller]")]
[ApiController]
public class ShopController : ControllerBase
{
    private readonly IShopServices _shopServices;

    public ShopController(IShopServices shopServices)
    {
        _shopServices = shopServices;
    }

    // Diagnostic endpoint to help verify linkage
    [HttpGet("diagnostics")]
    [Authorize]
    public async Task<IActionResult> Diagnostics()
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new { Success = false, Message = "Invalid token" });
        }

        var shop = await _shopServices.GetShopResponseByAccountIdAsync(userId.Value);
        if (shop == null)
        {
            return Ok(new { Success = true, Message = "No shop found for this account", AccountId = userId.Value });
        }

        return Ok(new { Success = true, Message = "Shop linked", AccountId = userId.Value, Shop = shop });
    }

    [HttpPost("create")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> CreateShop([FromBody] CreateShopRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Success = false, Message = "Invalid token" });
            }

            var shop = await _shopServices.CreateShopAsync(userId.Value, request);
            return Ok(new { Success = true, Message = "Tạo shop thành công", ShopId = shop.Id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi tạo shop" });
        }
    }

    // Backward compatible: return the first shop if exists
    [HttpGet("my-shop")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetMyShop()
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Success = false, Message = "Invalid token" });
            }

            var shop = await _shopServices.GetShopResponseByAccountIdAsync(userId.Value);
            if (shop == null)
            {
                return NotFound(new { Success = false, Message = "Bạn chưa tạo shop" });
            }

            return Ok(new { Success = true, Data = shop });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi lấy thông tin shop" });
        }
    }

    // New: list all shops of current seller
    [HttpGet("my-shops")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetMyShops()
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Success = false, Message = "Invalid token" });
            }

            var shops = await _shopServices.GetShopsResponseByAccountIdAsync(userId.Value);
            return Ok(new { Success = true, Data = shops });
        }
        catch
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi lấy thông tin shops" });
        }
    }

    [HttpPut("update/{shopId}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> UpdateShop(long shopId, [FromBody] UpdateShopRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Success = false, Message = "Invalid token" });
            }

            var shop = await _shopServices.UpdateShopAsync(shopId, userId.Value, request);
            return Ok(new { Success = true, Message = "Cập nhật shop thành công" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi cập nhật shop" });
        }
    }

    // Delete shop by id
    [HttpDelete("{shopId:long}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> DeleteShop(long shopId)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Success = false, Message = "Invalid token" });
            }

            var ok = await _shopServices.DeleteShopAsync(shopId, userId.Value);
            if (!ok)
            {
                return BadRequest(new { Success = false, Message = "Không thể xóa shop. Shop không thuộc tài khoản hoặc đã phát sinh đơn hàng." });
            }
            return Ok(new { Success = true, Message = "Xóa shop thành công" });
        }
        catch
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi xóa shop" });
        }
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetShopStatistics()
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Success = false, Message = "Invalid token" });
            }

            var stats = await _shopServices.GetShopStatisticsAsync(userId.Value);
            if (stats == null)
            {
                return NotFound(new { Success = false, Message = "Bạn chưa tạo shop" });
            }

            return Ok(new { Success = true, Data = stats });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi lấy thống kê shop" });
        }
    }

    // New: statistics by shop id
    [HttpGet("{shopId:long}/statistics")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetShopStatisticsById(long shopId)
    {
        try
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { Success = false, Message = "Invalid token" });
            }

            var stats = await _shopServices.GetShopStatisticsAsync(shopId, userId.Value);
            if (stats == null)
            {
                return NotFound(new { Success = false, Message = "Shop không tồn tại hoặc không thuộc tài khoản" });
            }

            return Ok(new { Success = true, Data = stats });
        }
        catch
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi lấy thống kê shop" });
        }
    }
}
