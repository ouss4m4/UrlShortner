using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using UrlShortner.API.Services;
using UrlShortner.Models;
using Xunit;

namespace Test;

public class AuthenticationTests
{
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthenticationTests()
    {
        // Setup test configuration
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:Secret", "ThisIsATestSecretKeyThatIsAtLeast32CharactersLong"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:AccessTokenExpirationMinutes", "60"},
            {"Jwt:RefreshTokenExpirationDays", "7"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _tokenService = new TokenService(_configuration);
    }

    [Fact]
    public void GenerateAccessToken_CreatesValidJwt()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var token = _tokenService.GenerateAccessToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal("TestIssuer", jwtToken.Issuer);
        Assert.Equal("TestAudience", jwtToken.Audiences.First());
        Assert.Equal("1", jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("testuser", jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal("test@example.com", jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value);
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectExpiration()
    {
        // Arrange
        var user = new User { Id = 1, Username = "test", Email = "test@example.com" };

        // Act
        var token = _tokenService.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var expectedExpiry = DateTime.UtcNow.AddMinutes(60);
        var actualExpiry = jwtToken.ValidTo;

        Assert.True((actualExpiry - expectedExpiry).TotalSeconds < 5); // Within 5 seconds
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsBase64String()
    {
        // Act
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Assert
        Assert.NotNull(refreshToken);
        Assert.NotEmpty(refreshToken);

        // Should be valid base64
        var bytes = Convert.FromBase64String(refreshToken);
        Assert.Equal(64, bytes.Length);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueTokens()
    {
        // Act
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void ValidateAccessToken_ValidToken_ReturnsUserId()
    {
        // Arrange
        var user = new User { Id = 42, Username = "test", Email = "test@example.com" };
        var token = _tokenService.GenerateAccessToken(user);

        // Act
        var userId = _tokenService.ValidateAccessToken(token);

        // Assert
        Assert.Equal(42, userId);
    }

    [Fact]
    public void ValidateAccessToken_InvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var userId = _tokenService.ValidateAccessToken(invalidToken);

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void ValidateAccessToken_ExpiredToken_ReturnsNull()
    {
        // Arrange - create token with immediate expiration
        var expiredConfig = new Dictionary<string, string>
        {
            {"Jwt:Secret", "ThisIsATestSecretKeyThatIsAtLeast32CharactersLong"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:AccessTokenExpirationMinutes", "0"} // Immediate expiration
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(expiredConfig!)
            .Build();

        var tokenService = new TokenService(config);
        var user = new User { Id = 1, Username = "test", Email = "test@example.com" };
        var token = tokenService.GenerateAccessToken(user);

        // Wait a moment to ensure expiration
        Thread.Sleep(1000);

        // Act
        var userId = _tokenService.ValidateAccessToken(token);

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void ValidateAccessToken_WrongSecret_ReturnsNull()
    {
        // Arrange - create token with one secret
        var user = new User { Id = 1, Username = "test", Email = "test@example.com" };
        var token = _tokenService.GenerateAccessToken(user);

        // Create validator with different secret
        var wrongConfig = new Dictionary<string, string>
        {
            {"Jwt:Secret", "DifferentSecretKeyThatIsAtLeast32Characters"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(wrongConfig!)
            .Build();

        var wrongTokenService = new TokenService(config);

        // Act
        var userId = wrongTokenService.ValidateAccessToken(token);

        // Assert
        Assert.Null(userId);
    }
}
