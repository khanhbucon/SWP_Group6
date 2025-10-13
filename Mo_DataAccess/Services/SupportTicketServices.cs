namespace Mo_DataAccess.Services;

public class SupportTicketServices:GenericRepository<SupportTicket>,ISupportTicketServices
{
    public SupportTicketServices(SwpGroup6Context context) : base(context)
    {
    }
}
