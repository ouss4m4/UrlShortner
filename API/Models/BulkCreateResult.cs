namespace UrlShortner.Models;

public class BulkCreateResult
{
    public int SuccessCount { get; set; }
    public IEnumerable<Url> CreatedUrls { get; set; } = new List<Url>();
    public IEnumerable<BulkCreateFailure> Failures { get; set; } = new List<BulkCreateFailure>();
}

public class BulkCreateFailure
{
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}
