namespace UrlShortner.API.Services;

public interface IUrlValidator
{
    ValidationResult ValidateUrl(string url);
    ValidationResult ValidateShortCode(string shortCode);
}
