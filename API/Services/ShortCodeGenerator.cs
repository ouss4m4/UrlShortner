using System;
using System.Text;

namespace UrlShortner.API.Services;

/// <summary>
/// Generates short codes using base62 encoding with random padding
/// This provides URL-safe, compact, and non-sequential codes
/// </summary>
public class ShortCodeGenerator : IShortCodeGenerator
{
    private const string Base62Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int Base = 62;
    private const int MinCodeLength = 6; // Minimum 6 characters for better aesthetics and security
    private static readonly Random _random = new Random();

    /// <summary>
    /// Encodes an integer into a base62 string with random padding to ensure minimum length
    /// </summary>
    /// <param name="id">The integer to encode (must be positive)</param>
    /// <returns>Base62 encoded string (minimum 6 characters)</returns>
    public string Encode(long id)
    {
        if (id < 0) throw new ArgumentException("ID must be non-negative", nameof(id));

        // Generate base62 code from ID
        var baseCode = id == 0 ? "0" : EncodeBase62(id);

        // If code is shorter than minimum, pad with random characters
        if (baseCode.Length < MinCodeLength)
        {
            var result = new StringBuilder(baseCode);
            var charsNeeded = MinCodeLength - baseCode.Length;

            for (int i = 0; i < charsNeeded; i++)
            {
                result.Append(Base62Alphabet[_random.Next(Base)]);
            }

            return result.ToString();
        }

        return baseCode;
    }

    private string EncodeBase62(long id)
    {
        var result = new StringBuilder();

        while (id > 0)
        {
            var remainder = (int)(id % Base);
            result.Insert(0, Base62Alphabet[remainder]);
            id /= Base;
        }

        return result.ToString();
    }

    /// <summary>
    /// Decodes a base62 string back into an integer
    /// </summary>
    /// <param name="shortCode">The base62 string to decode</param>
    /// <returns>Decoded integer value</returns>
    public long Decode(string shortCode)
    {
        if (string.IsNullOrEmpty(shortCode))
            throw new ArgumentException("Short code cannot be null or empty", nameof(shortCode));

        long result = 0;

        for (int i = 0; i < shortCode.Length; i++)
        {
            var character = shortCode[i];
            var index = Base62Alphabet.IndexOf(character);

            if (index == -1)
                throw new ArgumentException($"Invalid character '{character}' in short code", nameof(shortCode));

            result = result * Base + index;
        }

        return result;
    }
}
