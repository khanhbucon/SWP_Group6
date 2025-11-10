using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;
using Mo_Entities.ModelRequest;

namespace Mo_Api.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackServices _feedbackServices;

        public FeedbackController(IFeedbackServices feedbackServices)
        {
            _feedbackServices = feedbackServices;
        }

        // ✅ [POST] - Thêm feedback mới
        [HttpPost]
        public async Task<IActionResult> AddFeedback([FromBody] FeedbackRequest feedbackReq)
        {
            if (feedbackReq == null)
                return BadRequest(new { message = "Dữ liệu feedback không hợp lệ." });

            // Map từ DTO sang Entity
            var feedback = new Feedback
            {
                AccountId = feedbackReq.AccountId,
                ProductId = feedbackReq.ProductId,
                Rating = feedbackReq.Rating,
                Comment = feedbackReq.Comment,
                CreatedAt = DateTime.Now
            };

            // Gọi service thêm feedback
            var created = await _feedbackServices.AddFeedbackAsync(feedback);

            return Ok(new
            {
                message = "Kết nối và thêm feedback thành công!",
                data = created
            });
        }

        // ✅ [GET] - Lấy danh sách feedback theo ProductId
        [HttpGet("{productId}")]
        [AllowAnonymous] // Cho phép public (không cần token)
        public async Task<IActionResult> GetFeedbacksByProductId(long productId)
        {
            var feedbacks = await _feedbackServices.GetFeedbacksByProductIdAsync(productId);

            if (feedbacks == null || !feedbacks.Any())
                return Ok(new
                {
                    message = "Không có feedback nào cho sản phẩm này.",
                    count = 0,
                    data = new List<object>()
                });

            return Ok(new
            {
                message = "Lấy danh sách feedback thành công!",
                count = feedbacks.Count(),
                data = feedbacks
            });
        }
    }
}
