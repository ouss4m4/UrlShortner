using Xunit;
using UrlShortner.API.Services;

namespace Test;

public class UrlValidationTests
{
    private readonly IUrlValidator _validator;

    public UrlValidationTests()
    {
        _validator = new UrlValidator();
    }

    [Theory]
    [InlineData("https://www.google.com")]
    [InlineData("google.com")]
    [InlineData("www.google.com")]
    [InlineData("http://example.com")]
    [InlineData("https://subdomain.example.com/path")]
    [InlineData("https://example.com:8080")]
    [InlineData("https://example.com/path?query=value")]
    [InlineData("https://example.com/path#fragment")]
    public void ValidateUrl_ReturnsTrue_ForValidUrls(string url)
    {
        // Act
        var result = _validator.ValidateUrl(url);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Theory]
    [InlineData("ftp://example.com", "URL must use HTTP or HTTPS protocol")]
    [InlineData("javascript:alert(1)", "URL must use HTTP or HTTPS protocol")]
    [InlineData("data:text/html,<script>alert(1)</script>", "URL must use HTTP or HTTPS protocol")]
    [InlineData("file:///etc/passwd", "URL must use HTTP or HTTPS protocol")]
    public void ValidateUrl_ReturnsFalse_ForInvalidProtocols(string url, string expectedError)
    {
        // Act
        var result = _validator.ValidateUrl(url);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(expectedError, result.ErrorMessage);
    }

    [Theory]
    [InlineData("", "URL cannot be empty")]
    [InlineData(null, "URL cannot be empty")]
    [InlineData("   ", "URL cannot be empty")]
    public void ValidateUrl_ReturnsFalse_ForEmptyUrls(string url, string expectedError)
    {
        // Act
        var result = _validator.ValidateUrl(url);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(expectedError, result.ErrorMessage);
    }

    [Fact]
    public void ValidateUrl_ReturnsFalse_ForUrlExceedingMaxLength()
    {
        // Arrange - URL longer than 2048 characters
        var longUrl = "https://example.com/" + new string('a', 2100);

        // Act
        var result = _validator.ValidateUrl(longUrl);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("URL exceeds maximum length of 2048 characters", result.ErrorMessage);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("https://")]
    [InlineData("://example.com")]
    public void ValidateUrl_ReturnsFalse_ForMalformedUrls(string url)
    {
        // Act
        var result = _validator.ValidateUrl(url);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Invalid URL format", result.ErrorMessage);
    }

    [Theory]
    [InlineData("https://somegoh.aeklgh.egq.qe/")]
    [InlineData("somegoh.aeklgh.egq.qe")]
    public void ValidateUrl_ReturnsFalse_ForUnsupportedDomains(string url)
    {
        // Act
        var result = _validator.ValidateUrl(url);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Invalid URL format: invalid or unsupported domain", result.ErrorMessage);
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("127.0.0.1")]
    [InlineData("0.0.0.0")]
    [InlineData("::1")]
    public void ValidateUrl_ReturnsFalse_ForLocalhostUrls(string host)
    {
        // Arrange
        var url = $"https://{host}";

        // Act
        var result = _validator.ValidateUrl(url);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Localhost and private IP addresses are not allowed", result.ErrorMessage);
    }

    [Theory]
    [InlineData("10.0.0.1")]
    [InlineData("172.16.0.1")]
    [InlineData("192.168.1.1")]
    public void ValidateUrl_ReturnsFalse_ForPrivateIpAddresses(string ip)
    {
        // Arrange
        var url = $"https://{ip}";

        // Act
        var result = _validator.ValidateUrl(url);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Localhost and private IP addresses are not allowed", result.ErrorMessage);
    }

    [Fact]
    public void ValidateUrl_AllowsMaxLengthUrl()
    {
        // Arrange - Exactly 2048 characters
        var maxUrl = "https://example.com/" + new string('a', 2048 - "https://example.com/".Length);

        // Act
        var result = _validator.ValidateUrl(maxUrl);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("https://example.com/\0null")]
    [InlineData("https://example.com/\r\ninjection")]
    public void ValidateUrl_ReturnsFalse_ForUrlsWithControlCharacters(string url)
    {
        // Act
        var result = _validator.ValidateUrl(url);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Invalid URL format", result.ErrorMessage);
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("https://example.com:443")]
    [InlineData("http://example.com:80")]
    public void ValidateUrl_ReturnsTrue_ForStandardPorts(string url)
    {
        // Act
        var result = _validator.ValidateUrl(url);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    [InlineData("")]
    public void ValidateShortCode_ReturnsFalse_ForTooShort(string shortCode)
    {
        // Act
        var result = _validator.ValidateShortCode(shortCode);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Short code must be between 3 and 20 characters", result.ErrorMessage);
    }

    [Fact]
    public void ValidateShortCode_ReturnsFalse_ForTooLong()
    {
        // Arrange
        var shortCode = new string('a', 21);

        // Act
        var result = _validator.ValidateShortCode(shortCode);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Short code must be between 3 and 20 characters", result.ErrorMessage);
    }

    [Theory]
    [InlineData("abc@123")]
    [InlineData("test-code")]
    [InlineData("test_code")]
    [InlineData("test code")]
    [InlineData("test.code")]
    public void ValidateShortCode_ReturnsFalse_ForNonAlphanumeric(string shortCode)
    {
        // Act
        var result = _validator.ValidateShortCode(shortCode);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Short code can only contain alphanumeric characters (a-z, A-Z, 0-9)", result.ErrorMessage);
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("api")]
    [InlineData("swagger")]
    [InlineData("health")]
    [InlineData("login")]
    [InlineData("register")]
    public void ValidateShortCode_ReturnsFalse_ForReservedWords(string shortCode)
    {
        // Act
        var result = _validator.ValidateShortCode(shortCode);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal($"Short code '{shortCode}' is reserved and cannot be used", result.ErrorMessage);
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("Test123")]
    [InlineData("aBc123XyZ")]
    public void ValidateShortCode_ReturnsTrue_ForValidCodes(string shortCode)
    {
        // Act
        var result = _validator.ValidateShortCode(shortCode);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
}
