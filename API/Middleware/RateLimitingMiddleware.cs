using UrlShortner.API.Services;

namespace UrlShortner.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRedisRateLimiter _rateLimiter;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    // Default rate limits per endpoint
    private readonly Dictionary<string, (int limit, int windowSeconds)> _endpointLimits = new()
    {
        { "POST:/api/url", (10, 60) },           // 10 URL creations per minute
        { "POST:/api/url/bulk", (5, 60) },       // 5 bulk operations per minute
        { "GET:/api/analytics", (100, 60) }      // 100 analytics requests per minute
    };

    public RateLimitingMiddleware(
        RequestDelegate next,
        IRedisRateLimiter rateLimiter,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get client identifier (IP address)
        var clientId = GetClientIdentifier(context);

        // Get endpoint key - normalize paths with route parameters
        var path = context.Request.Path.Value ?? "/";
        var endpointKey = $"{context.Request.Method}:{path}";

        // Try exact match first, then prefix match for parameterized routes
        (int limit, int windowSeconds)? limits = null;
        if (_endpointLimits.TryGetValue(endpointKey, out var exactLimits))
        {
            limits = exactLimits;
        }
        else
        {
            // Check for prefix matches (e.g., "GET:/api/analytics" matches "GET:/api/analytics/url/123")
            foreach (var (pattern, patternLimits) in _endpointLimits)
            {
                if (endpointKey.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    limits = patternLimits;
                    endpointKey = pattern; // Use the pattern as the rate limit key to group all matching requests
                    break;
                }
            }
        }

        // Check if this endpoint has rate limiting configured
        if (limits.HasValue)
        {
            var (limit, windowSeconds) = limits.Value;

            try
            {
                // Check rate limit
                var result = await _rateLimiter.CheckRateLimitAsync(
                    clientId,
                    endpointKey,
                    limit,
                    windowSeconds);

                // Add rate limit headers
                context.Response.Headers.Append("X-RateLimit-Limit", limit.ToString());
                context.Response.Headers.Append("X-RateLimit-Remaining", result.RemainingRequests.ToString());

                if (!result.IsAllowed)
                {
                    // Rate limit exceeded
                    context.Response.Headers.Append("Retry-After", result.RetryAfterSeconds.ToString());
                    context.Response.StatusCode = 429; // Too Many Requests

                    _logger.LogWarning(
                        "Rate limit exceeded for client {ClientId} on endpoint {Endpoint}. " +
                        "Limit: {Limit}, Window: {Window}s, RetryAfter: {RetryAfter}s",
                        clientId, endpointKey, limit, windowSeconds, result.RetryAfterSeconds);

                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Rate limit exceeded",
                        message = $"Too many requests. Please try again in {result.RetryAfterSeconds} seconds.",
                        retryAfter = result.RetryAfterSeconds
                    });

                    return; // Short-circuit the pipeline
                }
            }
            catch (Exception ex)
            {
                // Log error but don't block the request if rate limiting fails
                _logger.LogError(ex, "Rate limiting check failed for {ClientId} on {Endpoint}. Allowing request.",
                    clientId, endpointKey);
            }
        }

        // Continue to next middleware
        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get real IP from X-Forwarded-For header (for proxies/load balancers)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP if multiple are present
            var ips = forwardedFor.Split(',');
            return ips[0].Trim();
        }

        // Fall back to direct connection IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
