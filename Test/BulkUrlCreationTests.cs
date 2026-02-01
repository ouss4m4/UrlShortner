using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Services;
using UrlShortner.API.Data;
using UrlShortner.Models;
using Xunit;

namespace Test;

public class BulkUrlCreationTests
{
    private static UrlShortnerDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<UrlShortnerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new UrlShortnerDbContext(options);
    }

    private static IShortCodeGenerator GetShortCodeGenerator()
    {
        return new ShortCodeGenerator();
    }

    [Fact]
    public async Task BulkCreateUrlsAsync_CreatesMultipleUrls()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var urls = new List<Url>
        {
            new Url { OriginalUrl = "https://example1.com", UserId = 1, CreatedAt = DateTime.UtcNow },
            new Url { OriginalUrl = "https://example2.com", UserId = 1, CreatedAt = DateTime.UtcNow },
            new Url { OriginalUrl = "https://example3.com", UserId = 1, CreatedAt = DateTime.UtcNow }
        };

        var result = await service.BulkCreateUrlsAsync(urls);

        Assert.Equal(3, result.SuccessCount);
        Assert.Empty(result.Failures);
        Assert.Equal(3, result.CreatedUrls.Count());
        Assert.All(result.CreatedUrls, url => Assert.NotEmpty(url.ShortCode));
    }

    [Fact]
    public async Task BulkCreateUrlsAsync_AutoGeneratesShortCodes()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var urls = new List<Url>
        {
            new Url { OriginalUrl = "https://example1.com", UserId = 1, CreatedAt = DateTime.UtcNow },
            new Url { OriginalUrl = "https://example2.com", UserId = 1, CreatedAt = DateTime.UtcNow }
        };

        var result = await service.BulkCreateUrlsAsync(urls);

        Assert.Equal(2, result.SuccessCount);
        Assert.All(result.CreatedUrls, url =>
        {
            Assert.NotNull(url.ShortCode);
            Assert.NotEmpty(url.ShortCode);
        });

        // Short codes should be unique
        var shortCodes = result.CreatedUrls.Select(u => u.ShortCode).ToList();
        Assert.Equal(shortCodes.Count, shortCodes.Distinct().Count());
    }

    [Fact]
    public async Task BulkCreateUrlsAsync_AllowsCustomShortCodes()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var urls = new List<Url>
        {
            new Url { OriginalUrl = "https://example1.com", ShortCode = "custom1", UserId = 1, CreatedAt = DateTime.UtcNow },
            new Url { OriginalUrl = "https://example2.com", ShortCode = "custom2", UserId = 1, CreatedAt = DateTime.UtcNow }
        };

        var result = await service.BulkCreateUrlsAsync(urls);

        Assert.Equal(2, result.SuccessCount);
        Assert.Contains(result.CreatedUrls, u => u.ShortCode == "custom1");
        Assert.Contains(result.CreatedUrls, u => u.ShortCode == "custom2");
    }

    [Fact]
    public async Task BulkCreateUrlsAsync_HandlesDuplicateShortCodes()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        // Create first URL
        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://existing.com",
            ShortCode = "existing",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });

        // Try to bulk create with duplicate short code
        var urls = new List<Url>
        {
            new Url { OriginalUrl = "https://new1.com", ShortCode = "new1", UserId = 1, CreatedAt = DateTime.UtcNow },
            new Url { OriginalUrl = "https://duplicate.com", ShortCode = "existing", UserId = 1, CreatedAt = DateTime.UtcNow },
            new Url { OriginalUrl = "https://new2.com", ShortCode = "new2", UserId = 1, CreatedAt = DateTime.UtcNow }
        };

        var result = await service.BulkCreateUrlsAsync(urls);

        Assert.Equal(2, result.SuccessCount);
        Assert.Single(result.Failures);
        Assert.Contains("existing", result.Failures.First().ShortCode);
        Assert.Contains("already taken", result.Failures.First().Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BulkCreateUrlsAsync_HandlesValidationErrors()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var urls = new List<Url>
        {
            new Url { OriginalUrl = "https://valid.com", ShortCode = "valid123", UserId = 1, CreatedAt = DateTime.UtcNow },
            new Url { OriginalUrl = "https://invalid.com", ShortCode = "ab", UserId = 1, CreatedAt = DateTime.UtcNow }, // Too short
            new Url { OriginalUrl = "https://reserved.com", ShortCode = "api", UserId = 1, CreatedAt = DateTime.UtcNow } // Reserved word
        };

        var result = await service.BulkCreateUrlsAsync(urls);

        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(2, result.Failures.Count());
        Assert.Contains(result.Failures, f => f.ShortCode == "ab");
        Assert.Contains(result.Failures, f => f.ShortCode == "api");
    }

    [Fact]
    public async Task BulkCreateUrlsAsync_MixedAutoAndCustomShortCodes()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var urls = new List<Url>
        {
            new Url { OriginalUrl = "https://auto1.com", UserId = 1, CreatedAt = DateTime.UtcNow }, // Auto
            new Url { OriginalUrl = "https://custom.com", ShortCode = "custom", UserId = 1, CreatedAt = DateTime.UtcNow }, // Custom
            new Url { OriginalUrl = "https://auto2.com", UserId = 1, CreatedAt = DateTime.UtcNow } // Auto
        };

        var result = await service.BulkCreateUrlsAsync(urls);

        Assert.Equal(3, result.SuccessCount);
        Assert.Empty(result.Failures);
        Assert.Contains(result.CreatedUrls, u => u.ShortCode == "custom");
        Assert.Equal(2, result.CreatedUrls.Count(u => u.ShortCode != "custom"));
    }

    [Fact]
    public async Task BulkCreateUrlsAsync_PreservesMetadata()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var expiry = DateTime.UtcNow.AddDays(7);
        var urls = new List<Url>
        {
            new Url
            {
                OriginalUrl = "https://example.com",
                Category = "Tech",
                Tags = "dotnet,api",
                Expiry = expiry,
                UserId = 1,
                CreatedAt = DateTime.UtcNow
            }
        };

        var result = await service.BulkCreateUrlsAsync(urls);

        Assert.Equal(1, result.SuccessCount);
        var created = result.CreatedUrls.First();
        Assert.Equal("Tech", created.Category);
        Assert.Equal("dotnet,api", created.Tags);
        Assert.Equal(expiry, created.Expiry);
    }

    [Fact]
    public async Task BulkCreateUrlsAsync_ReturnsEmptyForEmptyList()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var result = await service.BulkCreateUrlsAsync(new List<Url>());

        Assert.Equal(0, result.SuccessCount);
        Assert.Empty(result.Failures);
        Assert.Empty(result.CreatedUrls);
    }

    [Fact]
    public async Task BulkCreateUrlsAsync_HandlesLargeList()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var urls = Enumerable.Range(1, 100)
            .Select(i => new Url
            {
                OriginalUrl = $"https://example{i}.com",
                UserId = 1,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        var result = await service.BulkCreateUrlsAsync(urls);

        Assert.Equal(100, result.SuccessCount);
        Assert.Empty(result.Failures);
        Assert.Equal(100, result.CreatedUrls.Count());

        // All short codes should be unique
        var shortCodes = result.CreatedUrls.Select(u => u.ShortCode).ToList();
        Assert.Equal(100, shortCodes.Distinct().Count());
    }

    [Fact]
    public async Task BulkCreateUrlsAsync_PartialSuccessScenario()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var urls = new List<Url>
        {
            new Url { OriginalUrl = "https://valid1.com", ShortCode = "valid1", UserId = 1, CreatedAt = DateTime.UtcNow },
            new Url { OriginalUrl = "https://invalid.com", ShortCode = "x", UserId = 1, CreatedAt = DateTime.UtcNow }, // Too short
            new Url { OriginalUrl = "https://valid2.com", ShortCode = "valid2", UserId = 1, CreatedAt = DateTime.UtcNow },
            new Url { OriginalUrl = "https://reserved.com", ShortCode = "admin", UserId = 1, CreatedAt = DateTime.UtcNow } // Reserved
        };

        var result = await service.BulkCreateUrlsAsync(urls);

        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(2, result.Failures.Count());
        Assert.Equal(2, result.CreatedUrls.Count());
        Assert.Contains(result.CreatedUrls, u => u.ShortCode == "valid1");
        Assert.Contains(result.CreatedUrls, u => u.ShortCode == "valid2");
    }

    [Fact]
    public async Task BulkCreateUrlsAsync_AllFailuresScenario()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var urls = new List<Url>
        {
            new Url { OriginalUrl = "https://invalid1.com", ShortCode = "a", UserId = 1, CreatedAt = DateTime.UtcNow }, // Too short
            new Url { OriginalUrl = "https://invalid2.com", ShortCode = "api", UserId = 1, CreatedAt = DateTime.UtcNow }, // Reserved
            new Url { OriginalUrl = "https://invalid3.com", ShortCode = "xx", UserId = 1, CreatedAt = DateTime.UtcNow } // Too short
        };

        var result = await service.BulkCreateUrlsAsync(urls);

        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(3, result.Failures.Count());
        Assert.Empty(result.CreatedUrls);
    }
}
