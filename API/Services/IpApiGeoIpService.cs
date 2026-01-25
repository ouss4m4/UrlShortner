using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace UrlShortner.API.Services
{
    /// <summary>
    /// GeoIP service implementation using ip-api.com (free tier: 45 req/min)
    /// To switch to MaxMind or another provider, create a new implementation of IGeoIpService
    /// </summary>
    public class IpApiGeoIpService : IGeoIpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IpApiGeoIpService> _logger;

        public IpApiGeoIpService(HttpClient httpClient, ILogger<IpApiGeoIpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetCountryCodeAsync(string ipAddress)
        {
            // Skip lookup for local/invalid IPs
            if (string.IsNullOrWhiteSpace(ipAddress) ||
                ipAddress == "unknown" ||
                ipAddress == "::1" ||
                ipAddress.StartsWith("127.") ||
                ipAddress.StartsWith("192.168.") ||
                ipAddress.StartsWith("10."))
            {
                return string.Empty;
            }

            try
            {
                // IP-API free endpoint: http://ip-api.com/json/{ip}?fields=countryCode
                var response = await _httpClient.GetAsync($"http://ip-api.com/json/{ipAddress}?fields=status,countryCode");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GeoIP lookup failed for {IpAddress}: HTTP {StatusCode}", ipAddress, response.StatusCode);
                    return string.Empty;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<IpApiResponse>(json);

                if (result?.Status == "success" && !string.IsNullOrWhiteSpace(result.CountryCode))
                {
                    return result.CountryCode;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during GeoIP lookup for {IpAddress}", ipAddress);
                return string.Empty;
            }
        }

        private class IpApiResponse
        {
            public string? Status { get; set; }
            public string? CountryCode { get; set; }
        }
    }
}
