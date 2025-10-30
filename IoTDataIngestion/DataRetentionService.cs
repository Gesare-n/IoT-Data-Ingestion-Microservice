using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IoTDataIngestion
{
    /// <summary>
    /// Background service that implements data retention policies
    /// Automatically cleans up old sensor readings to prevent database growth
    /// </summary>
    public class DataRetentionService : BackgroundService
    {
        private readonly ILogger<DataRetentionService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public DataRetentionService(ILogger<DataRetentionService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Main execution method for the data retention service
        /// Runs cleanup task every hour
        /// </summary>
        /// <param name="stoppingToken">Token to signal service shutdown</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldData();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during data retention cleanup");
                }

                // Wait for 1 hour before next cleanup
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        /// <summary>
        /// Cleans up sensor readings older than 30 days
        /// </summary>
        private async Task CleanupOldData()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var cutoffDate = DateTimeOffset.Now.AddDays(-30);
            var oldReadings = dbContext.SensorReadings
                .AsEnumerable()
                .Where(r => r.Timestamp < cutoffDate);
            
            var count = oldReadings.Count();
            
            if (count > 0)
            {
                dbContext.SensorReadings.RemoveRange(oldReadings);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} sensor readings older than {Days} days", count, 30);
            }
            else
            {
                _logger.LogInformation("No old sensor readings to clean up");
            }
        }
    }
}