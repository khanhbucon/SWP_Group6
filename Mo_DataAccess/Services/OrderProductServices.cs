namespace Mo_DataAccess.Services;

public class OrderProductServices:GenericRepository<OrderProduct>,IOrderProductServices
{
    public OrderProductServices(SwpGroup6Context context) : base(context)
    {
    }
}
