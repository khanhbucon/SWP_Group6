using Microsoft.AspNetCore.Mvc;
using Mo_Client.Services;

namespace Mo_Client.Controllers;

public class SellerController : Controller
{
    private readonly AuthApiClient _api;
    private readonly OrderService _orderService;
    public SellerController(AuthApiClient api, OrderService orderService) { _api = api; _orderService = orderService; }

    private bool TrySetApiToken()
    {
        var token = Request.Cookies["accessToken"];
        if (string.IsNullOrWhiteSpace(token)) return false;
        _api.SetToken(token);
        return true;
    }

    public async Task<IActionResult> Index()
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var shops = await _api.GetMyShopsAsync();
        var profile = await _api.GetCurrentUserProfileAsync();
        ViewBag.HasShop = (shops?.Any() ?? false);
        var vm = new Mo_Client.Models.SellerDashboardViewModel
        {
            Profile = profile,
            Stats = await _api.GetShopStatisticsAsync()
        };
        return View(vm);
    }

    public async Task<IActionResult> Shops()
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var shops = await _api.GetMyShopsAsync();

        // ✅ THÊM 3 DÒNG NÀY
        var token = Request.Cookies["accessToken"];
        ViewBag.AccessToken = token;
        ViewBag.ApiBase = _api.GetBaseAddress()?.ToString()?.TrimEnd('/');

        return View(shops ?? new List<AuthApiClient.ShopResponse>());
    }

    public IActionResult AddShop() => RedirectToAction("Create", "Shop");
    public IActionResult EditShop(long shopId) => RedirectToAction("Edit", "Shop", new { shopId });
    public IActionResult ShopStats(long shopId) => RedirectToAction("Statistics", "Shop", new { shopId });
    public IActionResult ViewShop(long shopId) => RedirectToAction("Details", "Shop", new { shopId });

    public async Task<IActionResult> Sales()
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var profile = await _api.GetCurrentUserProfileAsync();
        var stats = await _api.GetShopStatisticsAsync();
        var vm = new Mo_Client.Models.SellerDashboardViewModel { Profile = profile, Stats = stats };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteShop(long shopId)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var (success, message) = await _api.DeleteShopAsync(shopId);
        if (success)
            TempData["Success"] = "Xóa shop thành công";
        else
            TempData["Error"] = string.IsNullOrWhiteSpace(message) ? "Không thể xóa shop" : message;
        return RedirectToAction("Shops");
    }

    // List product orders for seller
    public async Task<IActionResult> ProductOrders(string? status = null)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var resp = await _orderService.GetSellerOrdersAsync(status);
        var list = resp?.Orders ?? new List<Mo_Entities.ModelResponse.OrderHistoryResponse>();
        ViewBag.StatusFilter = status;
        ViewBag.TotalOrders = resp?.TotalOrders ?? 0;
        ViewBag.TotalRevenue = resp?.TotalSpent ?? 0;
        ViewBag.PendingOrders = resp?.PendingOrders ?? 0;
        ViewBag.ConfirmedOrders = resp?.ConfirmedOrders ?? 0;
        ViewBag.CompletedOrders = resp?.CompletedOrders ?? 0;
        ViewBag.CancelledOrders = resp?.CancelledOrders ?? 0;
        return View(list);
    }

    // New: load order detail partial for modal
    [HttpGet]
    public async Task<IActionResult> ProductOrderDetails(long orderId)
    {
        if (!TrySetApiToken()) return Unauthorized();

        var detail = await _orderService.GetSellerOrderDetailAsync(orderId);
        if (detail == null)
        {
            return NotFound();
        }

        return PartialView("_OrderDetailsPartial", detail);
    }

    public IActionResult ServiceOrders() => View();
    public IActionResult Preorders() => View();
    public IActionResult Reseller() => View();
    public IActionResult Reviews() => View();
    public IActionResult Coupons() => View();
    public IActionResult TopShops() => View();
}