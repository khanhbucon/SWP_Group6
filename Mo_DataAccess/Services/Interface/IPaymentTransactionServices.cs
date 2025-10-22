namespace Mo_DataAccess.Services.Interface;

public interface IPaymentTransactionServices :IGenericRepository<PaymentTransaction>
{
    Task<PaymentTransaction> CreateDepositTransactionAsync(long userId, decimal amount, string description);
    Task<bool> UpdateTransactionStatusAsync(long transactionId, string status);
}