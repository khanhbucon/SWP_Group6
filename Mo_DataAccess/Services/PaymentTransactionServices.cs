namespace Mo_DataAccess.Services;

public class PaymentTransactionServices:GenericRepository<PaymentTransaction>,IPaymentTransactionServices
{
    public PaymentTransactionServices(AppDbContext context) : base(context)
    {
    }
}