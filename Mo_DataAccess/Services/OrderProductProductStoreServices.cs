namespace Mo_DataAccess.Services;

public class OrderProductProductStoreServices:GenericRepository<OrderProductProductStore>,IOrderProductProductStoreServices
{
    public OrderProductProductStoreServices(AppDbContext context) : base(context)
    {
    }
}