namespace Mo_DataAccess.Services;

public class ReplyServices:GenericRepository<Reply>,IReplyServices
{
    public ReplyServices(AppDbContext context) : base(context)
    {
    }
}