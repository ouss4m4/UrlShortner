namespace UrlShortner.API.Models;

public class Url
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;  // previously "Long"
    public string ShortCode { get; set; } = string.Empty;     // previously "Short"
    public DateTime CreatedAt { get; set; }
    public DateTime? Expiry { get; set; }
}
