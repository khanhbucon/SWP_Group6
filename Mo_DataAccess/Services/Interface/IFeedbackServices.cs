using Mo_DataAccess.Repo;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services.Interface;

public interface IFeedbackServices :IGenericRepository<Feedback>
{
    Task<List<Feedback>> GetProductFeedbacksAsync(long productId);
    Task<(decimal AverageRating, int TotalCount)> GetProductRatingStatsAsync(long productId);
}