using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Services;
using UrlShortner.API.Data;
using UrlShortner.Models;
using Xunit;

namespace Test;

public class UrlCategoryTagTests
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

    // Category Tests
    [Fact]
    public async Task CreateUrlAsync_WithCategory_StoresCategory()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var url = new Url
        {
            OriginalUrl = "https://example.com",
            Category = "Technology",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        var created = await service.CreateUrlAsync(url);

        Assert.NotNull(created);
        Assert.Equal("Technology", created.Category);
    }

    [Fact]
    public async Task CreateUrlAsync_WithoutCategory_AllowsNullCategory()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var url = new Url
        {
            OriginalUrl = "https://example.com",
            Category = null,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        var created = await service.CreateUrlAsync(url);

        Assert.NotNull(created);
        Assert.Null(created.Category);
    }

    [Fact]
    public async Task GetUrlsByCategoryAsync_ReturnsUrlsInCategory()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        // Create URLs in different categories
        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://tech1.com",
            Category = "Technology",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://tech2.com",
            Category = "Technology",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://news1.com",
            Category = "News",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });

        var techUrls = await service.GetUrlsByCategoryAsync("Technology", 1);

        Assert.Equal(2, techUrls.Count());
        Assert.All(techUrls, u => Assert.Equal("Technology", u.Category));
    }

    [Fact]
    public async Task GetUrlsByCategoryAsync_IsCaseInsensitive()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://tech1.com",
            Category = "Technology",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });

        var urlsLower = await service.GetUrlsByCategoryAsync("technology", 1);
        var urlsUpper = await service.GetUrlsByCategoryAsync("TECHNOLOGY", 1);

        Assert.Single(urlsLower);
        Assert.Single(urlsUpper);
    }

    [Fact]
    public async Task GetUrlsByCategoryAsync_FiltersExpiredUrls()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://tech1.com",
            Category = "Technology",
            Expiry = DateTime.UtcNow.AddDays(1), // Not expired
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://tech2.com",
            Category = "Technology",
            Expiry = DateTime.UtcNow.AddDays(-1), // Expired
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });

        var urls = await service.GetUrlsByCategoryAsync("Technology", 1);

        Assert.Single(urls);
        Assert.Contains(urls, u => u.OriginalUrl == "https://tech1.com");
    }

    // Tags Tests
    [Fact]
    public async Task CreateUrlAsync_WithTags_StoresTags()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var url = new Url
        {
            OriginalUrl = "https://example.com",
            Tags = "dotnet,csharp,webapi",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        var created = await service.CreateUrlAsync(url);

        Assert.NotNull(created);
        Assert.Equal("dotnet,csharp,webapi", created.Tags);
    }

    [Fact]
    public async Task CreateUrlAsync_WithoutTags_AllowsNullTags()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var url = new Url
        {
            OriginalUrl = "https://example.com",
            Tags = null,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        var created = await service.CreateUrlAsync(url);

        Assert.NotNull(created);
        Assert.Null(created.Tags);
    }

    [Fact]
    public async Task GetUrlsByTagAsync_ReturnsUrlsWithTag()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://dotnet1.com",
            Tags = "dotnet,csharp",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://dotnet2.com",
            Tags = "dotnet,webapi",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://python1.com",
            Tags = "python,flask",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });

        var dotnetUrls = await service.GetUrlsByTagAsync("dotnet", 1);

        Assert.Equal(2, dotnetUrls.Count());
        Assert.All(dotnetUrls, u => Assert.Contains("dotnet", u.Tags));
    }

    [Fact]
    public async Task GetUrlsByTagAsync_IsCaseInsensitive()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://dotnet1.com",
            Tags = "DotNet,CSharp",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });

        var urlsLower = await service.GetUrlsByTagAsync("dotnet", 1);
        var urlsUpper = await service.GetUrlsByTagAsync("DOTNET", 1);

        Assert.Single(urlsLower);
        Assert.Single(urlsUpper);
    }

    [Fact]
    public async Task GetUrlsByTagAsync_MatchesPartialTag()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://example1.com",
            Tags = "csharp,dotnet,webapi",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://example2.com",
            Tags = "javascript,nodejs",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });

        var csharpUrls = await service.GetUrlsByTagAsync("csharp", 1);

        Assert.Single(csharpUrls);
        Assert.Contains("csharp", csharpUrls.First().Tags);
    }

    [Fact]
    public async Task GetUrlsByTagAsync_FiltersExpiredUrls()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://dotnet1.com",
            Tags = "dotnet,csharp",
            Expiry = DateTime.UtcNow.AddDays(1), // Not expired
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://dotnet2.com",
            Tags = "dotnet,csharp",
            Expiry = DateTime.UtcNow.AddDays(-1), // Expired
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });

        var urls = await service.GetUrlsByTagAsync("dotnet", 1);

        Assert.Single(urls);
        Assert.Contains(urls, u => u.OriginalUrl == "https://dotnet1.com");
    }

    // Combined Category & Tags Tests
    [Fact]
    public async Task CreateUrlAsync_WithCategoryAndTags_StoresBoth()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        var url = new Url
        {
            OriginalUrl = "https://example.com",
            Category = "Technology",
            Tags = "dotnet,csharp,webapi",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        var created = await service.CreateUrlAsync(url);

        Assert.NotNull(created);
        Assert.Equal("Technology", created.Category);
        Assert.Equal("dotnet,csharp,webapi", created.Tags);
    }

    [Fact]
    public async Task GetUrlsByCategoryAsync_ReturnsOnlyUserUrls()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://user1tech.com",
            Category = "Technology",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://user2tech.com",
            Category = "Technology",
            UserId = 2,
            CreatedAt = DateTime.UtcNow
        });

        var user1Urls = await service.GetUrlsByCategoryAsync("Technology", 1);

        Assert.Single(user1Urls);
        Assert.All(user1Urls, u => Assert.Equal(1, u.UserId));
    }

    [Fact]
    public async Task GetUrlsByTagAsync_ReturnsOnlyUserUrls()
    {
        await using var context = GetInMemoryContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());

        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://user1dotnet.com",
            Tags = "dotnet,csharp",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://user2dotnet.com",
            Tags = "dotnet,csharp",
            UserId = 2,
            CreatedAt = DateTime.UtcNow
        });

        var user1Urls = await service.GetUrlsByTagAsync("dotnet", 1);

        Assert.Single(user1Urls);
        Assert.All(user1Urls, u => Assert.Equal(1, u.UserId));
    }
}
