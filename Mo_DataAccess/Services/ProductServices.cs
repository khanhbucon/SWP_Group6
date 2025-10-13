namespace Mo_DataAccess.Services;

public class ProductServices :GenericRepository<Product>,IProductServices
{
    public ProductServices(SwpGroup6Context context) : base(context)
    {
    }
}