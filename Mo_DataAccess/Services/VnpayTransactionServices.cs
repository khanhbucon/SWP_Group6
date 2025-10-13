namespace Mo_DataAccess.Services;

public class VnpayTransactionServices:GenericRepository<VnpayTransaction>,IVnpayTransactionServices
{
    public VnpayTransactionServices(SwpGroup6Context context) : base(context)
    {
    }
}