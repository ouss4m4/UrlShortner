using Xunit;
using UrlShortner.API.Services;
using System.Linq;
using System.Collections.Generic;

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
        // Test that encoding produces non-empty, URL-safe codes
        var result1 = _generator.Encode(1);
        Assert.NotEmpty(result1);
        Assert.True(result1.Length >= 6, $"Code should be at least 6 characters, got: {result1}");

        var result2 = _generator.Encode(62);
        Assert.NotEmpty(result2);
        Assert.True(result2.Length >= 6);

        var result3 = _generator.Encode(100);
        Assert.NotEmpty(result3);
        Assert.DoesNotContain(" ", result3); // No spaces
        Assert.True(result3.Length >= 6); // At least 6 characters
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
    public void GeneratesShortCodesWithMinimumLength()
    {
        // Test that all codes meet minimum length requirement (6 characters)
        var testNumbers = new[] { 1, 2, 5, 10, 50, 100 };

        foreach (var number in testNumbers)
        {
            var code = _generator.Encode(number);
            Assert.True(code.Length >= 6, $"Code for {number} should be at least 6 characters, got: {code}");
        }
    }

    [Fact]
    public void CodesHaveVariationDueToRandomPadding()
    {
        // Test that encoding the same number multiple times produces different results
        // (due to random padding)
        var encodings = Enumerable.Range(0, 10)
            .Select(_ => _generator.Encode(1))
            .ToList();

        // All should have minimum length
        Assert.All(encodings, encoded => Assert.True(encoded.Length >= 6));

        // Due to random padding, we should see variation
        var uniqueEncodings = encodings.Distinct().Count();
        Assert.True(uniqueEncodings > 1, "Expected variation in encodings due to random padding");
    }

    [Fact]
    public void DifferentBaseNumbersProduceDifferentCodes()
    {
        // Generate codes for different numbers
        var codes = new List<string>();
        var testNumbers = new[] { 1, 2, 10, 100, 1000 };

        foreach (var number in testNumbers)
        {
            var code = _generator.Encode(number);
            codes.Add(code);
        }

        // All codes should be unique (different numbers = different codes)
        Assert.Equal(testNumbers.Length, codes.Distinct().Count());
    }

    [Fact]
    public void LargeNumbersProduceValidCodes()
    {
        // Test with large numbers
        var largeNumber = 123456789L;
        var code = _generator.Encode(largeNumber);

        Assert.NotEmpty(code);
        Assert.Matches(@"^[0-9a-zA-Z]+$", code);
        // Large numbers will naturally be longer than minimum
        Assert.True(code.Length >= 6);
    }

    [Fact]
    public void ZeroProducesValidCode()
    {
        // Edge case: zero
        var code = _generator.Encode(0);
        Assert.NotEmpty(code);
        Assert.True(code.Length >= 6);
        Assert.Matches(@"^[0-9a-zA-Z]+$", code);
    }
}
