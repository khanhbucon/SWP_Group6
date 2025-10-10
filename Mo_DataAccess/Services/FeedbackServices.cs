using Mo_DataAccess.Repo;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services;

public class FeedbackServices :GenericRepository<Feedback>, IFeedbackServices
{
    public FeedbackServices(AppDbContext context) : base(context)
    {
    }
}