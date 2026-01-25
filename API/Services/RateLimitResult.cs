namespace UrlShortner.API.Services;

public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public int RemainingRequests { get; set; }
    public int RetryAfterSeconds { get; set; }
}
