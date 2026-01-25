using System.Threading.Tasks;

namespace UrlShortner.API.Services
{
    /// <summary>
    /// Service for looking up geographic information from IP addresses.
    /// Abstraction allows switching between providers (IP-API, MaxMind, etc.)
    /// </summary>
    public interface IGeoIpService
    {
        /// <summary>
        /// Get country code (ISO 3166-1 alpha-2) for an IP address.
        /// </summary>
        /// <param name="ipAddress">IP address to lookup (IPv4 or IPv6)</param>
        /// <returns>Two-letter country code (e.g., "US", "UK") or empty string if lookup fails</returns>
        Task<string> GetCountryCodeAsync(string ipAddress);
    }
}
