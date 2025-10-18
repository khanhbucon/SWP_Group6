using Microsoft.AspNetCore.Mvc;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;
using Mo_Entities.ModelRequest;

namespace Mo_Api.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderProductController : ControllerBase
    {
        private readonly IOrderProductServices _orderService;

        public OrderProductController(IOrderProductServices orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        // 🟢 API: Fake tạo đơn hàng (không lưu DB)
        [HttpPost("Create")]
        public IActionResult Create([FromBody] OrderProductRequest model)
        {
            if (model == null)
                return BadRequest(new { message = "Dữ liệu trống!" });

            var random = new Random();

            // 🧩 Nếu không nhập hoặc nhập sai thì tạo ngẫu nhiên
            if (model.AccountId <= 0)
                model.AccountId = random.Next(1000, 9999);

            if (model.ProductVariantId <= 0)
                model.ProductVariantId = random.Next(1000, 9999);

            // 🧩 Giả lập đơn hàng mới (không ghi vào DB)
            var fakeOrder = new OrderProduct
            {
                Id = random.Next(1, 999999),
                AccountId = model.AccountId,
                ProductVariantId = model.ProductVariantId,
                Quantity = model.Quantity,
                TotalAmount = model.TotalAmount,
                Status = model.Status ?? "PENDING"
            };

            // ✅ Trả phản hồi thành công như thật
            return Ok(new
            {
                message = "Fake đơn hàng tạo thành công (không lưu DB)",
                data = fakeOrder
            });
        }

        // 🟢 API: Lấy đơn hàng theo AccountId
        [HttpGet("ByAccount/{accountId:long}")]
        public async Task<IActionResult> GetByAccount(long accountId)
        {
            try
            {
                var result = await _orderService.GetOrdersByAccountIdAsync(accountId);
                return result != null && result.Any()
                    ? Ok(result)
                    : NotFound(new { message = "Không tìm thấy đơn hàng." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server.", error = ex.Message });
            }
        }

        // 🟢 API: Cập nhật trạng thái đơn
        [HttpPut("UpdateStatus/{orderId:long}")]
        public async Task<IActionResult> UpdateStatus(long orderId, [FromBody] string newStatus)
        {
            if (string.IsNullOrEmpty(newStatus))
            {
                return BadRequest(new { message = "Trạng thái không được để trống." });
            }

            try
            {
                await _orderService.UpdateStatusAsync(orderId, newStatus);
                return Ok(new { message = "Cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server.", error = ex.Message });
            }
        }
    }
}
