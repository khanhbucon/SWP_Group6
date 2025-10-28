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

        [HttpPost]
        public async Task<IActionResult> AddFeedback([FromBody] FeedbackRequest feedbackReq)
        {
            if (feedbackReq == null)
                return BadRequest("Invalid feedback data.");

            // ✅ Map từ DTO sang Entity
            var feedback = new Feedback
            {
                AccountId = feedbackReq.AccountId,
                ProductId = feedbackReq.ProductId,
                Rating = feedbackReq.Rating,
                Comment = feedbackReq.Comment,
                CreatedAt = DateTime.Now
            };

            var created = await _feedbackServices.AddFeedbackAsync(feedback);

            return Ok(new
            {
                message = "Kết nối và thêm feedback thành công!",
                data = created
            });
        }
        // ✅ [GET] - lấy danh sách feedback theo productId
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetFeedbacksByProductId(long productId)
        {
            var feedbacks = await _feedbackServices.GetFeedbacksByProductIdAsync(productId);

            if (feedbacks == null || !feedbacks.Any())
                return NotFound("Không có feedback nào cho sản phẩm này.");

            return Ok(new
            {
                message = "Lấy danh sách feedback thành công!",
                count = feedbacks.Count(),
                data = feedbacks
            });
        }
    }
}
