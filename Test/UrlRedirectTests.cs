using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Data;
using UrlShortner.Models;
using UrlShortner.API.Services;
using Xunit;

namespace Test;

public class UrlRedirectTests
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
    public async Task CanExpandShortCodeToOriginalUrl()
    {
        using var context = GetInMemoryDbContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());
        
        var url = new Url
        {
            OriginalUrl = "https://www.example.com/very/long/path",
            ShortCode = "",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };
        
        var created = await service.CreateUrlAsync(url);
        Assert.NotEmpty(created.ShortCode);
        
        var retrieved = await service.GetUrlByShortCodeAsync(created.ShortCode);
        
        Assert.NotNull(retrieved);
        Assert.Equal("https://www.example.com/very/long/path", retrieved.OriginalUrl);
        Assert.Equal(created.Id, retrieved.Id);
    }

    [Fact]
    public async Task ReturnsNullForNonExistentShortCode()
    {
        using var context = GetInMemoryDbContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());
        
        var retrieved = await service.GetUrlByShortCodeAsync("notexist");
        
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task CanRetrieveMultipleUrlsByDifferentShortCodes()
    {
        using var context = GetInMemoryDbContext();
        var service = new UrlService(context, GetShortCodeGenerator(), new UrlValidator());
        
        var url1 = await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://site1.com",
            ShortCode = "",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });
        
        var url2 = await service.CreateUrlAsync(new Url
        {
            OriginalUrl = "https://site2.com",
            ShortCode = "",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        });
        
        Assert.NotEqual(url1.ShortCode, url2.ShortCode);
        
        var retrieved1 = await service.GetUrlByShortCodeAsync(url1.ShortCode);
        var retrieved2 = await service.GetUrlByShortCodeAsync(url2.ShortCode);
        
        Assert.NotNull(retrieved1);
        Assert.NotNull(retrieved2);
        Assert.Equal("https://site1.com", retrieved1.OriginalUrl);
        Assert.Equal("https://site2.com", retrieved2.OriginalUrl);
    }
}
