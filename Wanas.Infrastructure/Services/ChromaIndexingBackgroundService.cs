using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wanas.Application.Interfaces;

namespace Wanas.Infrastructure.Services
{
    /// <summary>
    /// Background service that indexes new listings to ChromaDB every hour
    /// </summary>
    public class ChromaIndexingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ChromaIndexingBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);

        public ChromaIndexingBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ChromaIndexingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ChromaDB Indexing Background Service started");

            // Wait a bit on startup to let the app initialize
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await IndexRecentListingsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during background indexing");
                }

                // Wait for the next interval
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("ChromaDB Indexing Background Service stopped");
        }

        private async Task IndexRecentListingsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var indexingService = scope.ServiceProvider.GetRequiredService<IChromaIndexingService>();

            _logger.LogInformation("Running scheduled indexing for recent listings");

            // Index listings from the last hour + 5 minutes buffer
            var since = DateTime.UtcNow.Subtract(_interval).AddMinutes(-5);
            var result = await indexingService.IndexRecentListingsAsync(since);

            if (result.TotalProcessed > 0)
            {
                _logger.LogInformation(
                             "Scheduled indexing complete. Processed: {Total}, Success: {Success}, Failed: {Failed}",
                  result.TotalProcessed, result.SuccessCount, result.FailedCount
                   );

                if (result.FailedCount > 0)
                {
                    _logger.LogWarning("Indexing errors: {Errors}", string.Join("; ", result.Errors));
                }
            }
        }
    }
}
