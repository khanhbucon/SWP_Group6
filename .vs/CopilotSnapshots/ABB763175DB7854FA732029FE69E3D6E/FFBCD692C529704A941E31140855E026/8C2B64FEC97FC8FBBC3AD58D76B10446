using Microsoft.AspNetCore.Mvc;
using Mo_Client.Services;

namespace Mo_Client.Controllers;

public class ShopController : Controller
{
    private readonly AuthApiClient _authApiClient;

    public ShopController(AuthApiClient authApiClient)
    {
        _authApiClient = authApiClient;
    }

    private bool TrySetApiToken()
    {
        var token = HttpContext.Request.Cookies["accessToken"];
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }
        _authApiClient.SetToken(token);
        return true;
    }

    // GET: /Shop/Create
    public IActionResult Create()
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        // If user already has a shop, redirect to View Shop
        var shopTask = _authApiClient.GetMyShopAsync();
        shopTask.Wait();
        var existed = shopTask.Result;
        if (existed != null)
        {
            TempData["Success"] = "Bạn đã có shop. Chuyển tới trang Shop.";
            return RedirectToAction("Index");
        }
        return View();
    }

    // POST: /Shop/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AuthApiClient.CreateShopRequest request)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var (success, message) = await _authApiClient.CreateShopAsync(request);
        if (success)
        {
            TempData["Success"] = "Tạo shop thành công!";
            return RedirectToAction("Index", "Home");
        }
        else
        {
            // Nếu user đã có shop, chuyển sang View Shop
            var msg = (message ?? string.Empty).ToLowerInvariant();
            if (!string.IsNullOrEmpty(msg) && (msg.Contains("co shop") || msg.Contains("có shop") || msg.Contains("đã có shop") || msg.Contains("has a shop") || msg.Contains("already has") || msg.Contains("exists")))
            {
                TempData["Success"] = "Bạn đã có shop. Chuyển sang trang xem shop.";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", message ?? "Có lỗi xảy ra khi tạo shop.");
            return View(request);
        }
    }

    // GET: /Shop/Index
    public async Task<IActionResult> Index()
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var shop = await _authApiClient.GetMyShopAsync();
        if (shop == null)
        {
            return RedirectToAction("Create");
        }

        return View(shop);
    }

    // GET: /Shop/Edit
    public async Task<IActionResult> Edit()
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var shop = await _authApiClient.GetMyShopAsync();
        if (shop == null)
        {
            return RedirectToAction("Create");
        }

        var request = new AuthApiClient.UpdateShopRequest(shop.Name, shop.Description);
        ViewBag.ShopId = shop.Id;
        return View(request);
    }

    // POST: /Shop/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long shopId, AuthApiClient.UpdateShopRequest request)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid)
        {
            ViewBag.ShopId = shopId;
            return View(request);
        }

        var success = await _authApiClient.UpdateShopAsync(shopId, request);
        if (success)
        {
            TempData["Success"] = "Cập nhật shop thành công!";
            return RedirectToAction("Index");
        }
        else
        {
            ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật shop.");
            ViewBag.ShopId = shopId;
            return View(request);
        }
    }

    // GET: /Shop/Statistics
    public async Task<IActionResult> Statistics()
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var stats = await _authApiClient.GetShopStatisticsAsync();
        if (stats == null)
        {
            TempData["Error"] = "Bạn chưa tạo shop.";
            return RedirectToAction("Create");
        }

        return View(stats);
    }
}