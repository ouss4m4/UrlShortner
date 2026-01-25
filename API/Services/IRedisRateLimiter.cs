namespace UrlShortner.API.Services;

public interface IRedisRateLimiter
{
    Task<RateLimitResult> CheckRateLimitAsync(string clientId, string endpoint, int limit, int windowSeconds);
}
