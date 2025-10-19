using Microsoft.AspNetCore.Mvc;
using Mo_Entities.Models;
using Mo_Entities.Models.Request;
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

        // ======================= DANH SÁCH =======================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MoApi");
                var response = await client.GetAsync("api/OrderProduct");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không thể tải danh sách đơn hàng từ server.";
                    return View(new List<OrderProduct>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<OrderProduct>>(json) ?? new List<OrderProduct>();

                if (data.Count == 0)
                {
                    ViewBag.Message = "Không có đơn hàng nào để hiển thị.";
                }

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi kết nối API: " + ex.Message;
                return View(new List<OrderProduct>());
            }
        }

        // ======================= FORM TẠO =======================
        [HttpGet]
        public IActionResult Create()
        {
            // Gửi sẵn model mặc định
            return View(new OrderProductRq { Status = "PENDING" });
        }

        // ======================= TẠO ĐƠN =======================
        [HttpPost]
        //[ValidateAntiForgeryToken]
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

        // ======================= CHI TIẾT =======================
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
                var order = JsonConvert.DeserializeObject<OrderProduct>(json);

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
        //[ValidateAntiForgeryToken]
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
