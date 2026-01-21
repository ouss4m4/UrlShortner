namespace UrlShortner.Models;

public class Url
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Long { get; set; } = string.Empty;
    public string Short { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? Expiry { get; set; }
}
