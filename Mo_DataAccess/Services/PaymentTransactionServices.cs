using Microsoft.EntityFrameworkCore;
using Mo_Entities.Models;

namespace Mo_DataAccess.Services;

public class PaymentTransactionServices:GenericRepository<PaymentTransaction>,IPaymentTransactionServices
{
    public PaymentTransactionServices(SwpGroup6Context context) : base(context)
    {
    }

    public async Task<PaymentTransaction> CreateDepositTransactionAsync(long userId, decimal amount, string description)
    {
        var paymentTransaction = new PaymentTransaction
        {
            UserId = userId,
            Type = "DEPOSIT",
            Amount = amount,
            PaymentDescription = description,
            CreatedAt = DateTime.UtcNow,
            Status = "PENDING"
        };

        await _context.AddAsync(paymentTransaction);
        return paymentTransaction;
    }

    public async Task<bool> UpdateTransactionStatusAsync(long transactionId, string status)
    {
        var transaction = await GetByIdAsync(transactionId);
        if (transaction == null) return false;

        transaction.Status = status;
        await UpdateAsync(transaction);
        return true;
    }


    public async Task<List<PaymentTransactionVm>> GetUserTransactionsAsync(long userId)
    {
        var transactions = await _context.PaymentTransactions
            .Where(pt => pt.UserId == userId)
            .OrderByDescending(pt => pt.CreatedAt)
            .Select(pt => new PaymentTransactionVm
            {
                Id = pt.Id,
                Type = pt.Type,
                Amount = pt.Amount,
                Status = pt.Status ?? "UNKNOWN",
                CreatedAt = pt.CreatedAt ?? DateTime.UtcNow,
                Description = pt.PaymentDescription
            })
            .ToListAsync();

        return transactions;
    }

}
