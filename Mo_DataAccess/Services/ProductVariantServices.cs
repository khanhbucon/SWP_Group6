namespace Mo_DataAccess.Services;

public class ProductVariantServices:GenericRepository<ProductVariant>,IProductVariantServices
{
    public ProductVariantServices(SwpGroup6Context context) : base(context)
    {
    }
}
