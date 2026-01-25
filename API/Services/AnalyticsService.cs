using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Data;

namespace UrlShortner.API.Services
{
    // Analytics computed from Visit events in real-time
    public class AnalyticsService : IAnalyticsService
    {
        private readonly UrlShortnerDbContext _context;

        public AnalyticsService(UrlShortnerDbContext context)
        {
            _context = context;
        }

        public async Task<UrlAnalytics> GetUrlAnalyticsAsync(int urlId)
        {
            var url = await _context.Urls.FindAsync(urlId);
            if (url == null)
            {
                return new UrlAnalytics { UrlId = urlId };
            }

            var visits = await _context.Visits
                .Where(v => v.UrlId == urlId)
                .ToListAsync();

            return new UrlAnalytics
            {
                UrlId = urlId,
                ShortCode = url.ShortCode,
                OriginalUrl = url.OriginalUrl,
                TotalVisits = visits.Count,
                FirstVisit = visits.Any() ? visits.Min(v => v.VisitedAt) : null,
                LastVisit = visits.Any() ? visits.Max(v => v.VisitedAt) : null
            };
        }

        public async Task<IEnumerable<DateAnalytics>> GetAnalyticsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var visits = await _context.Visits
                .Where(v => v.VisitedAt >= startDate && v.VisitedAt <= endDate)
                .ToListAsync();

            return visits
                .GroupBy(v => v.VisitedAt.Date)
                .Select(g => new DateAnalytics
                {
                    Date = g.Key,
                    TotalVisits = g.Count(),
                    UniqueIps = g.Select(v => v.IpAddress).Distinct().Count()
                })
                .OrderBy(a => a.Date)
                .ToList();
        }

        public async Task<IEnumerable<CountryAnalytics>> GetAnalyticsByCountryAsync()
        {
            var visits = await _context.Visits.ToListAsync();

            return visits
                .Where(v => !string.IsNullOrEmpty(v.Country))
                .GroupBy(v => v.Country)
                .Select(g => new CountryAnalytics
                {
                    Country = g.Key,
                    TotalVisits = g.Count()
                })
                .OrderByDescending(a => a.TotalVisits)
                .ToList();
        }
    }
}
