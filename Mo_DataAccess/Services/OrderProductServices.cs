namespace Mo_DataAccess.Services;

public class OrderProductServices:GenericRepository<OrderProduct>,IOrderProductServices
{
    public OrderProductServices(AppDbContext context) : base(context)
    {
    }
}