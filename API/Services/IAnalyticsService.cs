using System.Collections.Generic;
using System.Threading.Tasks;

namespace UrlShortner.API.Services
{
    // Analytics are computed from Visit events - read-only
    public interface IAnalyticsService
    {
        // Get analytics for a specific URL
        Task<UrlAnalytics> GetUrlAnalyticsAsync(int urlId);

        // Get analytics by date range
        Task<IEnumerable<DateAnalytics>> GetAnalyticsByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Get analytics by country
        Task<IEnumerable<CountryAnalytics>> GetAnalyticsByCountryAsync();
    }

    public class UrlAnalytics
    {
        public int UrlId { get; set; }
        public string ShortCode { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
        public int TotalVisits { get; set; }
        public DateTime? LastVisit { get; set; }
        public DateTime? FirstVisit { get; set; }
    }

    public class DateAnalytics
    {
        public DateTime Date { get; set; }
        public int TotalVisits { get; set; }
        public int UniqueIps { get; set; }
    }

    public class CountryAnalytics
    {
        public string Country { get; set; } = string.Empty;
        public int TotalVisits { get; set; }
    }
}
