using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.ModelResponse;
using Mo_Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Mo_Api.ApiController;

[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IAccountServices _accountServices;
    private readonly IShopServices _shopServices;
    private readonly IProductServices _productServices;
    private readonly SwpGroup6Context _context;

    public AdminController(
        IAccountServices accountServices, 
        IShopServices shopServices, 
        IProductServices productServices,
        SwpGroup6Context context)
    {
        _accountServices = accountServices;
        _shopServices = shopServices;
        _productServices = productServices;
        _context = context;
    }

    [HttpGet("dashboard-stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            // Lấy tổng số người dùng
            var totalUsers = await _context.Accounts.CountAsync();

            // Lấy tổng số cửa hàng
            var totalShops = await _context.Shops.CountAsync();

            // Lấy tổng số sản phẩm
            var totalProducts = await _context.Products.CountAsync();

            // Lấy số cửa hàng chờ duyệt (giả sử có status field)
            var pendingShops = await _context.Shops
                .Where(s => s.IsActive == false)
                .CountAsync();

            // Lấy số sản phẩm chờ duyệt (giả sử có status field)
            var pendingProducts = await _context.Products
                .Where(p => p.IsActive == false)
                .CountAsync();

            // Lấy số người dùng bị ban (giả sử có IsActive field)
            var bannedUsers = await _context.Accounts
                .Where(a => a.IsActive == false)
                .CountAsync();

          
            // Lấy 5 người dùng mới nhất
            var recentUsers = await _context.Accounts
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new RecentUserResponse
                {
                    UserId = a.Id,
                    Username = a.Username,
                    Email = a.Email,
                    CreatedAt = a.CreatedAt ?? DateTime.Now,
                    IsActive = a.IsActive ?? true
                })
                .ToListAsync();

            var stats = new DashboardStatsResponse
            {
                TotalUsers = totalUsers,
                TotalShops = totalShops,
                TotalProducts = totalProducts,
                PendingShops = pendingShops,
                PendingProducts = pendingProducts,
                BannedUsers = bannedUsers,
                RecentUsers = recentUsers,
            };

            return Ok(new { Success = true, Data = stats });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi lấy thống kê dashboard" });
        }
    }
}
