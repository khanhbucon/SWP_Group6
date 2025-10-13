namespace Mo_DataAccess.Services;

public class ReplyServices:GenericRepository<Reply>,IReplyServices
{
    public ReplyServices(SwpGroup6Context context) : base(context)
    {
    }
}
