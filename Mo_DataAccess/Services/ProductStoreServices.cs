namespace Mo_DataAccess.Services;

public class ProductStoreServices : GenericRepository<ProductStore>, IProductStoreServices
{
    public ProductStoreServices(SwpGroup6Context context) : base(context)
    {
    }
}