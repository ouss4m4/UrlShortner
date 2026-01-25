using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Controllers;
using UrlShortner.API.Data;
using UrlShortner.Models;
using UrlShortner.API.Services;
using Xunit;

namespace Test;

public class UrlControllerTests
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
    public async Task CreateReturns409ConflictWhenShortCodeExists()
    {
        using var context = GetInMemoryDbContext();
        var service = new UrlService(context, GetShortCodeGenerator());
        var controller = new UrlController(service);
        
        var url1 = new Url
        {
            OriginalUrl = "https://facebook.com",
            ShortCode = "fb",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };
        
        var result1 = await controller.Create(url1);
        Assert.IsType<CreatedAtActionResult>(result1);
        
        var url2 = new Url
        {
            OriginalUrl = "https://facebook.com/different",
            ShortCode = "fb",
            UserId = 2,
            CreatedAt = DateTime.UtcNow
        };
        
        var result2 = await controller.Create(url2);
        
        var conflictResult = Assert.IsType<ConflictObjectResult>(result2);
        Assert.Equal(409, conflictResult.StatusCode);
    }

    [Fact]
    public async Task CreateReturns201CreatedWhenShortCodeIsUnique()
    {
        using var context = GetInMemoryDbContext();
        var service = new UrlService(context, GetShortCodeGenerator());
        var controller = new UrlController(service);
        
        var url = new Url
        {
            OriginalUrl = "https://example.com",
            ShortCode = "unique123",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await controller.Create(url);
        
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        
        var createdUrl = Assert.IsType<Url>(createdResult.Value);
        Assert.Equal("unique123", createdUrl.ShortCode);
    }

    [Fact]
    public async Task CreateAutoGeneratesShortCodeWhenEmpty()
    {
        using var context = GetInMemoryDbContext();
        var service = new UrlService(context, GetShortCodeGenerator());
        var controller = new UrlController(service);
        
        var url = new Url
        {
            OriginalUrl = "https://example.com",
            ShortCode = "",
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await controller.Create(url);
        
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var createdUrl = Assert.IsType<Url>(createdResult.Value);
        
        Assert.NotEmpty(createdUrl.ShortCode);
        Assert.Matches(@"^[0-9a-zA-Z]+$", createdUrl.ShortCode);
    }
}
