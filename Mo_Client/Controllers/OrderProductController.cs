using Microsoft.AspNetCore.Mvc;
using Mo_Entities.Models.Request;
using Mo_Entities.Models.Response;
using Newtonsoft.Json;
using System.Text;



namespace Mo_Client.Controllers
{
    public class OrderProductController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderProductController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // ======================= DANH SÁCH (CÓ TÌM KIẾM) =======================
        [HttpGet]
        public async Task<IActionResult> Index(string? keyword)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MoApi");
                var response = await client.GetAsync("api/OrderProduct");

                if (!response.IsSuccessStatusCode)
                {
                    // Read API error body to show helpful message for debugging
                    var apiError = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Không thể tải danh sách đơn hàng từ server. API response: {apiError}";
                    return View(new List<OrderProductRp>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<OrderProductRp>>(json) ?? new List<OrderProductRp>();

                // ✅ Lọc theo từ khóa (theo tên sản phẩm hoặc tài khoản)
                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.Trim().ToLower();
                    data = data.Where(o =>
                        (!string.IsNullOrEmpty(o.ProductName) && o.ProductName.ToLower().Contains(keyword)) ||
                        (!string.IsNullOrEmpty(o.AccountName) && o.AccountName.ToLower().Contains(keyword))
                    ).ToList();
                }

                ViewBag.Keyword = keyword;
                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi kết nối API: " + ex.Message;
                return View(new List<OrderProductRp>());
            }
        }

        // ======================= FORM TẠO =======================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new OrderProductRq { Status = "PENDING" });
        }

        // ======================= TẠO MỚI =======================
        [HttpPost]
        public async Task<IActionResult> Create(OrderProductRq rq)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin hợp lệ.";
                return View(rq);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("MoApi");
                var json = JsonConvert.SerializeObject(rq);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/OrderProduct", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "✅ Tạo đơn hàng thành công!";
                    return RedirectToAction(nameof(Index));
                }

                var apiError = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"❌ Tạo đơn hàng thất bại! (API: {apiError})";
                return View(rq);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi tạo đơn hàng: " + ex.Message;
                return View(rq);
            }
        }

        // ======================= XEM CHI TIẾT =======================
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "ID đơn hàng không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var client = _httpClientFactory.CreateClient("MoApi");
                var response = await client.GetAsync($"api/OrderProduct/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng cần xem chi tiết.";
                    return RedirectToAction(nameof(Index));
                }

                var json = await response.Content.ReadAsStringAsync();
                var order = JsonConvert.DeserializeObject<OrderProductRp>(json);

                if (order == null)
                {
                    TempData["Error"] = "Không thể đọc dữ liệu đơn hàng từ API.";
                    return RedirectToAction(nameof(Index));
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi tải chi tiết đơn hàng: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ======================= XÓA =======================
        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "ID đơn hàng không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var client = _httpClientFactory.CreateClient("MoApi");
                var response = await client.DeleteAsync($"api/OrderProduct/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "🗑️ Xóa đơn hàng thành công!";
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"❌ Xóa đơn hàng thất bại! (API: {err})";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa đơn hàng: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}