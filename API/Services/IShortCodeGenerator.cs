namespace UrlShortner.API.Services;

/// <summary>
/// Service for generating and decoding short codes using base62 encoding
/// </summary>
public interface IShortCodeGenerator
{
    /// <summary>
    /// Encodes an integer into a base62 string
    /// </summary>
    /// <param name="id">The integer to encode</param>
    /// <returns>Base62 encoded string</returns>
    string Encode(long id);

    /// <summary>
    /// Decodes a base62 string back into an integer
    /// </summary>
    /// <param name="shortCode">The base62 string to decode</param>
    /// <returns>Decoded integer value</returns>
    long Decode(string shortCode);
}
