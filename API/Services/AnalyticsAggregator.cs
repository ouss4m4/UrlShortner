using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortner.API.Data;
using UrlShortner.Models;

namespace UrlShortner.API.Services
{
    /// <summary>
    /// Aggregates visit data into pre-computed analytics.
    /// Scheduler-agnostic - can be called by IHostedService, Hangfire, etc.
    /// </summary>
    public class AnalyticsAggregator : IAnalyticsAggregator
    {
        private readonly UrlShortnerDbContext _context;
        private readonly ILogger<AnalyticsAggregator> _logger;

        public AnalyticsAggregator(UrlShortnerDbContext context, ILogger<AnalyticsAggregator> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> AggregateHourlyAnalyticsAsync(DateTime? targetHour = null)
        {
            // Default to previous hour if not specified
            var hourToAggregate = targetHour ?? DateTime.UtcNow.AddHours(-1);
            var startOfHour = new DateTime(hourToAggregate.Year, hourToAggregate.Month, hourToAggregate.Day,
                                          hourToAggregate.Hour, 0, 0, DateTimeKind.Utc);
            var endOfHour = startOfHour.AddHours(1);

            _logger.LogInformation("Starting analytics aggregation for {StartHour} to {EndHour}",
                                   startOfHour, endOfHour);

            try
            {
                // Get all visits in this hour
                var visits = await _context.Visits
                    .Where(v => v.VisitedAt >= startOfHour && v.VisitedAt < endOfHour)
                    .ToListAsync();

                if (!visits.Any())
                {
                    _logger.LogInformation("No visits found for hour {StartHour}", startOfHour);
                    return 0;
                }

                // Group by country and aggregate
                var aggregations = visits
                    .GroupBy(v => v.Country)
                    .Select(g => new
                    {
                        Country = string.IsNullOrWhiteSpace(g.Key) ? "Unknown" : g.Key,
                        VisitCount = g.Count()
                    })
                    .ToList();

                int recordsCreated = 0;

                foreach (var agg in aggregations)
                {
                    // Check if analytics record already exists
                    var existing = await _context.Analytics
                        .FirstOrDefaultAsync(a =>
                            a.StatDate.Date == startOfHour.Date &&
                            a.StatHour == startOfHour.Hour &&
                            a.Country == agg.Country);

                    if (existing != null)
                    {
                        // Update existing record
                        existing.Visits = agg.VisitCount;
                        _logger.LogDebug("Updated analytics: {Date} {Hour}:00 {Country} = {Visits} visits",
                                        startOfHour.Date, startOfHour.Hour, agg.Country, agg.VisitCount);
                    }
                    else
                    {
                        // Create new record
                        var analytics = new Analytics
                        {
                            StatDate = startOfHour.Date,
                            StatHour = startOfHour.Hour,
                            Country = agg.Country,
                            Visits = agg.VisitCount
                        };
                        _context.Analytics.Add(analytics);
                        recordsCreated++;
                        _logger.LogDebug("Created analytics: {Date} {Hour}:00 {Country} = {Visits} visits",
                                        startOfHour.Date, startOfHour.Hour, agg.Country, agg.VisitCount);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Analytics aggregation complete: {RecordsCreated} records created/updated for hour {StartHour}",
                                       recordsCreated, startOfHour);

                return recordsCreated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during analytics aggregation for hour {StartHour}", startOfHour);
                throw;
            }
        }
    }
}
