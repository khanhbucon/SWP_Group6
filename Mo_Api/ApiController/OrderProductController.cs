using Microsoft.AspNetCore.Mvc;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;
using Mo_Entities.Models.Request;
using Mo_Entities.Models.Response;

namespace Mo_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderProductController : ControllerBase
    {
        private readonly IOrderProductServices _service;

        public OrderProductController(IOrderProductServices service)
        {
            _service = service;
        }

        // =================== LẤY TẤT CẢ ===================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            var response = result.Select(o => new OrderProductRp
            {
                Id = o.Id,
                AccountId = o.AccountId,
                AccountName = o.Account?.Username,
                ProductVariantId = o.ProductVariantId,
                ProductName = o.ProductVariant?.Name,
                TotalAmount = o.TotalAmount,
                Quantity = o.Quantity,
                Status = o.Status
            });

            return Ok(response);
        }

        // =================== LẤY THEO ID ===================
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null)
                return NotFound(new { message = $"Không tìm thấy đơn hàng ID {id}" });

            var response = new OrderProductRp
            {
                Id = order.Id,
                AccountId = order.AccountId,
                AccountName = order.Account?.Username,
                ProductVariantId = order.ProductVariantId,
                ProductName = order.ProductVariant?.Name,
                TotalAmount = order.TotalAmount,
                Quantity = order.Quantity,
                Status = order.Status
            };

            return Ok(response);
        }

        // =================== TẠO MỚI ===================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderProductRq rq)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var order = new OrderProduct
            {
                AccountId = rq.AccountId,
                ProductVariantId = rq.ProductVariantId,
                TotalAmount = rq.TotalAmount,
                Quantity = rq.Quantity,
                Status = rq.Status
            };

            var created = await _service.CreateAsync(order);
            return Ok(new { message = "Tạo đơn hàng thành công", id = created.Id });
        }

        // =================== XÓA ===================
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null)
                return NotFound(new { message = $"Không tìm thấy đơn hàng ID {id}" });

            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return StatusCode(500, new { message = "Xóa đơn hàng thất bại" });

            return Ok(new { message = "Đã xóa đơn hàng thành công" });
        }
    }
}
