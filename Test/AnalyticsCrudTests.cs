using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Data;
using UrlShortner.API.Models;
using UrlShortner.API.Services;
using Xunit;

namespace Test
{
    // Analytics are computed from Visits, not created manually
    public class AnalyticsComputationTests
    {
        private UrlShortnerDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<UrlShortnerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new UrlShortnerDbContext(options);
        }

        [Fact]
        public async Task CanComputeUrlAnalytics()
        {
            using var context = GetInMemoryDbContext();
            var service = new AnalyticsService(context);

            // Create a URL
            var url = new Url
            {
                ShortCode = "test123",
                OriginalUrl = "https://example.com",
                CreatedAt = DateTime.UtcNow,
                UserId = 1
            };
            context.Urls.Add(url);
            await context.SaveChangesAsync();

            // Create some visits for this URL
            var visit1 = new Visit
            {
                UrlId = url.Id,
                IpAddress = "192.168.1.1",
                UserAgent = "Browser1",
                Referrer = "",
                Country = "US",
                VisitedAt = DateTime.UtcNow.AddHours(-2)
            };
            var visit2 = new Visit
            {
                UrlId = url.Id,
                IpAddress = "192.168.1.2",
                UserAgent = "Browser2",
                Referrer = "",
                Country = "UK",
                VisitedAt = DateTime.UtcNow.AddHours(-1)
            };
            context.Visits.AddRange(visit1, visit2);
            await context.SaveChangesAsync();

            // Compute analytics
            var analytics = await service.GetUrlAnalyticsAsync(url.Id);

            Assert.Equal(url.Id, analytics.UrlId);
            Assert.Equal("test123", analytics.ShortCode);
            Assert.Equal("https://example.com", analytics.OriginalUrl);
            Assert.Equal(2, analytics.TotalVisits);
            Assert.NotNull(analytics.FirstVisit);
            Assert.NotNull(analytics.LastVisit);
        }

        [Fact]
        public async Task CanComputeAnalyticsByDateRange()
        {
            using var context = GetInMemoryDbContext();
            var service = new AnalyticsService(context);

            var url = new Url { ShortCode = "abc", OriginalUrl = "https://test.com", CreatedAt = DateTime.UtcNow, UserId = 1 };
            context.Urls.Add(url);
            await context.SaveChangesAsync();

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            // Add visits for today
            context.Visits.Add(new Visit { UrlId = url.Id, IpAddress = "1.1.1.1", UserAgent = "A", Referrer = "", Country = "", VisitedAt = today.AddHours(1) });
            context.Visits.Add(new Visit { UrlId = url.Id, IpAddress = "1.1.1.2", UserAgent = "B", Referrer = "", Country = "", VisitedAt = today.AddHours(2) });

            // Add visit for yesterday
            context.Visits.Add(new Visit { UrlId = url.Id, IpAddress = "1.1.1.3", UserAgent = "C", Referrer = "", Country = "", VisitedAt = yesterday.AddHours(1) });

            await context.SaveChangesAsync();

            var analytics = await service.GetAnalyticsByDateRangeAsync(yesterday, today.AddDays(1));
            var analyticsList = analytics.ToList();

            Assert.Equal(2, analyticsList.Count); // 2 different days
            Assert.Contains(analyticsList, a => a.Date == today && a.TotalVisits == 2);
            Assert.Contains(analyticsList, a => a.Date == yesterday && a.TotalVisits == 1);
        }

        [Fact]
        public async Task CanComputeAnalyticsByCountry()
        {
            using var context = GetInMemoryDbContext();
            var service = new AnalyticsService(context);

            var url = new Url { ShortCode = "xyz", OriginalUrl = "https://test.com", CreatedAt = DateTime.UtcNow, UserId = 1 };
            context.Urls.Add(url);
            await context.SaveChangesAsync();

            // Add visits from different countries
            context.Visits.Add(new Visit { UrlId = url.Id, IpAddress = "1.1.1.1", UserAgent = "A", Referrer = "", Country = "US", VisitedAt = DateTime.UtcNow });
            context.Visits.Add(new Visit { UrlId = url.Id, IpAddress = "1.1.1.2", UserAgent = "B", Referrer = "", Country = "US", VisitedAt = DateTime.UtcNow });
            context.Visits.Add(new Visit { UrlId = url.Id, IpAddress = "1.1.1.3", UserAgent = "C", Referrer = "", Country = "UK", VisitedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var analytics = await service.GetAnalyticsByCountryAsync();
            var analyticsList = analytics.ToList();

            Assert.Equal(2, analyticsList.Count);
            Assert.Contains(analyticsList, a => a.Country == "US" && a.TotalVisits == 2);
            Assert.Contains(analyticsList, a => a.Country == "UK" && a.TotalVisits == 1);
        }
    }
}
