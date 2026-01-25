namespace UrlShortner.Models;

public class Visit
{
    public int Id { get; set; }
    public int UrlId { get; set; }
    public int? UserId { get; set; }  // optional, for registered users

    // Structured visit metadata (better than JSON string)
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Referrer { get; set; } = string.Empty;

    public DateTime VisitedAt { get; set; }
}
