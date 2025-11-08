using Microsoft.EntityFrameworkCore;
using Mo_DataAccess.Repo;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services;

public class FeedbackServices :GenericRepository<Feedback>, IFeedbackServices
{
    public FeedbackServices(SwpGroup6Context context) : base(context)
    {
    }

    public async Task<List<Feedback>> GetProductFeedbacksAsync(long productId)
    {
        return await _context.Feedbacks
            .Include(f => f.Account)
            .Include(f => f.Replies)
            .Where(f => f.ProductId == productId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<(decimal AverageRating, int TotalCount)> GetProductRatingStatsAsync(long productId)
    {
        var feedbacks = await _context.Feedbacks
            .Where(f => f.ProductId == productId)
            .ToListAsync();

        if (!feedbacks.Any())
        {
            return (0, 0);
        }

        var averageRating = feedbacks.Average(f => f.Rating);
        return ((decimal)averageRating, feedbacks.Count);
    }
}