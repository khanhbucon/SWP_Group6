namespace Mo_DataAccess.Services;

public class VnpayTransactionServices:GenericRepository<VnpayTransaction>,IVnpayTransactionServices
{
    public VnpayTransactionServices(AppDbContext context) : base(context)
    {
    }
}