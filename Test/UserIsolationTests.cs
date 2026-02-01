using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UrlShortner.DTOs;
using UrlShortner.Models;
using Xunit;

namespace Test;

public class UserIsolationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UserIsolationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task<(string token, int userId)> RegisterAndLoginUser(HttpClient client, string username, string email)
    {
        var registerRequest = new RegisterRequest
        {
            Username = username,
            Email = email,
            Password = "Test123!@#"
        };

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        return (authResponse!.AccessToken, authResponse.UserId);
    }

    [Fact]
    public async Task GetUrlsByUserId_ReturnsOwnUrlsOnly()
    {
        var client = _factory.CreateClient();

        // Create two users
        var (token1, userId1) = await RegisterAndLoginUser(client, $"user1_{Guid.NewGuid()}", $"user1_{Guid.NewGuid()}@test.com");
        var (token2, userId2) = await RegisterAndLoginUser(client, $"user2_{Guid.NewGuid()}", $"user2_{Guid.NewGuid()}@test.com");

        // User 1 creates a URL
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var url1 = new { originalUrl = "https://user1.com", userId = userId1 };
        await client.PostAsJsonAsync("/api/url", url1);

        // User 1 can get their own URLs
        var response1 = await client.GetAsync($"/api/url/user/{userId1}");
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        // User 2 cannot get User 1's URLs (Forbidden)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var response2 = await client.GetAsync($"/api/url/user/{userId1}");
        Assert.Equal(HttpStatusCode.Forbidden, response2.StatusCode);
    }

    [Fact]
    public async Task GetUrlsByUserId_RequiresAuthentication()
    {
        var client = _factory.CreateClient();

        // Try to get URLs without authentication
        var response = await client.GetAsync("/api/url/user/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUrl_RequiresOwnership()
    {
        var client = _factory.CreateClient();

        // Create two users
        var (token1, userId1) = await RegisterAndLoginUser(client, $"user1_{Guid.NewGuid()}", $"user1_{Guid.NewGuid()}@test.com");
        var (token2, userId2) = await RegisterAndLoginUser(client, $"user2_{Guid.NewGuid()}", $"user2_{Guid.NewGuid()}@test.com");

        // User 1 creates a URL
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var url = new { originalUrl = "https://original.com", userId = userId1 };
        var createResponse = await client.PostAsJsonAsync("/api/url", url);
        var createdUrl = await createResponse.Content.ReadFromJsonAsync<Url>();

        // User 1 can update their own URL
        var updateData = new { id = createdUrl!.Id, originalUrl = "https://updated.com", shortCode = createdUrl.ShortCode, userId = userId1, createdAt = createdUrl.CreatedAt };
        var updateResponse1 = await client.PutAsJsonAsync($"/api/url/{createdUrl.Id}", updateData);
        Assert.Equal(HttpStatusCode.OK, updateResponse1.StatusCode);

        // User 2 cannot update User 1's URL
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var updateResponse2 = await client.PutAsJsonAsync($"/api/url/{createdUrl.Id}", updateData);
        Assert.Equal(HttpStatusCode.Forbidden, updateResponse2.StatusCode);
    }

    [Fact]
    public async Task DeleteUrl_RequiresOwnership()
    {
        var client = _factory.CreateClient();

        // Create two users
        var (token1, userId1) = await RegisterAndLoginUser(client, $"user1_{Guid.NewGuid()}", $"user1_{Guid.NewGuid()}@test.com");
        var (token2, userId2) = await RegisterAndLoginUser(client, $"user2_{Guid.NewGuid()}", $"user2_{Guid.NewGuid()}@test.com");

        // User 1 creates a URL
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var url = new { originalUrl = "https://original.com", userId = userId1 };
        var createResponse = await client.PostAsJsonAsync("/api/url", url);
        var createdUrl = await createResponse.Content.ReadFromJsonAsync<Url>();

        // User 2 cannot delete User 1's URL
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var deleteResponse1 = await client.DeleteAsync($"/api/url/{createdUrl!.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse1.StatusCode);

        // User 1 can delete their own URL
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var deleteResponse2 = await client.DeleteAsync($"/api/url/{createdUrl.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse2.StatusCode);
    }

    [Fact]
    public async Task GetAnalytics_RequiresUrlOwnership()
    {
        var client = _factory.CreateClient();

        // Create two users
        var (token1, userId1) = await RegisterAndLoginUser(client, $"user1_{Guid.NewGuid()}", $"user1_{Guid.NewGuid()}@test.com");
        var (token2, userId2) = await RegisterAndLoginUser(client, $"user2_{Guid.NewGuid()}", $"user2_{Guid.NewGuid()}@test.com");

        // User 1 creates a URL
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var url = new { originalUrl = "https://analytics.com", userId = userId1 };
        var createResponse = await client.PostAsJsonAsync("/api/url", url);
        var createdUrl = await createResponse.Content.ReadFromJsonAsync<Url>();

        // User 1 can get analytics for their own URL
        var analyticsResponse1 = await client.GetAsync($"/api/analytics/url/{createdUrl!.Id}");
        Assert.Equal(HttpStatusCode.OK, analyticsResponse1.StatusCode);

        // User 2 cannot get analytics for User 1's URL
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var analyticsResponse2 = await client.GetAsync($"/api/analytics/url/{createdUrl.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, analyticsResponse2.StatusCode);
    }

    [Fact]
    public async Task GetAnalytics_RequiresAuthentication()
    {
        var client = _factory.CreateClient();

        // Try to get analytics without authentication
        var response = await client.GetAsync("/api/analytics/url/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserProfile_RequiresOwnership()
    {
        var client = _factory.CreateClient();

        // Create two users
        var (token1, userId1) = await RegisterAndLoginUser(client, $"user1_{Guid.NewGuid()}", $"user1_{Guid.NewGuid()}@test.com");
        var (token2, userId2) = await RegisterAndLoginUser(client, $"user2_{Guid.NewGuid()}", $"user2_{Guid.NewGuid()}@test.com");

        // User 1 can get their own profile
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var response1 = await client.GetAsync($"/api/user/{userId1}");
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        // User 2 cannot get User 1's profile
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var response2 = await client.GetAsync($"/api/user/{userId1}");
        Assert.Equal(HttpStatusCode.Forbidden, response2.StatusCode);
    }

    [Fact]
    public async Task UpdateUserProfile_RequiresOwnership()
    {
        var client = _factory.CreateClient();

        // Create two users
        var (token1, userId1) = await RegisterAndLoginUser(client, $"user1_{Guid.NewGuid()}", $"user1_{Guid.NewGuid()}@test.com");
        var (token2, userId2) = await RegisterAndLoginUser(client, $"user2_{Guid.NewGuid()}", $"user2_{Guid.NewGuid()}@test.com");

        // User 1 gets their profile
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var getResponse = await client.GetAsync($"/api/user/{userId1}");
        var user1 = await getResponse.Content.ReadFromJsonAsync<User>();

        // User 1 can update their own profile
        var updateData = new { id = user1!.Id, username = "newusername", email = user1.Email, createdAt = user1.CreatedAt, updatedAt = DateTime.UtcNow };
        var updateResponse1 = await client.PutAsJsonAsync($"/api/user/{userId1}", updateData);
        Assert.Equal(HttpStatusCode.OK, updateResponse1.StatusCode);

        // User 2 cannot update User 1's profile
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var updateResponse2 = await client.PutAsJsonAsync($"/api/user/{userId1}", updateData);
        Assert.Equal(HttpStatusCode.Forbidden, updateResponse2.StatusCode);
    }

    [Fact]
    public async Task DeleteUserProfile_RequiresOwnership()
    {
        var client = _factory.CreateClient();

        // Create two users
        var (token1, userId1) = await RegisterAndLoginUser(client, $"user1_{Guid.NewGuid()}", $"user1_{Guid.NewGuid()}@test.com");
        var (token2, userId2) = await RegisterAndLoginUser(client, $"user2_{Guid.NewGuid()}", $"user2_{Guid.NewGuid()}@test.com");

        // User 2 cannot delete User 1's profile
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var deleteResponse1 = await client.DeleteAsync($"/api/user/{userId1}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse1.StatusCode);

        // User 1 can delete their own profile
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var deleteResponse2 = await client.DeleteAsync($"/api/user/{userId1}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse2.StatusCode);
    }
}
