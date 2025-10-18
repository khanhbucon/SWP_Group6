using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mo_Client.Services;
using Mo_Entities.ModelRequest;
using Mo_Entities.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Mo_Client.Controllers
{
    public class OrderProductController : Controller
    {
        private readonly SwpGroup6Context _context;
        private readonly OrderProductApiClient _apiClient;

        // 🔹 Không dùng DI — tự tạo thủ công DbContext & ApiClient
        public OrderProductController()
        {
            // 👉 Tạo thủ công DbContext với connection string đầy đủ
            var optionsBuilder = new DbContextOptionsBuilder<SwpGroup6Context>();
            optionsBuilder.UseSqlServer("server=ADMIN-PC;database=SWP_Group6;uid=sa;pwd=123123;TrustServerCertificate=true");

            _context = new SwpGroup6Context(optionsBuilder.Options);
            _apiClient = new OrderProductApiClient();
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.ProductVariants = _context.ProductVariants.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderProductRequest model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProductVariants = _context.ProductVariants.ToList();
                return View(model);
            }

            model.Status ??= "PENDING";
            bool result = await _apiClient.CreateOrderAsync(model);

            if (result)
            {
                TempData["Success"] = "✅ Tạo đơn hàng thành công!";
                return RedirectToAction("Create");
            }

            TempData["Error"] = "❌ Tạo đơn thất bại, vui lòng thử lại.";
            ViewBag.ProductVariants = _context.ProductVariants.ToList();
            return View(model);
        }
    }
}
