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

    // Public shop listing for guests
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 12)
    {
        try
        {
            var http = new HttpClient();
            var apiUrl = _authApiClient.GetBaseAddress().ToString().TrimEnd('/');
            var response = await http.GetAsync($"{apiUrl}/api/shop/GetAllShops?page={page}&pageSize={pageSize}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ShopListApiResponse>();
                if (result?.Success == true && result.Data != null)
                {
                    ViewBag.CurrentPage = result.Pagination?.CurrentPage ?? page;
                    ViewBag.TotalPages = result.Pagination?.TotalPages ?? 1;
                    ViewBag.PageSize = pageSize;
                    return View(result.Data);
                }
            }
            
            return View(new List<ShopListItem>());
        }
        catch
        {
            return View(new List<ShopListItem>());
        }
    }

    public class ShopListItem
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ProductCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string OwnerName { get; set; } = string.Empty;
    }

    public class ShopListApiResponse
    {
        public bool Success { get; set; }
        public List<ShopListItem>? Data { get; set; }
        public PaginationInfo? Pagination { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }

    // GET: /Shop/Create
    public async Task<IActionResult> Create()
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        // Allow up to 5 shops
        var shops = await _authApiClient.GetMyShopsAsync();
        if (shops != null && shops.Count >= 5)
        {
            TempData["Error"] = "Bạn đã đạt giới hạn 5 gian hàng";
            return RedirectToAction("Shops", "Seller");
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
            TempData["Success"] = "Tạo shop thành công! Shop của bạn đang chờ admin duyệt.";
            return RedirectToAction("Shops", "Seller");
        }
        else
        {
            // Nếu user đã có shop, chuyển sang View Shop
            var msg = (message ?? string.Empty).ToLowerInvariant();
            if (!string.IsNullOrEmpty(msg) && (msg.Contains("co shop") || msg.Contains("có shop") || msg.Contains("đã có shop") || msg.Contains("has a shop") || msg.Contains("already has") || msg.Contains("exists")))
            {
                TempData["Success"] = "Bạn đã có shop. Chuyển đến trang trang xem shop.";
                return RedirectToAction("Shops", "Seller");
            }

            ModelState.AddModelError("", message ?? "Có lỗi xảy ra khi tạo shop.");
            return View(request);
        }
    }

    // GET: /Shop/Index
    public async Task<IActionResult> Index()
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var shops = await _authApiClient.GetMyShopsAsync();
        if (shops == null || shops.Count == 0)
        {
            return RedirectToAction("Create");
    }

        // For now reuse old view which expects a single shop: show the first
        return View(shops.First());
    }

    // GET: /Shop/Edit
    public async Task<IActionResult> Edit(long? shopId)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var shop = shopId.HasValue
            ? (await _authApiClient.GetMyShopsAsync())?.FirstOrDefault(s => s.Id == shopId.Value)
            : await _authApiClient.GetMyShopAsync();
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

    // POST: /Shop/ToggleActive
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(long shopId, bool active)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var (success, message) = await _authApiClient.ToggleShopActiveAsync(shopId, active);
        if (success)
        {
            TempData["Success"] = active ? "Đã bật hoạt động" : "Đã tạm dừng hoạt động";
        }
        else
        {
            TempData["Error"] = message ?? "Không thể thay đổi trạng thái (có thể shop đang chờ duyệt).";
        }
        // Redirect back to seller shops list so user remains on /Seller/Shops
        return RedirectToAction("Shops", "Seller");
    }

    // GET: /Shop/Statistics
    public async Task<IActionResult> Statistics(long? shopId)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var stats = shopId.HasValue
            ? await _authApiClient.GetShopStatisticsByIdAsync(shopId.Value)
            : await _authApiClient.GetShopStatisticsAsync();
        if (stats == null)
        {
            TempData["Error"] = "Bạn chưa tạo shop.";
            return RedirectToAction("Create");
        }

        return View(stats);
    }
}
