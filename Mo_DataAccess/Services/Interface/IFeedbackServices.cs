using Mo_Entities.Models;
using Mo_Entities.ModelResponse;

namespace Mo_DataAccess.Services.Interface
{
    public interface IFeedbackServices
    {
        // ✅ Đổi kiểu trả về từ Feedback → FeedbackResponse
        Task<IEnumerable<FeedbackResponse>> GetFeedbacksByProductIdAsync(long productId);
        Task<Feedback> AddFeedbackAsync(Feedback feedback);
    }
}
