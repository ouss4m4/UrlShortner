using System.Text.RegularExpressions;

namespace UrlShortner.API.Services;

public class UrlValidator : IUrlValidator
{
    private const int MaxUrlLength = 2048;
    private const int MinShortCodeLength = 3;
    private const int MaxShortCodeLength = 20;

    private static readonly HashSet<string> ReservedWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin", "api", "swagger", "health", "login", "register", "logout",
        "user", "users", "url", "urls", "analytics", "dashboard", "settings"
    };

    private static readonly Regex AlphanumericRegex = new("^[a-zA-Z0-9]+$", RegexOptions.Compiled);

    public ValidationResult ValidateUrl(string url)
    {
        // Check for null/empty
        if (string.IsNullOrWhiteSpace(url))
        {
            return ValidationResult.Failure("URL cannot be empty");
        }

        // Check length
        if (url.Length > MaxUrlLength)
        {
            return ValidationResult.Failure($"URL exceeds maximum length of {MaxUrlLength} characters");
        }

        // Check for control characters
        if (url.Any(c => char.IsControl(c)))
        {
            return ValidationResult.Failure("Invalid URL format: contains control characters");
        }

        // Quick check for localhost before parsing
        if (url.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("127.0.0.1") ||
            url.Contains("::1") ||
            url.Contains("0.0.0.0"))
        {
            return ValidationResult.Failure("Localhost and private IP addresses are not allowed");
        }

        // Try to parse as URI
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return ValidationResult.Failure("Invalid URL format");
        }

        // Check protocol
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return ValidationResult.Failure("URL must use HTTP or HTTPS protocol");
        }

        // Check for private IP addresses
        if (IsPrivateIpAddress(uri.Host))
        {
            return ValidationResult.Failure("Localhost and private IP addresses are not allowed");
        }

        return ValidationResult.Success();
    }

    public ValidationResult ValidateShortCode(string shortCode)
    {
        // Check for null/empty
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            return ValidationResult.Failure($"Short code must be between {MinShortCodeLength} and {MaxShortCodeLength} characters");
        }

        // Check length
        if (shortCode.Length < MinShortCodeLength || shortCode.Length > MaxShortCodeLength)
        {
            return ValidationResult.Failure($"Short code must be between {MinShortCodeLength} and {MaxShortCodeLength} characters");
        }

        // Check alphanumeric
        if (!AlphanumericRegex.IsMatch(shortCode))
        {
            return ValidationResult.Failure("Short code can only contain alphanumeric characters (a-z, A-Z, 0-9)");
        }

        // Check reserved words
        if (ReservedWords.Contains(shortCode))
        {
            return ValidationResult.Failure($"Short code '{shortCode}' is reserved and cannot be used");
        }

        return ValidationResult.Success();
    }

    private static bool IsLocalhost(string host)
    {
        return host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
               host.Equals("127.0.0.1") ||
               host.Equals("[::1]") ||
               host.Equals("::1") ||
               host.Equals("0.0.0.0");
    }

    private static bool IsPrivateIpAddress(string host)
    {
        // Try to parse as IP address
        if (!System.Net.IPAddress.TryParse(host, out var ipAddress))
        {
            return false;
        }

        // Check if it's IPv4
        if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            var bytes = ipAddress.GetAddressBytes();

            // Check for private ranges:
            // 10.0.0.0/8
            if (bytes[0] == 10)
                return true;

            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                return true;

            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
                return true;
        }

        return false;
    }
}
