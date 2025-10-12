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

    public async Task<IActionResult> Index()
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var shop = await _api.GetMyShopAsync();
        ViewBag.HasShop = shop != null;
        return View();
    }

    public async Task<IActionResult> Shops()
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var shop = await _api.GetMyShopAsync();
        return View(shop);
    }

    public IActionResult AddShop() => RedirectToAction("Create", "Shop");
    public IActionResult EditShop() => RedirectToAction("Edit", "Shop");
    public IActionResult ShopStats() => RedirectToAction("Statistics", "Shop");

    public IActionResult Sales() => View();
    public IActionResult ProductOrders() => View();
    public IActionResult ServiceOrders() => View();
    public IActionResult Preorders() => View();
    public IActionResult Reseller() => View();
    public IActionResult Reviews() => View();
    public IActionResult Coupons() => View();
    public IActionResult TopShops() => View();
}
