using Microsoft.EntityFrameworkCore;
using Mo_Entities.Models;
using Mo_Entities.ModelResponse;
using Mo_Client.Models;

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
            Type = "NapTien",
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


    private static string GetTypeDisplay(string type) => type switch
    {
        "NapTien" => "Nạp tiền",
        "MuaHang" => "Mua hàng",
        "RutTien" => "Rút tiền",
        "BanHang" => "Bán hàng",
        "HoaHong" => "Hoa hồng",
        "ChiaSe" => "Chia sẻ",
        _ => type
    };

    private static string GetStatusDisplay(string status) => status switch
    {
        "COMPLETED" => "Thành công",
        "PENDING" => "Đang xử lý",
        "FAILED" => "Thất bại",
        "CANCELLED" => "Đã hủy",
        _ => status
    };

    private static bool IsIncomeType(string type) => type switch
    {
        "NapTien" => true,
        "BanHang" => true,
        "HoaHong" => true,
        _ => false
    };

    private static bool IsExpenseType(string type) => type switch
    {
        "MuaHang" => true,
        "RutTien" => true,
        "ChiaSe" => true,
        _ => false
    };

    public async Task<TransactionHistoryListResponse> GetUserTransactionHistoryAsync(long userId)
    {
        var query = _context.PaymentTransactions
            .Where(pt => pt.UserId == userId)
            .OrderByDescending(pt => pt.CreatedAt);

        var totalCount = await query.CountAsync();
        var transactions = await query.ToListAsync();
        
        var transactionResponses = transactions.Select(pt => new TransactionHistoryResponse
        {
            Id = pt.Id,
            UserId = pt.UserId,
            Type = pt.Type,
            Amount = pt.Amount,
            Description = pt.PaymentDescription,
            CreatedAt = pt.CreatedAt,
            Status = pt.Status,
            TypeDisplay = GetTypeDisplay(pt.Type),
            StatusDisplay = GetStatusDisplay(pt.Status),
            AmountDisplay = $"{pt.Amount:N0} VNĐ",
            IsIncome = IsIncomeType(pt.Type),
            IsExpense = IsExpenseType(pt.Type)
        }).ToList();

        var totalIncome = await _context.PaymentTransactions
            .Where(pt => pt.UserId == userId && (pt.Type == "NapTien" || pt.Type == "BanHang" || pt.Type == "HoaHong"))
            .SumAsync(pt => pt.Amount);

        var totalExpense = await _context.PaymentTransactions
            .Where(pt => pt.UserId == userId && (pt.Type == "MuaHang" || pt.Type == "RutTien" || pt.Type == "ChiaSe"))
            .SumAsync(pt => pt.Amount);

        return new TransactionHistoryListResponse
        {
            Transactions = transactionResponses,
            TotalCount = totalCount,
            PageNumber = 1,
            PageSize = totalCount,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetAmount = totalIncome - totalExpense
        };
    }

}
