namespace Mo_DataAccess.Services;

public class ProductServices :GenericRepository<Product>,IProductServices
{
    public ProductServices(AppDbContext context) : base(context)
    {
    }
}