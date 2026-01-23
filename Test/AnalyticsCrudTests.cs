using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Data;
using UrlShortner.API.Models;
using UrlShortner.API.Services;
using Xunit;

namespace Test
{
    public class AnalyticsCrudTests
    {
        private UrlShortnerDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<UrlShortnerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new UrlShortnerDbContext(options);
        }

        [Fact]
        public async Task CanCreateAnalytics()
        {
            using var context = GetInMemoryDbContext();
            var service = new AnalyticsService(context);
            var analytics = new Analytics
            {
                StatDate = DateTime.UtcNow.Date,
                StatHour = 14,
                Visits = 100,
                Country = "US"
            };

            var created = await service.CreateAnalyticsAsync(analytics);

            Assert.NotEqual(0, created.Id);
            Assert.Equal(100, created.Visits);
        }

        [Fact]
        public async Task CanGetAnalyticsByDate()
        {
            using var context = GetInMemoryDbContext();
            var service = new AnalyticsService(context);
            var date = DateTime.UtcNow.Date;
            await service.CreateAnalyticsAsync(new Analytics { StatDate = date, StatHour = 1, Visits = 10, Country = "US" });
            await service.CreateAnalyticsAsync(new Analytics { StatDate = date, StatHour = 2, Visits = 20, Country = "UK" });
            await service.CreateAnalyticsAsync(new Analytics { StatDate = date.AddDays(-1), StatHour = 1, Visits = 5, Country = "US" });

            var results = await service.GetAnalyticsByDateAsync(date);

            Assert.Equal(2, results.Count());
        }

        [Fact]
        public async Task CanDeleteAnalytics()
        {
            using var context = GetInMemoryDbContext();
            var service = new AnalyticsService(context);
            var analytics = new Analytics { StatDate = DateTime.UtcNow.Date, StatHour = 5, Visits = 30, Country = "CA" };
            await service.CreateAnalyticsAsync(analytics);

            var deleted = await service.DeleteAnalyticsAsync(analytics.Id);

            Assert.True(deleted);
        }
    }
}
