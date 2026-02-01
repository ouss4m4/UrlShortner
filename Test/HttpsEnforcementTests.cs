using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace Test;

public class HttpsEnforcementTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HttpsEnforcementTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HttpRequest_RedirectsToHttps_InProduction()
    {
        // Arrange
        var client = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        // Act
        var response = await client.GetAsync("http://localhost/api/url/test");

        // Assert
        Assert.Equal(HttpStatusCode.PermanentRedirect, response.StatusCode);
        Assert.True(response.Headers.Location?.ToString().StartsWith("https://"));
    }

    [Fact]
    public async Task HttpsRequest_ProcessedNormally_InProduction()
    {
        // Arrange
        var client = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
            })
            .CreateClient();

        // Act - HTTPS request should work normally
        var response = await client.GetAsync("/nonexistent");

        // Assert - Should get 404 (URL not found) not a redirect
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task HttpRequest_AllowedInDevelopment()
    {
        // Arrange
        var client = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        // Act
        var response = await client.GetAsync("http://localhost/api/url/test");

        // Assert - Should not redirect in development
        Assert.NotEqual(HttpStatusCode.PermanentRedirect, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.MovedPermanently, response.StatusCode);
    }
}
