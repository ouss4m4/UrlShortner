using API.Services;
using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Data;
using UrlShortner.API.Models;
using UrlShortner.API.Services;
using Xunit;

namespace Test;

public class UrlCachingTests : IAsyncLifetime
{
    private readonly UrlShortnerDbContext _context;
    private readonly IShortCodeGenerator _shortCodeGenerator;
    private readonly ICacheService _cacheService;
    private readonly IUrlService _urlService;

    public UrlCachingTests()
    {
        var options = new DbContextOptionsBuilder<UrlShortnerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UrlShortnerDbContext(options);
        _shortCodeGenerator = new ShortCodeGenerator();

        // Use real Redis for cache tests
        var redis = StackExchange.Redis.ConnectionMultiplexer.Connect("localhost:6379");
        _cacheService = new RedisCacheService(redis);

        _urlService = new UrlService(_context, _shortCodeGenerator, _cacheService);
    }

    public async Task InitializeAsync()
    {
        // Clean up cache before each test
        var redis = StackExchange.Redis.ConnectionMultiplexer.Connect("localhost:6379");
        var db = redis.GetDatabase();
        await db.ExecuteAsync("FLUSHDB");
    }

    public Task DisposeAsync()
    {
        _context.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetUrlByShortCodeAsync_CachesResult_OnFirstCall()
    {
        // Arrange
        var url = new Url
        {
            OriginalUrl = "https://example.com",
            UserId = 1
        };
        var created = await _urlService.CreateUrlAsync(url);

        // Clear cache to ensure fresh start
        await _cacheService.RemoveAsync($"url:shortcode:{created.ShortCode}");

        // Act - First call should cache the result
        var result1 = await _urlService.GetUrlByShortCodeAsync(created.ShortCode);
        var cached = await _cacheService.GetAsync<Url>($"url:shortcode:{created.ShortCode}");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(cached);
        Assert.Equal(created.Id, cached.Id);
        Assert.Equal(created.OriginalUrl, cached.OriginalUrl);
    }

    [Fact]
    public async Task GetUrlByShortCodeAsync_ReturnsFromCache_OnSecondCall()
    {
        // Arrange
        var url = new Url
        {
            OriginalUrl = "https://example.com",
            UserId = 1
        };
        var created = await _urlService.CreateUrlAsync(url);

        // Act - First call populates cache
        var result1 = await _urlService.GetUrlByShortCodeAsync(created.ShortCode);

        // Modify the database record
        created.OriginalUrl = "https://modified.com";
        await _context.SaveChangesAsync();

        // Second call should return cached version (not modified)
        var result2 = await _urlService.GetUrlByShortCodeAsync(created.ShortCode);

        // Assert
        Assert.NotNull(result2);
        Assert.Equal("https://example.com", result2.OriginalUrl); // From cache, not DB
    }

    [Fact]
    public async Task UpdateUrlAsync_InvalidatesCache()
    {
        // Arrange
        var url = new Url
        {
            OriginalUrl = "https://example.com",
            UserId = 1
        };
        var created = await _urlService.CreateUrlAsync(url);

        // Populate cache
        await _urlService.GetUrlByShortCodeAsync(created.ShortCode);
        var cachedBefore = await _cacheService.GetAsync<Url>($"url:shortcode:{created.ShortCode}");

        // Act - Update should invalidate cache
        created.OriginalUrl = "https://updated.com";
        await _urlService.UpdateUrlAsync(created);

        var cachedAfter = await _cacheService.GetAsync<Url>($"url:shortcode:{created.ShortCode}");

        // Assert
        Assert.NotNull(cachedBefore);
        Assert.Null(cachedAfter); // Cache should be invalidated
    }

    [Fact]
    public async Task DeleteUrlAsync_InvalidatesCache()
    {
        // Arrange
        var url = new Url
        {
            OriginalUrl = "https://example.com",
            UserId = 1
        };
        var created = await _urlService.CreateUrlAsync(url);

        // Populate cache
        await _urlService.GetUrlByShortCodeAsync(created.ShortCode);
        var cachedBefore = await _cacheService.GetAsync<Url>($"url:shortcode:{created.ShortCode}");

        // Act - Delete should invalidate cache
        await _urlService.DeleteUrlAsync(created.Id);

        var cachedAfter = await _cacheService.GetAsync<Url>($"url:shortcode:{created.ShortCode}");

        // Assert
        Assert.NotNull(cachedBefore);
        Assert.Null(cachedAfter); // Cache should be invalidated
    }
}
