using Microsoft.AspNetCore.Mvc;
using Mo_Entities.Models;
using System.Net.Http.Json;

namespace Mo_Client.Controllers
{
    public class ProductController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public ProductController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // ✅ Hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            var client = _clientFactory.CreateClient("MoApi");

            // ⚠️ Chú ý: API thật là "api/Products" (có chữ s)
            var response = await client.GetAsync("api/Products");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Không thể tải danh sách sản phẩm.";
                return View(new List<Product>());
            }

            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            return View(products);
        }
    }
}
