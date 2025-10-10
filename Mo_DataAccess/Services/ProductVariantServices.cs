namespace Mo_DataAccess.Services;

public class ProductVariantServices:GenericRepository<ProductVariant>,IProductVariantServices
{
    public ProductVariantServices(AppDbContext context) : base(context)
    {
    }
}