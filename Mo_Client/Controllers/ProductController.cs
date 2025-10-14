using Microsoft.AspNetCore.Mvc;
using Mo_Client.Services;

namespace Mo_Client.Controllers;

public class ProductController : Controller
{
    private readonly AuthApiClient _api;
    public ProductController(AuthApiClient api) { _api = api; }

    private bool TrySetApiToken()
    {
        var token = Request.Cookies["accessToken"]; if (string.IsNullOrWhiteSpace(token)) return false; _api.SetToken(token); return true;
    }

    public IActionResult Create(long shopId)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        ViewBag.ShopId = shopId;
        return View(new AuthApiClient.CreateProductRequest(shopId, "", null, null, 0, 0, 0, null, null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AuthApiClient.CreateProductRequest request)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(request);

        var resp = await _api.PostJsonAsync<object>("/api/product", request);
        if (resp.Success)
        {
            TempData["Success"] = "Thêm sản phẩm thành công!";
            return RedirectToAction("Shops", "Seller");
        }
        ModelState.AddModelError("", resp.Message ?? "Không thể thêm sản phẩm");
        return View(request);
    }
}
