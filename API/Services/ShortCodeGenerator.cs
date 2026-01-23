using System;
using System.Text;

namespace UrlShortner.API.Services;

/// <summary>
/// Generates short codes using base62 encoding (0-9, a-z, A-Z)
/// This provides URL-safe, compact representations of integer IDs
/// </summary>
public class ShortCodeGenerator : IShortCodeGenerator
{
    private const string Base62Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int Base = 62;

    /// <summary>
    /// Encodes an integer into a base62 string
    /// </summary>
    /// <param name="id">The integer to encode (must be positive)</param>
    /// <returns>Base62 encoded string</returns>
    public string Encode(long id)
    {
        if (id == 0) return "0";
        if (id < 0) throw new ArgumentException("ID must be non-negative", nameof(id));

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
