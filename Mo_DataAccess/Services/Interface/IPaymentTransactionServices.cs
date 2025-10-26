using Mo_Entities.Models;
using Mo_Entities.ModelResponse;
using Mo_Client.Models;

namespace Mo_DataAccess.Services.Interface;

public interface IPaymentTransactionServices :IGenericRepository<PaymentTransaction>
{
    Task<PaymentTransaction> CreateDepositTransactionAsync(long userId, decimal amount, string description);
    Task<bool> UpdateTransactionStatusAsync(long transactionId, string status);
    Task<List<PaymentTransactionVm>> GetUserTransactionsAsync(long userId);
    Task<TransactionHistoryListResponse> GetUserTransactionHistoryAsync(long userId);
}