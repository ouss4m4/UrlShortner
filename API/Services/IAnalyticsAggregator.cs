using System.Threading.Tasks;

namespace UrlShortner.API.Services
{
    /// <summary>
    /// Core analytics aggregation logic (scheduler-agnostic).
    /// Can be called by IHostedService, Hangfire, or any other scheduler.
    /// </summary>
    public interface IAnalyticsAggregator
    {
        /// <summary>
        /// Aggregates visit data into the Analytics table for the specified hour.
        /// If no hour specified, aggregates the previous hour.
        /// </summary>
        /// <param name="targetHour">The hour to aggregate (UTC). If null, aggregates previous hour.</param>
        /// <returns>Number of analytics records created/updated</returns>
        Task<int> AggregateHourlyAnalyticsAsync(DateTime? targetHour = null);
    }
}
