using ClosedXML.Excel; // Added for Excel parsing/template
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo_Client.Services;
using ClosedXML.Excel; // Added for Excel parsing/template

namespace Mo_Client.Controllers;

public class ProductController : Controller
{
    private readonly AuthApiClient _api;
    public ProductController(AuthApiClient api) { _api = api; }

    private bool TrySetApiToken()
    {
        var token = Request.Cookies["accessToken"]; if (string.IsNullOrWhiteSpace(token)) return false; _api.SetToken(token); return true;
    }

    // Public product listing for guests
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 12, long? categoryId = null, long? subCategoryId = null)
    {
        try
        {
            var http = new HttpClient();
            var apiUrl = _api.GetBaseAddress().ToString().TrimEnd('/');
            
            // Build URL with optional filters
            var url = $"{apiUrl}/api/product/GetAllProducts?page={page}&pageSize={pageSize}";
            if (subCategoryId.HasValue)
            {
                url += $"&subCategoryId={subCategoryId.Value}";
            }
            else if (categoryId.HasValue)
            {
                url += $"&categoryId={categoryId.Value}";
            }
            
            var response = await http.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ProductListApiResponse>();
                if (result?.Success == true && result.Data != null)
                {
                    ViewBag.CurrentPage = result.Pagination?.CurrentPage ?? page;
                    ViewBag.TotalPages = result.Pagination?.TotalPages ?? 1;
                    ViewBag.PageSize = pageSize;
                    ViewBag.CategoryId = categoryId;
                    ViewBag.SubCategoryId = subCategoryId;
                    return View(result.Data);
                }
            }
            
            return View(new List<ProductListItem>());
        }
        catch
        {
            return View(new List<ProductListItem>());
        }
    }

    public class ProductListItem
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public long ShopId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string SubCategoryName { get; set; } = string.Empty;
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class ProductListApiResponse
    {
        public bool Success { get; set; }
        public List<ProductListItem>? Data { get; set; }
        public PaginationInfo? Pagination { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }

    public IActionResult Create(long shopId)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        ViewBag.ShopId = shopId;
        // Load categories for cascading dropdown
        var categoriesTask = _api.GetCategoriesAsync();
        categoriesTask.Wait();
        ViewBag.Categories = categoriesTask.Result ?? new List<AuthApiClient.IdName>();
        return View(new AuthApiClient.CreateProductRequest(shopId, "", null, null, 0, 0, 0, null, null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AuthApiClient.CreateProductRequest request)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid)
        {
            // Re-populate categories if validation fails
            ViewBag.ShopId = request.ShopId;
            ViewBag.Categories = await _api.GetCategoriesAsync() ?? new List<AuthApiClient.IdName>();
            return View(request);
        }

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
        // reload categories on error
        ViewBag.ShopId = request.ShopId;
        ViewBag.Categories = await _api.GetCategoriesAsync() ?? new List<AuthApiClient.IdName>();
        return View(request);
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");

        //  VALIDATE PAGE NUMBER - Đảm bảo page >= 1
        if (page < 1) page = 1;

        var paged = await _api.GetMyProductsPagedAsync(search, page, pageSize);
        if (paged == null) return View(new List<AuthApiClient.ProductSummary>());

        // CHECK IF PAGE EXCEEDS TOTAL PAGES - Redirect về trang 1 nếu vượt quá
        if (page > paged.TotalPages && paged.TotalPages > 0)
        {
            return RedirectToAction("List", new { search, page = 1, pageSize });
        }

        ViewBag.Page = paged.Page;
        ViewBag.PageSize = paged.PageSize;
        ViewBag.Total = paged.Total;
        ViewBag.TotalPages = paged.TotalPages;
        ViewBag.ApiBase = _api.GetBaseAddress()?.ToString()?.TrimEnd('/');

        var token = Request.Cookies["accessToken"];
        ViewBag.AccessToken = token;

        return View(paged.Items);
    }

    [HttpGet]
    [AllowAnonymous] 
    public async Task<IActionResult> Details(long id)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var product = await _api.GetProductAsync(id);
        if (product == null) return RedirectToAction("List");
        var variants = await _api.GetVariantsAsync(id) ?? new List<AuthApiClient.VariantDto>();
        ViewBag.Variants = variants;
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
            IsActive = product.IsActive,
            IsPending = false,
            CurrentImageUrl = _api.GetBaseAddress()?.ToString()?.TrimEnd('/') + $"/api/product/{product.Id}/image"
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProductVm model)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);

        // fetch current product to determine pending state
        var current = await _api.GetProductAsync(model.Id);
        if (current == null) return RedirectToAction("List");
        var isPending = current.IsActive == null;

        // if pending, never send IsActive change
        var request = new AuthApiClient.UpdateProductRequest(
            model.Id,
            model.Name,
            model.ShortDescription,
            model.DetailedDescription,
            model.Fee,
            isPending ? null : model.IsActive
        );

        // attach image fields via dynamic to include properties not in the record
        var body = new
        {
            request.Id,
            request.Name,
            ShortDescription = request.ShortDescription,
            DetailedDescription = request.DetailedDescription,
            request.Fee,
            request.IsActive,
            ImageUrl = model.NewImageBase64,
            RemoveImage = model.RemoveImage
        };

        var http = new HttpClient { BaseAddress = _api.GetBaseAddress() };
        http.DefaultRequestHeaders.Authorization = _api.GetAuthHeader();
        var resp = await http.PutAsJsonAsync("/api/product", body);
        if (resp.IsSuccessStatusCode)
        {
            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("Details", new { id = model.Id });
        }
        ModelState.AddModelError("", "Không thể cập nhật sản phẩm");
        return View(model);
    }
    //xóa sản phẩm nếu sản phẩm thuộc về tài khoản và chưa có đơn hàng nào
    //•
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var result = await _api.DeleteProductAsync(id);
        if (result.Success)
        {
            TempData["Success"] = "Đã xoá sản phẩm";
        }
        else
        {
            TempData["Error"] = result.Message ?? "Không thể xoá sản phẩm (không thuộc quyền sở hữu hoặc đã có đơn hàng)";
        }
        return RedirectToAction("List");
    }

    // Add a variant to existing product (same productId, different name)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVariant(long productId, string name, decimal price, int stock)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        if (productId <= 0 || string.IsNullOrWhiteSpace(name) || price < 0 || stock < 0)
        {
            TempData["Error"] = "Dữ liệu biến thể không hợp lệ";
            return RedirectToAction("Details", new { id = productId });
        }
        var (success, message) = await _api.CreateVariantAsync(productId, name.Trim(), price, stock);
        TempData[success ? "Success" : "Error"] = success ? "Đã thêm biến thể" : (message ?? "Không thể thêm biến thể");
        return RedirectToAction("Details", new { id = productId });
    }

    // =============== BULK UPLOAD FROM TXT/EXCEL (Create multiple products) ===============
    [HttpGet]
    public async Task<IActionResult> BulkUpload()
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var shops = await _api.GetMyShopsAsync();
        ViewBag.Shops = shops ?? new List<AuthApiClient.ShopResponse>();
        return View();
    }

    public class LineResult
    {
        public int Index { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkUpload(long shopId, long subCategoryId, decimal price, int stock, string name, decimal? fee, string? variantName, IFormFile file)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var shops = await _api.GetMyShopsAsync();
        ViewBag.Shops = shops ?? new List<AuthApiClient.ShopResponse>();
        ViewBag.SelectedShopId = shopId;

        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn file .txt hoặc .xlsx";
            return View();
        }
        var results = new List<LineResult>();
        int total = 0, ok = 0, fail = 0;

        try
        {
            IEnumerable<string> linesEnumerable;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext == ".xlsx")
            {
                linesEnumerable = ReadLinesFromExcel(file);
            }
            else
            {
                linesEnumerable = await ReadLinesFromTxtAsync(file);
            }

            foreach (var trimmed in linesEnumerable)
            {
                total++;
                var req = new AuthApiClient.CreateProductRequest(
                    shopId,
                    name,
                    // Save one line to both short and detailed descriptions
                    trimmed,
                    trimmed,
                    subCategoryId,
                    price,
                    stock,
                    null,
                    fee,
                    string.IsNullOrWhiteSpace(variantName) ? "Default" : variantName
                );

                var http = new HttpClient { BaseAddress = _api.GetBaseAddress() };
                http.DefaultRequestHeaders.Authorization = _api.GetAuthHeader();
                var resp = await http.PostAsJsonAsync("/api/product", req);
                if (resp.IsSuccessStatusCode)
                {
                    ok++; results.Add(new LineResult { Index = total, Content = trimmed, Success = true });
                }
                else
                {
                    fail++; var reason = resp.ReasonPhrase ?? "Bad Request";
                    try
                    {
                        var json = await resp.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(json)) reason = json.Length > 120 ? json[..120] + "..." : json;
                    }
                    catch { }
                    results.Add(new LineResult { Index = total, Content = trimmed, Success = false, Error = reason });
                }
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi khi đọc file: " + ex.Message;
            return View();
        }

        ViewBag.Results = results;
        ViewBag.Summary = $"TOTAL:{total} | SUCCESS:{ok} | ERROR:{fail}";
        return View();
    }

    private static async Task<List<string>> ReadLinesFromTxtAsync(IFormFile file)
    {
        var lines = new List<string>();
        using var sr = new StreamReader(file.OpenReadStream());
        while (!sr.EndOfStream)
        {
            var l = (await sr.ReadLineAsync())?.Trim();
            if (!string.IsNullOrWhiteSpace(l)) lines.Add(l!);
        }
        return lines;
    }

    private static List<string> ReadLinesFromExcel(IFormFile file)
    {
        var lines = new List<string>();
        using var wb = new XLWorkbook(file.OpenReadStream());
        var ws = wb.Worksheets.First();
        var firstRow = ws.FirstRowUsed()?.RowNumber() ?? 1;
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? ws.LastRowUsed()?.RowNumber() ?? 0;
        if (lastRow == 0) return lines;

        // Identify header in column A if present
        var headerVal = ws.Cell(firstRow, 1).GetString().Trim();
        bool hasHeader = string.Equals(headerVal, "value", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(headerVal, "serial", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(headerVal, "account", StringComparison.OrdinalIgnoreCase);
        var start = hasHeader ? firstRow + 1 : firstRow;
        for (int r = start; r <= lastRow; r++)
        {
            var v = ws.Cell(r, 1).GetString()?.Trim();
            if (!string.IsNullOrWhiteSpace(v)) lines.Add(v);
        }
        return lines;
    }

    // =============== BULK UPLOAD FOR SPECIFIC PRODUCT (e.g., add accounts to one product) ===============
    [HttpGet]
    public async Task<IActionResult> BulkUploadForProduct(long productId)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var product = await _api.GetProductAsync(productId);
        if (product == null)
        {
            TempData["Error"] = "Sản phẩm không tồn tại";
            return RedirectToAction("List");
        }
        ViewBag.Product = product;
        var variants = await _api.GetVariantsAsync(productId) ?? new List<AuthApiClient.VariantDto>();
        ViewBag.Variants = variants;
        return View();
    }

    [HttpGet]
    public IActionResult DownloadSerialTemplate()
    {
        // Provide an Excel template with header "Value" and a few sample rows
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Serials");
        ws.Cell(1, 1).Value = "Value";
        ws.Cell(2, 1).Value = "username|password";
        ws.Cell(3, 1).Value = "serial-key-1";
        ws.Cell(4, 1).Value = "serial-key-2";
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "add_product_template.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkUploadForProduct(long productId, long variantId, IFormFile file, bool ignoreDuplicates = true)
    {
        if (!TrySetApiToken()) return RedirectToAction("Login", "Account");
        var product = await _api.GetProductAsync(productId);
        if (product == null)
        {
            TempData["Error"] = "Sản phẩm không tồn tại";
            return RedirectToAction("List");
        }
        ViewBag.Product = product;
        var variants = await _api.GetVariantsAsync(productId) ?? new List<AuthApiClient.VariantDto>();
        ViewBag.Variants = variants;

        if (variantId <= 0)
        {
            TempData["Error"] = "Vui lòng chọn biến thể";
            return View();
        }
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn file .xlsx (hoặc .txt)";
            return View();
        }

        // Validate file size (max 10MB)
        if (file.Length > 10 * 1024 * 1024)
        {
            TempData["Error"] = "File quá lớn. Vui lòng chọn file nhỏ hơn 10MB";
            return View();
        }

        var lines = new List<string>();
        try
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext == ".xlsx")
            {
                lines = ReadLinesFromExcel(file);
            }
            else
            {
                lines = await ReadLinesFromTxtAsync(file);
            }

            // Validate number of lines (max 10000)
            if (lines.Count > 10000)
            {
                TempData["Error"] = "File có quá nhiều dòng. Vui lòng chia nhỏ file (tối đa 10,000 dòng)";
                return View();
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Lỗi khi đọc file: {ex.Message}";
            return View();
        }

        AuthApiClient.ImportResponse? result = null;
        try
        {
            Console.WriteLine($"Starting import: ProductId={productId}, VariantId={variantId}, Lines={lines.Count}");
            result = await _api.ImportToVariantAsync(productId, variantId, lines, ignoreDuplicates);
            Console.WriteLine($"Import result: Success={result?.Success}, Summary={result?.Summary}");
            
            if (result == null || !result.Success)
            {
                TempData["Error"] = result?.Summary ?? "Import thất bại";
                return View();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Import exception: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            TempData["Error"] = $"Lỗi khi import: {ex.Message}";
            return View();
        }

        ViewBag.Summary = result.Summary;
        ViewBag.Results = result.Results.Select(r => new LineResult
        {
            Index = r.Index,
            Content = r.Content,
            Success = r.Success,
            Error = r.Error
        }).ToList();

        TempData["Success"] = "Import thành công";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> SubCategories(long categoryId)
    {
        if (!TrySetApiToken()) return Unauthorized();
        var subs = await _api.GetSubCategoriesAsync(categoryId) ?? new List<AuthApiClient.IdName>();
        return Json(new { success = true, data = subs });
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
        public bool IsPending { get; set; }
        public string? CurrentImageUrl { get; set; }
        public string? NewImageBase64 { get; set; }
        public bool RemoveImage { get; set; }
    }

    // Action công khai cho khách xem sản phẩm 
    // Đổi tên action thành View thay vì PublicDetails
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> View(long id)  // ĐỔI TÊN THÀNH View
    {
        try
        {
            var token = Request.Cookies["accessToken"];
            var isLoggedIn = !string.IsNullOrWhiteSpace(token);

            var http = new HttpClient();
            var apiUrl = _api.GetBaseAddress().ToString().TrimEnd('/');
            var response = await http.GetAsync($"{apiUrl}/api/Product/details/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm";
                return RedirectToAction("Index");
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var result = await response.Content.ReadFromJsonAsync<PublicProductDetailResponse>(options);
            if (result?.Success == true && result.Data != null)
            {
                ViewBag.IsLoggedIn = isLoggedIn;
                ViewBag.ApiBaseUrl = apiUrl + "/api"; // Pass API base URL to view
                return View("PublicDetails", result.Data);  // View name vẫn giữ nguyên
            }

            TempData["Error"] = "Không thể tải thông tin sản phẩm";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Lỗi: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    // Helper classes cho PublicDetails
    public class PublicProductDetailResponse
    {
        public bool Success { get; set; }
        public PublicProductDetailData? Data { get; set; }
    }

    public class PublicProductDetailData
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Details { get; set; }
        public string? Image { get; set; }
        public string? ShopName { get; set; }
        public long ShopId { get; set; }
        public string? CategoryName { get; set; }
        public string? SubCategoryName { get; set; }
        public long? SubCategoryId { get; set; }
        public decimal Fee { get; set; }
        public int TotalStock { get; set; }
        public int TotalSold { get; set; }
        public decimal AverageRating { get; set; } = 0;
        public int TotalFeedbacks { get; set; } = 0;
        public List<PublicVariantInfo> Variants { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class PublicVariantInfo
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? Stock { get; set; }
    }
    
}
