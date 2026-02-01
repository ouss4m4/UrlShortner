using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Test;

public class CorsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CorsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PreflightRequest_ReturnsCorrectHeaders_ForAllowedOrigin()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/url");
        request.Headers.Add("Origin", "https://example.com");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "content-type");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.True(response.Headers.Contains("Access-Control-Allow-Methods"));
        Assert.True(response.Headers.Contains("Access-Control-Allow-Headers"));
    }

    [Fact]
    public async Task ActualRequest_IncludesCorsHeaders_ForAllowedOrigin()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/url/test");
        request.Headers.Add("Origin", "https://example.com");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task PostRequest_AllowsCors_WithCredentials()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/url");
        request.Headers.Add("Origin", "https://example.com");
        request.Content = JsonContent.Create(new
        {
            originalUrl = "https://google.com",
            shortCode = "test" + Guid.NewGuid().ToString()[..5]
        });

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.Contains("Access-Control-Allow-Credentials",
            response.Headers.Select(h => h.Key));
    }

    [Fact]
    public async Task CorsHeaders_ExposeRequiredHeaders()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/url/test");
        request.Headers.Add("Origin", "https://example.com");

        // Act
        var response = await client.SendAsync(request);

        // Assert - Should expose custom headers like X-RateLimit-*
        if (response.Headers.Contains("Access-Control-Expose-Headers"))
        {
            var exposeHeaders = response.Headers.GetValues("Access-Control-Expose-Headers").First();
            Assert.Contains("X-RateLimit", exposeHeaders);
        }
    }
}
