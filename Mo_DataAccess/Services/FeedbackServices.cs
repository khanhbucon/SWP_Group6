using Microsoft.EntityFrameworkCore;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;
using Mo_Entities.ModelResponse;

namespace Mo_DataAccess.Services
{
    public class FeedbackServices : IFeedbackServices
    {
        private readonly SwpGroup6Context _context;

        public FeedbackServices(SwpGroup6Context context)
        {
            _context = context;
        }

        // ✅ GET: lấy danh sách feedback theo ProductId (trả về DTO)
        public async Task<IEnumerable<FeedbackResponse>> GetFeedbacksByProductIdAsync(long productId)
        {
            return await _context.Feedbacks
                .Include(f => f.Account)
                .Where(f => f.ProductId == productId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FeedbackResponse
                {
                    Id = f.Id,
                    AccountId = f.AccountId,
                    AccountName = f.Account.Username ?? f.Account.Username, // tuỳ theo cột bạn có
                    ProductId = f.ProductId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();
        }

        // ✅ POST: thêm feedback mới
        public async Task<Feedback> AddFeedbackAsync(Feedback feedback)
        {
            feedback.CreatedAt = DateTime.Now;
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }
    }
}
