using Microsoft.EntityFrameworkCore;
using Mo_Entities.Models;
using Mo_Entities.ModelResponse;
using Mo_Client.Models;
using Mo_DataAccess.Services.Interface;

namespace Mo_DataAccess.Services;

public class PaymentTransactionServices:GenericRepository<PaymentTransaction>,IPaymentTransactionServices
{
    private readonly IAccountServices _accountServices;

    public PaymentTransactionServices(SwpGroup6Context context, IAccountServices accountServices) : base(context)
    {
        _accountServices = accountServices;
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

    public async Task<PaymentTransaction> CreateWithdrawTransactionAsync(long userId, decimal amount, string description)
    {
        var paymentTransaction = new PaymentTransaction
        {
            UserId = userId,
            Type = "RutTien",
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

    public async Task<(bool Success, string Message)> UpdateWithdrawTransactionStatusAsync(long transactionId, string newStatus)
    {
        try
        {
            // Get transaction
            var transaction = await GetByIdAsync(transactionId);
            if (transaction == null)
            {
                return (false, "Giao dịch không tồn tại.");
            }

            // Validate transaction type
            if (!string.Equals(transaction.Type, "RutTien", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Chỉ có thể cập nhật trạng thái cho giao dịch rút tiền.");
            }

            // Validate status
            if (!string.Equals(newStatus, "COMPLETED", StringComparison.OrdinalIgnoreCase) && 
                !string.Equals(newStatus, "FAILED", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Trạng thái không hợp lệ. Chỉ chấp nhận COMPLETED hoặc FAILED.");
            }

            // Get user account
            var account = await _accountServices.GetByIdAsync(transaction.UserId);
            if (account == null)
            {
                return (false, "Không tìm thấy tài khoản người dùng.");
            }

            var currentBalance = account.Balance ?? 0;
            var oldStatus = transaction.Status ?? string.Empty;

            // Check if already in the same status
            if (string.Equals(oldStatus, newStatus, StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"Giao dịch đã ở trạng thái '{oldStatus}' rồi.");
            }

            // Validate old status (should be PENDING, COMPLETED, or FAILED)
            if (!string.IsNullOrEmpty(oldStatus) && 
                !string.Equals(oldStatus, "PENDING", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(oldStatus, "COMPLETED", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(oldStatus, "FAILED", StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"Trạng thái hiện tại '{oldStatus}' không hợp lệ. Chỉ chấp nhận PENDING, COMPLETED, hoặc FAILED.");
            }

            // Handle status change and balance update using transaction
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update transaction status
                transaction.Status = newStatus;
                _context.PaymentTransactions.Update(transaction);

                // Handle balance update based on status change
                bool balanceChanged = false;
                
                if (string.Equals(oldStatus, "PENDING", StringComparison.OrdinalIgnoreCase))
                {
                    // PENDING -> COMPLETED: Deduct balance
                    if (string.Equals(newStatus, "COMPLETED", StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentBalance < transaction.Amount)
                        {
                            await dbTransaction.RollbackAsync();
                            return (false, "Số dư tài khoản không đủ để thực hiện giao dịch rút tiền.");
                        }
                        
                        account.Balance = currentBalance - transaction.Amount;
                        balanceChanged = true;
                    }
                    // PENDING -> FAILED: No balance change needed
                }
                else if (string.Equals(oldStatus, "COMPLETED", StringComparison.OrdinalIgnoreCase) && 
                         string.Equals(newStatus, "FAILED", StringComparison.OrdinalIgnoreCase))
                {
                    // COMPLETED -> FAILED: Refund balance (rollback)
                    account.Balance = currentBalance + transaction.Amount;
                    balanceChanged = true;
                }
                else if (string.Equals(oldStatus, "FAILED", StringComparison.OrdinalIgnoreCase) && 
                         string.Equals(newStatus, "COMPLETED", StringComparison.OrdinalIgnoreCase))
                {
                    // FAILED -> COMPLETED: Deduct balance
                    if (currentBalance < transaction.Amount)
                    {
                        await dbTransaction.RollbackAsync();
                        return (false, "Số dư tài khoản không đủ để thực hiện giao dịch rút tiền.");
                    }
                    
                    account.Balance = currentBalance - transaction.Amount;
                    balanceChanged = true;
                }

                // Update account if balance changed
                if (balanceChanged)
                {
                    account.UpdatedAt = DateTime.UtcNow;
                    _context.Accounts.Update(account);
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return (true, "Cập nhật trạng thái giao dịch thành công.");
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return (false, $"Có lỗi xảy ra khi cập nhật: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            return (false, $"Có lỗi xảy ra: {ex.Message}");
        }
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

    public async Task<TransactionHistoryListResponse> GetAllTransactionsAsync()
    {
        var query = _context.PaymentTransactions
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
            .Where(pt => pt.Type == "NapTien" || pt.Type == "BanHang" || pt.Type == "HoaHong")
            .SumAsync(pt => pt.Amount);

        var totalExpense = await _context.PaymentTransactions
            .Where(pt => pt.Type == "MuaHang" || pt.Type == "RutTien" || pt.Type == "ChiaSe")
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
