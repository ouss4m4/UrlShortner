using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Data;
using UrlShortner.API.Services;
using UrlShortner.Models;
using Xunit;

namespace Test;

public class UrlExpirationTests
{
    private UrlShortnerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<UrlShortnerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new UrlShortnerDbContext(options);
    }

    [Fact]
    public async Task GetUrlByShortCodeAsync_ReturnsNull_WhenUrlIsExpired()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var shortCodeGenerator = new ShortCodeGenerator();
        var urlService = new UrlService(context, shortCodeGenerator, null);

        var expiredUrl = new Url
        {
            OriginalUrl = "https://example.com",
            ShortCode = string.Empty,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            Expiry = DateTime.UtcNow.AddDays(-1) // Expired yesterday
        };

        context.Urls.Add(expiredUrl);
        await context.SaveChangesAsync();

        var shortCode = shortCodeGenerator.Encode(expiredUrl.Id);
        expiredUrl.ShortCode = shortCode;
        await context.SaveChangesAsync();

        // Act
        var result = await urlService.GetUrlByShortCodeAsync(shortCode);

        // Assert
        Assert.Null(result); // Should return null for expired URL
    }

    [Fact]
    public async Task GetUrlByShortCodeAsync_ReturnsUrl_WhenUrlIsNotExpired()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var shortCodeGenerator = new ShortCodeGenerator();
        var urlService = new UrlService(context, shortCodeGenerator, null);

        var activeUrl = new Url
        {
            OriginalUrl = "https://example.com",
            ShortCode = string.Empty,
            CreatedAt = DateTime.UtcNow,
            Expiry = DateTime.UtcNow.AddDays(7) // Expires in 7 days
        };

        context.Urls.Add(activeUrl);
        await context.SaveChangesAsync();

        var shortCode = shortCodeGenerator.Encode(activeUrl.Id);
        activeUrl.ShortCode = shortCode;
        await context.SaveChangesAsync();

        // Act
        var result = await urlService.GetUrlByShortCodeAsync(shortCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://example.com", result.OriginalUrl);
    }

    [Fact]
    public async Task GetUrlByShortCodeAsync_ReturnsUrl_WhenExpiryIsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var shortCodeGenerator = new ShortCodeGenerator();
        var urlService = new UrlService(context, shortCodeGenerator, null);

        var permanentUrl = new Url
        {
            OriginalUrl = "https://example.com",
            ShortCode = string.Empty,
            CreatedAt = DateTime.UtcNow,
            Expiry = null // No expiration
        };

        context.Urls.Add(permanentUrl);
        await context.SaveChangesAsync();

        var shortCode = shortCodeGenerator.Encode(permanentUrl.Id);
        permanentUrl.ShortCode = shortCode;
        await context.SaveChangesAsync();

        // Act
        var result = await urlService.GetUrlByShortCodeAsync(shortCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://example.com", result.OriginalUrl);
    }

    [Fact]
    public async Task GetUrlsByUserIdAsync_ExcludesExpiredUrls()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var shortCodeGenerator = new ShortCodeGenerator();
        var urlService = new UrlService(context, shortCodeGenerator, null);

        var urls = new List<Url>
        {
            new Url
            {
                UserId = 1,
                OriginalUrl = "https://active1.com",
                ShortCode = "a1",
                CreatedAt = DateTime.UtcNow,
                Expiry = DateTime.UtcNow.AddDays(7) // Active
            },
            new Url
            {
                UserId = 1,
                OriginalUrl = "https://expired.com",
                ShortCode = "ex",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Expiry = DateTime.UtcNow.AddDays(-1) // Expired
            },
            new Url
            {
                UserId = 1,
                OriginalUrl = "https://permanent.com",
                ShortCode = "pm",
                CreatedAt = DateTime.UtcNow,
                Expiry = null // No expiration
            }
        };

        context.Urls.AddRange(urls);
        await context.SaveChangesAsync();

        // Act
        var result = await urlService.GetUrlsByUserIdAsync(1);

        // Assert
        Assert.Equal(2, result.Count()); // Only active and permanent URLs
        Assert.DoesNotContain(result, u => u.OriginalUrl == "https://expired.com");
    }

    [Fact]
    public async Task CreateUrlAsync_AcceptsExpiryDate()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var shortCodeGenerator = new ShortCodeGenerator();
        var urlService = new UrlService(context, shortCodeGenerator, null);

        var url = new Url
        {
            OriginalUrl = "https://example.com",
            ShortCode = string.Empty,
            CreatedAt = DateTime.UtcNow,
            Expiry = DateTime.UtcNow.AddDays(30) // Expires in 30 days
        };

        // Act
        var result = await urlService.CreateUrlAsync(url);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Expiry);
        Assert.True(result.Expiry > DateTime.UtcNow);
    }
}
