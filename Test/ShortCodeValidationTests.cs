using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Data;
using UrlShortner.API.Services;
using UrlShortner.Models;
using Xunit;

namespace Test;

public class ShortCodeValidationTests
{
    private UrlShortnerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<UrlShortnerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new UrlShortnerDbContext(options);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    public async Task CreateUrlAsync_ThrowsException_WhenShortCodeIsTooShort(string shortCode)
    {
        // Arrange
        var context = CreateInMemoryContext();
        var shortCodeGenerator = new ShortCodeGenerator();
        var urlService = new UrlService(context, shortCodeGenerator, null);

        var url = new Url
        {
            OriginalUrl = "https://example.com",
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await urlService.CreateUrlAsync(url)
        );
        Assert.Contains("at least 3 characters", exception.Message);
    }

    [Fact]
    public async Task CreateUrlAsync_ThrowsException_WhenShortCodeIsTooLong()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var shortCodeGenerator = new ShortCodeGenerator();
        var urlService = new UrlService(context, shortCodeGenerator, null);

        var url = new Url
        {
            OriginalUrl = "https://example.com",
            ShortCode = "thisshortcodeiswaytoolongandshouldfail",
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await urlService.CreateUrlAsync(url)
        );
        Assert.Contains("maximum of 20 characters", exception.Message);
    }

    [Theory]
    [InlineData("hello world")]  // space
    [InlineData("hello-world")]  // hyphen
    [InlineData("hello_world")]  // underscore
    [InlineData("hello.world")]  // dot
    [InlineData("hello@world")]  // special char
    [InlineData("hello/world")]  // slash
    [InlineData("hello\\world")] // backslash
    public async Task CreateUrlAsync_ThrowsException_WhenShortCodeContainsInvalidCharacters(string shortCode)
    {
        // Arrange
        var context = CreateInMemoryContext();
        var shortCodeGenerator = new ShortCodeGenerator();
        var urlService = new UrlService(context, shortCodeGenerator, null);

        var url = new Url
        {
            OriginalUrl = "https://example.com",
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await urlService.CreateUrlAsync(url)
        );
        Assert.Contains("alphanumeric", exception.Message);
    }

    [Theory]
    [InlineData("api")]
    [InlineData("swagger")]
    [InlineData("admin")]
    [InlineData("health")]
    [InlineData("analytics")]
    public async Task CreateUrlAsync_ThrowsException_WhenShortCodeIsReserved(string shortCode)
    {
        // Arrange
        var context = CreateInMemoryContext();
        var shortCodeGenerator = new ShortCodeGenerator();
        var urlService = new UrlService(context, shortCodeGenerator, null);

        var url = new Url
        {
            OriginalUrl = "https://example.com",
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await urlService.CreateUrlAsync(url)
        );
        Assert.Contains("reserved", exception.Message);
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("GitHub")]
    [InlineData("MyURL")]
    [InlineData("test2026")]
    public async Task CreateUrlAsync_AcceptsValidShortCodes(string shortCode)
    {
        // Arrange
        var context = CreateInMemoryContext();
        var shortCodeGenerator = new ShortCodeGenerator();
        var urlService = new UrlService(context, shortCodeGenerator, null);

        var url = new Url
        {
            OriginalUrl = "https://example.com",
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await urlService.CreateUrlAsync(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(shortCode, result.ShortCode);
    }

    [Fact]
    public async Task CreateUrlAsync_IsCaseSensitive()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var shortCodeGenerator = new ShortCodeGenerator();
        var urlService = new UrlService(context, shortCodeGenerator, null);

        var url1 = new Url
        {
            OriginalUrl = "https://example1.com",
            ShortCode = "ABC",
            CreatedAt = DateTime.UtcNow
        };

        var url2 = new Url
        {
            OriginalUrl = "https://example2.com",
            ShortCode = "abc",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result1 = await urlService.CreateUrlAsync(url1);
        var result2 = await urlService.CreateUrlAsync(url2);

        // Assert - Both should succeed (case sensitive)
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal("ABC", result1.ShortCode);
        Assert.Equal("abc", result2.ShortCode);
    }
}
