namespace Mo_DataAccess.Services;

public class ShopServices  :GenericRepository<Shop>  ,IShopServices
{
    public ShopServices(SwpGroup6Context context) : base(context)
    {
    }
}