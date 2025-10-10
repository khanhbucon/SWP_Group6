namespace Mo_DataAccess.Services;

public class SupportTicketServices:GenericRepository<SupportTicket>,ISupportTicketServices
{
    public SupportTicketServices(AppDbContext context) : base(context)
    {
    }
}