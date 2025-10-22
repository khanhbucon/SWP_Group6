using Mo_Entities.ModelRequest;
using Mo_Entities.ModelResponse;

namespace Mo_DataAccess.Services.Interface;

public interface IVnpayTransactionServices:IGenericRepository<VnpayTransaction>
{
    Task<VnpayDepositResponse> CreateDepositRequestAsync(long userId, VnpayDepositRequest request);
    Task<bool> VerifyDepositAsync(string transactionId, decimal amount);
    Task<bool> ProcessDepositAsync(long userId, decimal amount, string transactionId);
}