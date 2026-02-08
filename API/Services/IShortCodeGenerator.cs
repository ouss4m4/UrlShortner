namespace UrlShortner.API.Services;

/// <summary>
/// Service for generating short codes using base62 encoding with random padding
/// </summary>
public interface IShortCodeGenerator
{
    /// <summary>
    /// Encodes an integer into a base62 string with random padding for aesthetics
    /// </summary>
    /// <param name="id">The integer to encode</param>
    /// <returns>Base62 encoded string (minimum 6 characters)</returns>
    string Encode(long id);
}
