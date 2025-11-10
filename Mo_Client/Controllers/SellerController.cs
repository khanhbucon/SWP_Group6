using Microsoft.AspNetCore.Mvc;
using Mo_Client.Services;

namespace Mo_Client.Controllers;

public class SellerController : Controller
{
    private readonly AuthApiClient _api;
    public SellerController(AuthApiClient api) { _api = api; }

    private bool TrySetApiToken()
    {
        var token = Request.Cookies["accessToken"];
        if (string.IsNullOrWhiteSpace(token)) return false;
        _api.SetToken(token);
        return true;
    }


    // seller  dashboard 
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


    //xoa shop
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






    public async Task<IActionResult> ProductOrders()
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var orders = await _api.GetMyProductOrdersAsync();
        return View(orders ?? new List<AuthApiClient.ProductOrderItem>());
    }
    public IActionResult ServiceOrders() => View();
    public IActionResult Preorders() => View();
    public IActionResult Reseller() => View();
    public IActionResult Reviews() => View();
    public IActionResult Coupons() => View();
    public IActionResult TopShops() => View();
}
