using StackExchange.Redis;
using UrlShortner.API.Services;
using Xunit;

namespace Test;

[Collection("Redis Tests")]
public class RateLimiterTests : IAsyncLifetime
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IRedisRateLimiter _rateLimiter;

    public RateLimiterTests()
    {
        _redis = ConnectionMultiplexer.Connect("localhost:6379");
        _rateLimiter = new RedisRateLimiter(_redis);
    }

    public async Task InitializeAsync()
    {
        // Clean up before each test
        var db = _redis.GetDatabase();
        await db.ExecuteAsync("FLUSHDB");
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CheckRateLimit_AllowsRequestsWithinLimit()
    {
        // Arrange
        var clientId = "192.168.1.1";
        var endpoint = "create-url";
        var limit = 10;
        var windowSeconds = 60;

        // Act & Assert - First 10 requests should be allowed
        for (int i = 0; i < 10; i++)
        {
            var result = await _rateLimiter.CheckRateLimitAsync(clientId, endpoint, limit, windowSeconds);
            Assert.True(result.IsAllowed, $"Request {i + 1} should be allowed");
            Assert.Equal(limit - (i + 1), result.RemainingRequests);
        }
    }

    [Fact]
    public async Task CheckRateLimit_BlocksRequestsOverLimit()
    {
        // Arrange
        var clientId = "192.168.1.2";
        var endpoint = "create-url";
        var limit = 5;
        var windowSeconds = 60;

        // Act - Make 5 requests (at limit)
        for (int i = 0; i < 5; i++)
        {
            await _rateLimiter.CheckRateLimitAsync(clientId, endpoint, limit, windowSeconds);
        }

        // Assert - 6th request should be blocked
        var result = await _rateLimiter.CheckRateLimitAsync(clientId, endpoint, limit, windowSeconds);
        Assert.False(result.IsAllowed);
        Assert.Equal(0, result.RemainingRequests);
        Assert.True(result.RetryAfterSeconds > 0);
    }

    [Fact]
    public async Task CheckRateLimit_ReturnsCorrectRetryAfter()
    {
        // Arrange
        var clientId = "192.168.1.3";
        var endpoint = "create-url";
        var limit = 3;
        var windowSeconds = 60;

        // Act - Exhaust limit
        for (int i = 0; i < 3; i++)
        {
            await _rateLimiter.CheckRateLimitAsync(clientId, endpoint, limit, windowSeconds);
        }

        var result = await _rateLimiter.CheckRateLimitAsync(clientId, endpoint, limit, windowSeconds);

        // Assert - RetryAfter should be close to window duration
        Assert.False(result.IsAllowed);
        Assert.True(result.RetryAfterSeconds > 0);
        Assert.True(result.RetryAfterSeconds <= windowSeconds);
    }

    [Fact]
    public async Task CheckRateLimit_ResetsAfterWindowExpires()
    {
        // Arrange
        var clientId = "192.168.1.4";
        var endpoint = "create-url";
        var limit = 5;
        var windowSeconds = 2; // Short window for testing

        // Act - Exhaust limit
        for (int i = 0; i < 5; i++)
        {
            await _rateLimiter.CheckRateLimitAsync(clientId, endpoint, limit, windowSeconds);
        }

        var blockedResult = await _rateLimiter.CheckRateLimitAsync(clientId, endpoint, limit, windowSeconds);
        Assert.False(blockedResult.IsAllowed);

        // Wait for window to expire
        await Task.Delay(2100); // Wait slightly longer than window

        // Assert - Should allow requests again
        var allowedResult = await _rateLimiter.CheckRateLimitAsync(clientId, endpoint, limit, windowSeconds);
        Assert.True(allowedResult.IsAllowed);
        Assert.Equal(limit - 1, allowedResult.RemainingRequests);
    }

    [Fact]
    public async Task CheckRateLimit_IsolatesClientsByIP()
    {
        // Arrange
        var client1 = "192.168.1.5";
        var client2 = "192.168.1.6";
        var endpoint = "create-url";
        var limit = 3;
        var windowSeconds = 60;

        // Act - Exhaust limit for client1
        for (int i = 0; i < 3; i++)
        {
            await _rateLimiter.CheckRateLimitAsync(client1, endpoint, limit, windowSeconds);
        }

        var client1Blocked = await _rateLimiter.CheckRateLimitAsync(client1, endpoint, limit, windowSeconds);
        var client2Allowed = await _rateLimiter.CheckRateLimitAsync(client2, endpoint, limit, windowSeconds);

        // Assert
        Assert.False(client1Blocked.IsAllowed);
        Assert.True(client2Allowed.IsAllowed); // Different client should not be affected
    }

    [Fact]
    public async Task CheckRateLimit_IsolatesByEndpoint()
    {
        // Arrange
        var clientId = "192.168.1.7";
        var endpoint1 = "create-url";
        var endpoint2 = "bulk-create";
        var limit = 3;
        var windowSeconds = 60;

        // Act - Exhaust limit for endpoint1
        for (int i = 0; i < 3; i++)
        {
            await _rateLimiter.CheckRateLimitAsync(clientId, endpoint1, limit, windowSeconds);
        }

        var endpoint1Blocked = await _rateLimiter.CheckRateLimitAsync(clientId, endpoint1, limit, windowSeconds);
        var endpoint2Allowed = await _rateLimiter.CheckRateLimitAsync(clientId, endpoint2, limit, windowSeconds);

        // Assert
        Assert.False(endpoint1Blocked.IsAllowed);
        Assert.True(endpoint2Allowed.IsAllowed); // Different endpoint should not be affected
    }

    [Fact]
    public async Task CheckRateLimit_HandlesConcurrentRequests()
    {
        // Arrange
        var clientId = "192.168.1.8";
        var endpoint = "create-url";
        var limit = 10;
        var windowSeconds = 60;

        // Act - Make 15 concurrent requests
        var tasks = Enumerable.Range(0, 15)
            .Select(_ => _rateLimiter.CheckRateLimitAsync(clientId, endpoint, limit, windowSeconds))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert - Exactly 10 should be allowed, 5 blocked
        var allowedCount = results.Count(r => r.IsAllowed);
        var blockedCount = results.Count(r => !r.IsAllowed);

        Assert.Equal(10, allowedCount);
        Assert.Equal(5, blockedCount);
    }

    [Fact]
    public async Task CheckRateLimit_WorksAcrossMultipleInstances()
    {
        // Arrange - Simulate two server instances with separate rate limiter objects
        var instance1 = new RedisRateLimiter(_redis);
        var instance2 = new RedisRateLimiter(_redis);

        var clientId = "192.168.1.9";
        var endpoint = "create-url";
        var limit = 10;
        var windowSeconds = 60;

        // Act - Make 5 requests from instance1, 5 from instance2
        for (int i = 0; i < 5; i++)
        {
            await instance1.CheckRateLimitAsync(clientId, endpoint, limit, windowSeconds);
        }
        for (int i = 0; i < 5; i++)
        {
            await instance2.CheckRateLimitAsync(clientId, endpoint, limit, windowSeconds);
        }

        // Try one more from each instance
        var result1 = await instance1.CheckRateLimitAsync(clientId, endpoint, limit, windowSeconds);
        var result2 = await instance2.CheckRateLimitAsync(clientId, endpoint, limit, windowSeconds);

        // Assert - Both should be blocked because total is 10 across both instances
        Assert.False(result1.IsAllowed);
        Assert.False(result2.IsAllowed);
    }

    [Fact]
    public async Task CheckRateLimit_HandlesNullOrEmptyClientId()
    {
        // Arrange
        var endpoint = "create-url";
        var limit = 10;
        var windowSeconds = 60;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _rateLimiter.CheckRateLimitAsync(null!, endpoint, limit, windowSeconds));

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _rateLimiter.CheckRateLimitAsync("", endpoint, limit, windowSeconds));
    }

    [Fact]
    public async Task CheckRateLimit_HandlesNullOrEmptyEndpoint()
    {
        // Arrange
        var clientId = "192.168.1.10";
        var limit = 10;
        var windowSeconds = 60;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _rateLimiter.CheckRateLimitAsync(clientId, null!, limit, windowSeconds));

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _rateLimiter.CheckRateLimitAsync(clientId, "", limit, windowSeconds));
    }
}
