using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Data;
using UrlShortner.API.Models;
using UrlShortner.API.Services;
using Xunit;

namespace Test
{
    public class VisitCrudTests
    {
        private UrlShortnerDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<UrlShortnerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new UrlShortnerDbContext(options);
        }

        [Fact]
        public async Task CanCreateVisit()
        {
            using var context = GetInMemoryDbContext();
            var service = new VisitService(context);
            var visit = new Visit
            {
                UrlId = 1,
                UserId = 1,
                Metadata = "Mozilla/5.0",
                VisitedAt = DateTime.UtcNow
            };

            var created = await service.CreateVisitAsync(visit);

            Assert.NotEqual(0, created.Id);
            Assert.Equal(1, created.UrlId);
        }

        [Fact]
        public async Task CanGetVisitsByUrlId()
        {
            using var context = GetInMemoryDbContext();
            var service = new VisitService(context);
            await service.CreateVisitAsync(new Visit { UrlId = 5, UserId = 1, Metadata = "V1", VisitedAt = DateTime.UtcNow });
            await service.CreateVisitAsync(new Visit { UrlId = 5, UserId = 2, Metadata = "V2", VisitedAt = DateTime.UtcNow });
            await service.CreateVisitAsync(new Visit { UrlId = 6, UserId = 1, Metadata = "V3", VisitedAt = DateTime.UtcNow });

            var visits = await service.GetVisitsByUrlIdAsync(5);

            Assert.Equal(2, visits.Count());
        }

        [Fact]
        public async Task CanDeleteVisit()
        {
            using var context = GetInMemoryDbContext();
            var service = new VisitService(context);
            var visit = new Visit { UrlId = 1, UserId = 1, Metadata = "Del", VisitedAt = DateTime.UtcNow };
            await service.CreateVisitAsync(visit);

            var deleted = await service.DeleteVisitAsync(visit.Id);

            Assert.True(deleted);
        }
    }
}
