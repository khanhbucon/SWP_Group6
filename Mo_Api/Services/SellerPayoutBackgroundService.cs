//using System.Text.RegularExpressions;
//using Microsoft.EntityFrameworkCore;
//using Mo_DataAccess.Services.Interface;
//using Mo_Entities.Models;

//namespace Mo_Api.Services;

//public class SellerPayoutBackgroundService : BackgroundService
//{
//    //private readonly IServiceProvider _serviceProvider;
//    //private readonly ILogger<SellerPayoutBackgroundService> _logger;
//    //// Test config: hold window 2 minutes, poll every 30 seconds
//    //private static readonly TimeSpan HoldWindow = TimeSpan.FromMinutes(2);
//    //private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

//    //public SellerPayoutBackgroundService(IServiceProvider serviceProvider, ILogger<SellerPayoutBackgroundService> logger)
//    //{
//    //    _serviceProvider = serviceProvider;
//    //    _logger = logger;
//    //}

//    //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    //{
//    //    _logger.LogInformation("SellerPayoutBackgroundService started");
//    //    while (!stoppingToken.IsCancellationRequested)
//    //    {
//    //        try
//    //        {
//    //            await ProcessDuePayoutsAsync(stoppingToken);
//    //        }
//    //        catch (Exception ex)
//    //        {
//    //            _logger.LogError(ex, "Error processing seller payouts");
//    //        }

//    //        try
//    //        {
//    //            await Task.Delay(Interval, stoppingToken);
//    //        }
//    //        catch (TaskCanceledException)
//    //        {
//    //            // shutting down
//    //        }
//    //    }
//    //    _logger.LogInformation("SellerPayoutBackgroundService stopped");
//    //}

//    //private async Task ProcessDuePayoutsAsync(CancellationToken ct)
//    //{
//    //    using var scope = _serviceProvider.CreateScope();
//    //    var db = scope.ServiceProvider.GetRequiredService<SwpGroup6Context>();
//    //    var orderService = scope.ServiceProvider.GetRequiredService<IOrderProductServices>();

//    //    var thresholdUtc = DateTime.UtcNow - HoldWindow;

//    //    var pending = await db.PaymentTransactions
//    //        .Where(pt => pt.Type == "BanHang" && pt.Status == "PENDING" && (pt.CreatedAt ?? DateTime.UtcNow) <= thresholdUtc)
//    //        .OrderBy(pt => pt.CreatedAt)
//    //        .Take(200)
//    //        .ToListAsync(ct);

//    //    foreach (var pt in pending)
//    //    {
//    //        if (ct.IsCancellationRequested) break;

//    //        var orderId = TryParseOrderId(pt.PaymentDescription);
//    //        if (orderId == null)
//    //        {
//    //            _logger.LogWarning("Cannot parse OrderId from PaymentDescription: {Desc}", pt.PaymentDescription);
//    //            continue;
//    //        }

//    //        try
//    //        {
//    //            var released = await orderService.ReleaseSellerPayoutAsync(orderId.Value);
//    //            _logger.LogInformation("Release payout for Order {OrderId}: {Released}", orderId, released);
//    //        }
//    //        catch (Exception ex)
//    //        {
//    //            _logger.LogError(ex, "Error releasing payout for Order {OrderId}", orderId);
//    //        }
//    //    }
//    //}

//    //private static long? TryParseOrderId(string? description)
//    //{
//    //    if (string.IsNullOrWhiteSpace(description)) return null;
//    //    // Expect pattern: "... (Order #123)"
//    //    var match = Regex.Match(description, @"Order\s*#(\d+)");
//    //    if (match.Success && long.TryParse(match.Groups[1].Value, out var id))
//    //    {
//    //        return id;
//    //    }
//    //    return null;
//    //}
//    protected override Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        throw new NotImplementedException();
//    }
//}


