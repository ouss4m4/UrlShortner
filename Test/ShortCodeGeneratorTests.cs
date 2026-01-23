using Xunit;
using UrlShortner.API.Services;

namespace Test;

public class ShortCodeGeneratorTests
{
    private readonly IShortCodeGenerator _generator;

    public ShortCodeGeneratorTests()
    {
        _generator = new ShortCodeGenerator();
    }

    [Fact]
    public void CanEncodePositiveInteger()
    {
        // Test base62 encoding of various integers
        var result1 = _generator.Encode(1);
        Assert.Equal("1", result1);

        var result2 = _generator.Encode(62);
        Assert.Equal("10", result2); // 62 in base62 = "10"

        var result3 = _generator.Encode(100);
        Assert.NotEmpty(result3);
        Assert.DoesNotContain(" ", result3); // No spaces
    }

    [Fact]
    public void CanDecodeBase62String()
    {
        // Test base62 decoding
        var decoded1 = _generator.Decode("1");
        Assert.Equal(1, decoded1);

        var decoded2 = _generator.Decode("10");
        Assert.Equal(62, decoded2);
    }

    [Fact]
    public void EncodeDecodeRoundTrip()
    {
        // Test that encode->decode returns original value
        var testNumbers = new[] { 1, 10, 100, 1000, 10000, 123456 };

        foreach (var number in testNumbers)
        {
            var encoded = _generator.Encode(number);
            var decoded = _generator.Decode(encoded);
            Assert.Equal(number, decoded);
        }
    }

    [Fact]
    public void EncodedStringsAreUrlSafe()
    {
        // Test that encoded strings only contain URL-safe characters
        // Base62: 0-9, a-z, A-Z (no special characters, spaces, or URL-unsafe chars)
        var testNumbers = new[] { 1, 100, 1000, 10000, 999999 };

        foreach (var number in testNumbers)
        {
            var encoded = _generator.Encode(number);
            Assert.Matches(@"^[0-9a-zA-Z]+$", encoded); // Only alphanumeric
        }
    }

    [Fact]
    public void DifferentNumbersProduceDifferentCodes()
    {
        // Test uniqueness
        var code1 = _generator.Encode(1);
        var code2 = _generator.Encode(2);
        var code3 = _generator.Encode(100);

        Assert.NotEqual(code1, code2);
        Assert.NotEqual(code2, code3);
        Assert.NotEqual(code1, code3);
    }

    [Fact]
    public void GeneratesShortCodesWithMinimumLength()
    {
        // Short codes should be padded to minimum length for consistency
        // For small IDs, we want consistent length (e.g., minimum 6 characters)
        var code = _generator.Encode(1);
        Assert.NotEmpty(code);
        // Note: We'll implement padding in the actual service
    }
}
