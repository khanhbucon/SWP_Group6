namespace Mo_DataAccess.Services;

public class ProductStoreServices : GenericRepository<ProductStore>, IProductStoreServices
{
    public ProductStoreServices(AppDbContext context) : base(context)
    {
    }
}