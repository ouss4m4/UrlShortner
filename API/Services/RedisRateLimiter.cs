using StackExchange.Redis;

namespace UrlShortner.API.Services;

public class RedisRateLimiter : IRedisRateLimiter
{
    private readonly IConnectionMultiplexer _redis;
    private const string KeyPrefix = "ratelimit";

    public RedisRateLimiter(IConnectionMultiplexer redis)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    }

    public async Task<RateLimitResult> CheckRateLimitAsync(string clientId, string endpoint, int limit, int windowSeconds)
    {
        // Validate inputs
        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentException("Client ID cannot be null or empty", nameof(clientId));

        if (string.IsNullOrEmpty(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

        var db = _redis.GetDatabase();

        // Create unique key for this client + endpoint combination
        // Include current window to implement fixed window algorithm
        var currentWindow = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / windowSeconds;
        var key = $"{KeyPrefix}:{clientId}:{endpoint}:{currentWindow}";

        // Use Redis transaction to atomically increment and get TTL
        var transaction = db.CreateTransaction();

        // Increment counter
        var incrementTask = transaction.StringIncrementAsync(key);

        // Set expiration if this is the first request in the window
        var expireTask = transaction.KeyExpireAsync(key, TimeSpan.FromSeconds(windowSeconds));

        // Get TTL for RetryAfter calculation
        var ttlTask = transaction.KeyTimeToLiveAsync(key);

        // Execute transaction
        await transaction.ExecuteAsync();

        var currentCount = await incrementTask;
        var ttl = await ttlTask;

        // Calculate remaining requests
        var remaining = Math.Max(0, limit - (int)currentCount);

        // Check if limit exceeded
        if (currentCount > limit)
        {
            // Calculate retry after seconds
            var retryAfter = ttl.HasValue ? (int)ttl.Value.TotalSeconds : windowSeconds;

            return new RateLimitResult
            {
                IsAllowed = false,
                RemainingRequests = 0,
                RetryAfterSeconds = retryAfter
            };
        }

        return new RateLimitResult
        {
            IsAllowed = true,
            RemainingRequests = remaining,
            RetryAfterSeconds = 0
        };
    }
}
