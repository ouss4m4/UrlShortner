using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UrlShortner.API.Services
{
    /// <summary>
    /// IHostedService that schedules periodic analytics aggregation.
    /// To switch to Hangfire: replace this service with Hangfire job registration,
    /// but keep using IAnalyticsAggregator for the actual work.
    /// </summary>
    public class AnalyticsAggregationHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AnalyticsAggregationHostedService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Run every hour

        public AnalyticsAggregationHostedService(
            IServiceProvider serviceProvider,
            ILogger<AnalyticsAggregationHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Analytics Aggregation Service started. Running every {Interval}", _interval);

            // Wait for the app to fully start
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunAggregationAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during scheduled analytics aggregation");
                }

                // Wait for next interval
                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Service is stopping, exit gracefully
                    break;
                }
            }

            _logger.LogInformation("Analytics Aggregation Service stopped");
        }

        private async Task RunAggregationAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var aggregator = scope.ServiceProvider.GetRequiredService<IAnalyticsAggregator>();

            _logger.LogInformation("Running scheduled analytics aggregation...");
            var recordsProcessed = await aggregator.AggregateHourlyAnalyticsAsync();
            _logger.LogInformation("Scheduled aggregation complete: {RecordsProcessed} records", recordsProcessed);
        }
    }
}
