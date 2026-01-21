namespace UrlShortner.Models;

public class Visit
{
    public int Id { get; set; }
    public int UrlId { get; set; }
    public string Metadata { get; set; } = string.Empty; // user agent, IP, etc.
    public DateTime VisitedAt { get; set; }
}
