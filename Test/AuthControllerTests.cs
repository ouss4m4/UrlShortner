using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UrlShortner.DTOs;
using UrlShortner.Models;
using Xunit;

namespace Test;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsAuthResponse()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var request = new RegisterRequest
        {
            Username = $"testuser_{uniqueId}",
            Email = $"test_{uniqueId}@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.AccessToken);
        Assert.NotEmpty(authResponse.RefreshToken);
        Assert.Equal($"testuser_{uniqueId}", authResponse.Username);
        Assert.Equal($"test_{uniqueId}@example.com", authResponse.Email);
        Assert.True(authResponse.UserId > 0);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409Conflict()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var request = new RegisterRequest
        {
            Username = $"user1_{uniqueId}",
            Email = $"duplicate_{uniqueId}@example.com",
            Password = "Password123!"
        };

        // First registration
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Try to register again with same email
        var duplicateRequest = new RegisterRequest
        {
            Username = $"user2_{uniqueId}",
            Email = $"duplicate_{uniqueId}@example.com",
            Password = "DifferentPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidEmail_Returns400BadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "invalid-email",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400BadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "weak"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange - First register a user
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var registerRequest = new RegisterRequest
        {
            Username = $"logintest_{uniqueId}",
            Email = $"login_{uniqueId}@example.com",
            Password = "LoginPassword123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = $"login_{uniqueId}@example.com",
            Password = "LoginPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.AccessToken);
        Assert.NotEmpty(authResponse.RefreshToken);
        Assert.Equal($"logintest_{uniqueId}", authResponse.Username);
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401Unauthorized()
    {
        // Arrange - First register a user
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var registerRequest = new RegisterRequest
        {
            Username = $"testuser_{uniqueId}",
            Email = $"wrongpass_{uniqueId}@example.com",
            Password = "CorrectPassword123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = $"wrongpass_{uniqueId}@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_NonexistentUser_Returns401Unauthorized()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var loginRequest = new LoginRequest
        {
            Email = $"nonexistent_{uniqueId}@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsNewAccessToken()
    {
        // Arrange - Register and get initial tokens
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var registerRequest = new RegisterRequest
        {
            Username = $"refreshtest_{uniqueId}",
            Email = $"refresh_{uniqueId}@example.com",
            Password = "RefreshPassword123!"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = authResponse!.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var newAuthResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(newAuthResponse);
        Assert.NotEmpty(newAuthResponse.AccessToken);
        Assert.NotEmpty(newAuthResponse.RefreshToken);
        Assert.NotEqual(authResponse.AccessToken, newAuthResponse.AccessToken); // New token should be different
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_Returns401Unauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "invalid-refresh-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ValidRequest_Returns200OK()
    {
        // Arrange - Register and login
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var registerRequest = new RegisterRequest
        {
            Username = $"logouttest_{uniqueId}",
            Email = $"logout_{uniqueId}@example.com",
            Password = "LogoutPassword123!"
        };

        // Create a new client for this test to avoid token conflicts
        var client = _factory.CreateClient();
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Add bearer token to client
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        // Act
        var response = await client.PostAsync("/api/auth/logout", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutAuth_Returns401Unauthorized()
    {
        // Act
        var response = await _client.PostAsync("/api/auth/logout", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
