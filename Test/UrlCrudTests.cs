using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Data;
using UrlShortner.API.Models;
using UrlShortner.API.Services;
using Xunit;

namespace Test
{
    public class UrlCrudTests
    {
        private UrlShortnerDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<UrlShortnerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new UrlShortnerDbContext(options);
        }

        private IShortCodeGenerator GetShortCodeGenerator()
        {
            return new ShortCodeGenerator();
        }

        [Fact]
        public async Task CanCreateUrl()
        {
            using var context = GetInMemoryDbContext();
            var service = new UrlService(context, GetShortCodeGenerator());
            var url = new Url
            {
                OriginalUrl = "https://example.com",
                ShortCode = "abc123",
                UserId = 1,
                CreatedAt = DateTime.UtcNow
            };

            var created = await service.CreateUrlAsync(url);

            Assert.NotEqual(0, created.Id);
            Assert.Equal("https://example.com", created.OriginalUrl);
            Assert.Equal("abc123", created.ShortCode);
        }

        [Fact]
        public async Task CanReadUrl()
        {
            using var context = GetInMemoryDbContext();
            var service = new UrlService(context, GetShortCodeGenerator());
            var url = new Url
            {
                OriginalUrl = "https://test.com",
                ShortCode = "test123",
                UserId = 1,
                CreatedAt = DateTime.UtcNow
            };
            await service.CreateUrlAsync(url);

            var retrieved = await service.GetUrlByIdAsync(url.Id);

            Assert.NotNull(retrieved);
            Assert.Equal("https://test.com", retrieved.OriginalUrl);
            Assert.Equal("test123", retrieved.ShortCode);
        }

        [Fact]
        public async Task CanGetUrlByShortCode()
        {
            using var context = GetInMemoryDbContext();
            var service = new UrlService(context, GetShortCodeGenerator());
            var url = new Url
            {
                OriginalUrl = "https://google.com",
                ShortCode = "goog",
                UserId = 1,
                CreatedAt = DateTime.UtcNow
            };
            await service.CreateUrlAsync(url);

            var retrieved = await service.GetUrlByShortCodeAsync("goog");

            Assert.NotNull(retrieved);
            Assert.Equal("https://google.com", retrieved.OriginalUrl);
        }

        [Fact]
        public async Task CanUpdateUrl()
        {
            using var context = GetInMemoryDbContext();
            var service = new UrlService(context, GetShortCodeGenerator());
            var url = new Url
            {
                OriginalUrl = "https://old.com",
                ShortCode = "old123",
                UserId = 1,
                CreatedAt = DateTime.UtcNow
            };
            await service.CreateUrlAsync(url);

            url.OriginalUrl = "https://new.com";
            var updated = await service.UpdateUrlAsync(url);

            Assert.NotNull(updated);
            Assert.Equal("https://new.com", updated.OriginalUrl);
        }

        [Fact]
        public async Task CanDeleteUrl()
        {
            using var context = GetInMemoryDbContext();
            var service = new UrlService(context, GetShortCodeGenerator());
            var url = new Url
            {
                OriginalUrl = "https://delete.com",
                ShortCode = "del123",
                UserId = 1,
                CreatedAt = DateTime.UtcNow
            };
            await service.CreateUrlAsync(url);

            var deleted = await service.DeleteUrlAsync(url.Id);
            var retrieved = await service.GetUrlByIdAsync(url.Id);

            Assert.True(deleted);
            Assert.Null(retrieved);
        }

        [Fact]
        public async Task AutoGeneratesShortCodeWhenEmpty()
        {
            using var context = GetInMemoryDbContext();
            var service = new UrlService(context, GetShortCodeGenerator());
            var url = new Url
            {
                OriginalUrl = "https://example.com/very/long/url/path",
                ShortCode = "",
                UserId = 1,
                CreatedAt = DateTime.UtcNow
            };

            var created = await service.CreateUrlAsync(url);

            Assert.NotEqual(0, created.Id);
            Assert.NotEmpty(created.ShortCode);
            Assert.DoesNotContain(" ", created.ShortCode);
            Assert.Matches(@"^[0-9a-zA-Z]+$", created.ShortCode);
        }

        [Fact]
        public async Task PreservesManualShortCode()
        {
            using var context = GetInMemoryDbContext();
            var service = new UrlService(context, GetShortCodeGenerator());
            var url = new Url
            {
                OriginalUrl = "https://example.com",
                ShortCode = "custom",
                UserId = 1,
                CreatedAt = DateTime.UtcNow
            };

            var created = await service.CreateUrlAsync(url);

            Assert.Equal("custom", created.ShortCode);
        }

        [Fact]
        public async Task ThrowsExceptionWhenCustomShortCodeAlreadyExists()
        {
            using var context = GetInMemoryDbContext();
            var service = new UrlService(context, GetShortCodeGenerator());

            var url1 = new Url
            {
                OriginalUrl = "https://facebook.com",
                ShortCode = "fb",
                UserId = 1,
                CreatedAt = DateTime.UtcNow
            };
            await service.CreateUrlAsync(url1);

            var url2 = new Url
            {
                OriginalUrl = "https://facebook.com/different",
                ShortCode = "fb",
                UserId = 2,
                CreatedAt = DateTime.UtcNow
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.CreateUrlAsync(url2)
            );

            Assert.Contains("fb", exception.Message);
            Assert.Contains("already taken", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
