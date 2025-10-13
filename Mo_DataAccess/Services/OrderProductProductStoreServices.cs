namespace Mo_DataAccess.Services;

public class OrderProductProductStoreServices:GenericRepository<OrderProductProductStore>,IOrderProductProductStoreServices
{
    public OrderProductProductStoreServices(SwpGroup6Context context) : base(context)
    {
    }
}
