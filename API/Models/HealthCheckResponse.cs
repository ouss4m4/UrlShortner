namespace UrlShortner.Models;

public class HealthCheckResponse
{
    public string Status { get; set; } = "Healthy";
    public string InstanceId { get; set; } = Environment.MachineName;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Details { get; set; }
}
