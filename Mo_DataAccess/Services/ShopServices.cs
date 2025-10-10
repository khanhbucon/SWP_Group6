namespace Mo_DataAccess.Services;

public class ShopServices  :GenericRepository<Shop>  ,IShopServices
{
    public ShopServices(AppDbContext context) : base(context)
    {
    }
}