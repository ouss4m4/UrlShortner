using System.Text.RegularExpressions;

namespace UrlShortner.API.Services;

public class UrlValidator : IUrlValidator
{
    private const int MaxUrlLength = 2048;
    private const int MinShortCodeLength = 3;
    private const int MaxShortCodeLength = 20;
    private static readonly Regex DomainLabelRegex = new("^[a-zA-Z0-9-]{1,63}$", RegexOptions.Compiled);
    private static readonly Regex TldRegex = new("^[a-zA-Z]{2,63}$", RegexOptions.Compiled);

    private static readonly HashSet<string> ReservedWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin", "api", "swagger", "health", "login", "register", "logout",
        "user", "users", "url", "urls", "analytics", "dashboard", "settings"
    };

    private static readonly HashSet<string> KnownTlds = new(StringComparer.OrdinalIgnoreCase)
    {
        "com", "net", "org", "io", "co", "dev", "app", "ai", "me", "xyz", "info", "biz",
        "edu", "gov", "mil", "us", "uk", "ca", "de", "fr", "es", "it", "nl", "se",
        "no", "fi", "dk", "ch", "at", "ie", "be", "pl", "cz", "sk", "hu", "ro",
        "pt", "gr", "tr", "il", "sa", "ae", "in", "pk", "bd", "lk", "np",
        "cn", "jp", "kr", "sg", "hk", "tw", "vn", "th", "my", "id", "ph",
        "au", "nz", "br", "mx", "ar", "cl", "co", "pe", "uy", "za", "ng", "ke",
        "gg", "gg", "to", "tv", "site", "online", "store", "tech", "cloud", "studio"
    };

    private static readonly Regex AlphanumericRegex = new("^[a-zA-Z0-9]+$", RegexOptions.Compiled);

    public ValidationResult ValidateUrl(string url)
    {
        // Check for null/empty
        if (string.IsNullOrWhiteSpace(url))
        {
            return ValidationResult.Failure("URL cannot be empty");
        }

        url = url.Trim();

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

        // Block dangerous schemes early
        if (url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            return ValidationResult.Failure("URL must use HTTP or HTTPS protocol");
        }

        // Early check for localhost patterns (some may fail URI parsing on certain platforms)
        if (url.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("127.0.0.1") ||
            url.Contains("0.0.0.0") ||
            url.Contains("::1"))
        {
            return ValidationResult.Failure("Localhost and private IP addresses are not allowed");
        }

        // Normalize: prepend https:// if no scheme present
        var normalizedUrl = url.Contains("://", StringComparison.Ordinal) ? url : $"https://{url}";

        // Parse and validate
        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri))
        {
            return ValidationResult.Failure("Invalid URL format");
        }

        // Protocol must be http or https
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return ValidationResult.Failure("URL must use HTTP or HTTPS protocol");
        }

        // Check for private IP ranges (parsed IPs)
        if (IsPrivateIpAddress(uri.Host))
        {
            return ValidationResult.Failure("Localhost and private IP addresses are not allowed");
        }

        // Validate domain structure and TLD
        if (!IsValidPublicHost(uri.Host))
        {
            return ValidationResult.Failure("Invalid URL format: invalid or unsupported domain");
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

    private static bool IsValidPublicHost(string host)
    {
        // Check if it's an IP address (public IPs are allowed, private already checked above)
        if (System.Net.IPAddress.TryParse(host, out _))
        {
            return true;
        }

        // Must be a DNS hostname - validate structure
        var hostType = Uri.CheckHostName(host);
        if (hostType != UriHostNameType.Dns)
        {
            return false;
        }

        // Validate domain labels
        var labels = host.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (labels.Length < 2)
        {
            return false;
        }

        foreach (var label in labels)
        {
            if (!DomainLabelRegex.IsMatch(label))
            {
                return false;
            }

            if (label.StartsWith('-') || label.EndsWith('-'))
            {
                return false;
            }
        }

        // Validate TLD
        var tld = labels[^1];
        if (!TldRegex.IsMatch(tld))
        {
            return false;
        }

        return KnownTlds.Contains(tld);
    }
}
