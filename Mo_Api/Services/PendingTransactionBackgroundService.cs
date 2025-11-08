using Microsoft.Extensions.Hosting;

namespace Mo_Api.Services;

public class PendingTransactionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PendingTransactionBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(15);

    public PendingTransactionBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PendingTransactionBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PendingTransactionBackgroundService started. Running every 15 seconds.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cleanupService = scope.ServiceProvider.GetRequiredService<PendingTransactionCleanupService>();
                await cleanupService.ProcessPendingTransactions();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PendingTransactionBackgroundService");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("PendingTransactionBackgroundService stopped.");
    }
}

