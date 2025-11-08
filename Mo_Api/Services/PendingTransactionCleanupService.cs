using Hangfire;
using Microsoft.EntityFrameworkCore;
using Mo_DataAccess.Services.Interface;
using Mo_Entities.Models;

namespace Mo_Api.Services;

public class PendingTransactionCleanupService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PendingTransactionCleanupService> _logger;

    public PendingTransactionCleanupService(
        IServiceProvider serviceProvider,
        ILogger<PendingTransactionCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Background job để tự động xử lý transaction PENDING:
    /// - Verify với SePay để chuyển thành COMPLETED nếu tìm thấy giao dịch (từ 5 giây trở lên)
    /// - Chuyển thành FAILED nếu quá 10 phút mà chưa có giao dịch
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessPendingTransactions()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SwpGroup6Context>();
            var sePayService = scope.ServiceProvider.GetRequiredService<SePayService>();
            var vietQrService = scope.ServiceProvider.GetRequiredService<VietQrService>();
            
            var now = DateTime.UtcNow;
            var tenMinutesAgo = now.AddMinutes(-10);
            var fiveSecondsAgo = now.AddSeconds(-5); // Chỉ verify transaction đã tạo ít nhất 5 giây (tránh verify quá sớm)
            
            // Tìm tất cả transaction PENDING của type NapTien
            var pendingTransactions = await context.PaymentTransactions
                .Where(t => t.Status == "PENDING" && 
                           t.CreatedAt.HasValue && 
                           t.Type == "NapTien")
                .ToListAsync();

            if (!pendingTransactions.Any())
            {
                _logger.LogInformation("No pending transactions found to process");
                return;
            }

            _logger.LogInformation($"Found {pendingTransactions.Count} pending transactions to process");

            int completedCount = 0;
            int failedCount = 0;
            int skippedCount = 0;

            foreach (var transaction in pendingTransactions)
            {
                try
                {
                    var transactionAge = (now - transaction.CreatedAt.Value).TotalMinutes;
                    
                    // Nếu quá 10 phút → chuyển thành FAILED
                    if (transaction.CreatedAt.Value <= tenMinutesAgo)
                    {
                        transaction.Status = "FAILED";
                        context.PaymentTransactions.Update(transaction);
                        failedCount++;
                        
                        _logger.LogInformation($"Transaction {transaction.Id} expired. Updated to FAILED. Age: {transactionAge:F1} minutes");
                        continue;
                    }
                    
                    // Nếu chưa đủ 5 giây → skip (tránh verify quá sớm)
                    if (transaction.CreatedAt.Value > fiveSecondsAgo)
                    {
                        skippedCount++;
                        _logger.LogDebug($"Transaction {transaction.Id} too new to verify. Age: {transactionAge * 60:F0} seconds. Skipping.");
                        continue;
                    }
                    
                    // Thử verify với SePay để chuyển thành COMPLETED
                    try
                    {
                        var qrContent = vietQrService.GenerateQrContent(transaction.Id.ToString(), transaction.UserId.ToString());
                        var sepayTransaction = await sePayService.FindMatchingTransactionAsync(
                            transaction.Amount, 
                            qrContent, 
                            transaction.CreatedAt
                        );
                        
                        // Nếu không tìm thấy với description đầy đủ, thử với transaction ID only
                        if (sepayTransaction == null)
                        {
                            var transactionIdOnly = $"NAPTIEN{transaction.Id}";
                            sepayTransaction = await sePayService.FindMatchingTransactionAsync(
                                transaction.Amount, 
                                transactionIdOnly, 
                                transaction.CreatedAt
                            );
                        }
                        
                        if (sepayTransaction != null && 
                            (sepayTransaction.Status == "SUCCESS" || sepayTransaction.Status == "COMPLETED"))
                        {
                            // Tìm thấy giao dịch từ SePay → chuyển thành COMPLETED
                            var account = await context.Accounts.FindAsync(transaction.UserId);
                            if (account != null)
                            {
                                using var dbTransaction = await context.Database.BeginTransactionAsync();
                                try
                                {
                                    transaction.Status = "COMPLETED";
                              
                                    
                                    var oldBalance = account.Balance ?? 0;
                                    account.Balance = oldBalance + transaction.Amount;
                                    account.UpdatedAt = now;
                                    
                                    context.PaymentTransactions.Update(transaction);
                                    context.Accounts.Update(account);
                                    
                                    await context.SaveChangesAsync();
                                    await dbTransaction.CommitAsync();
                                    
                                    completedCount++;
                                    _logger.LogInformation($"Transaction {transaction.Id} verified and updated to COMPLETED. Amount: {transaction.Amount}, New Balance: {account.Balance}");
                                }
                                catch (Exception ex)
                                {
                                    await dbTransaction.RollbackAsync();
                                    _logger.LogError(ex, $"Error updating transaction {transaction.Id} to COMPLETED");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Account not found for transaction {transaction.Id}, UserId: {transaction.UserId}");
                            }
                        }
                        else
                        {
                            _logger.LogDebug($"Transaction {transaction.Id} not found in SePay yet. Age: {transactionAge:F1} minutes. Will retry later.");
                        }
                    }
                    catch (Exception verifyEx)
                    {
                        _logger.LogError(verifyEx, $"Error verifying transaction {transaction.Id} with SePay");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing transaction {transaction.Id}");
                }
            }

            // Save tất cả changes (cho các transaction FAILED)
            if (failedCount > 0)
            {
                var changesCount = await context.SaveChangesAsync();
                _logger.LogInformation($"Saved {changesCount} changes for failed transactions");
            }

            _logger.LogInformation($"Processed pending transactions: {completedCount} completed, {failedCount} failed, {skippedCount} skipped (too new)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessPendingTransactions job");
            throw;
        }
    }
}

