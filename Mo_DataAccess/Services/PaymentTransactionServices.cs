namespace Mo_DataAccess.Services;

public class PaymentTransactionServices:GenericRepository<PaymentTransaction>,IPaymentTransactionServices
{
    public PaymentTransactionServices(SwpGroup6Context context) : base(context)
    {
    }
}