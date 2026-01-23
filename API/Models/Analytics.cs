namespace UrlShortner.API.Models;

public class Analytics
{
    public int Id { get; set; }
    public DateTime StatDate { get; set; }
    public int StatHour { get; set; }
    public int Visits { get; set; }
    public string Country { get; set; } = string.Empty;
}
