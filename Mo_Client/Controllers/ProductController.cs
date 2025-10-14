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

        var http = new HttpClient { BaseAddress = _api.GetBaseAddress() };
        http.DefaultRequestHeaders.Authorization = _api.GetAuthHeader();
        var apiResp = await http.PostAsJsonAsync("/api/product", request);
        if (apiResp.IsSuccessStatusCode)
        {
            var json = await apiResp.Content.ReadFromJsonAsync<CreateProductEnvelope>();
            if (json?.Id > 0)
            {
                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Details", new { id = json.Id });
            }
        }
        ModelState.AddModelError("", "Không thể thêm sản phẩm");
        return View(request);
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var items = await _api.GetMyProductsAsync();
        return View(items ?? new List<AuthApiClient.ProductSummary>());
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var product = await _api.GetProductAsync(id);
        if (product == null) return RedirectToAction("List");
        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var product = await _api.GetProductAsync(id);
        if (product == null) return RedirectToAction("List");
        var vm = new EditProductVm
        {
            Id = product.Id,
            Name = product.Name,
            ShortDescription = product.Description,
            DetailedDescription = product.Details,
            Fee = product.Fee,
            IsActive = product.IsActive
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProductVm model)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);
        var ok = await _api.UpdateProductAsync(new AuthApiClient.UpdateProductRequest(model.Id, model.Name, model.ShortDescription, model.DetailedDescription, model.Fee, model.IsActive));
        if (ok)
        {
            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("Details", new { id = model.Id });
        }
        ModelState.AddModelError("", "Không thể cập nhật sản phẩm");
        return View(model);
    }

    private record CreateProductEnvelope(bool Success, long Id);

    public class EditProductVm
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? ShortDescription { get; set; }
        public string? DetailedDescription { get; set; }
        public decimal? Fee { get; set; }
        public bool? IsActive { get; set; }
    }
}
