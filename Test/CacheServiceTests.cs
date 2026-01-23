using API.Services;
using StackExchange.Redis;
using Xunit;

namespace Test;

public class CacheServiceTests : IAsyncLifetime
{
    private readonly ICacheService _cacheService;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public CacheServiceTests()
    {
        // Connect to Redis running in Docker
        _redis = ConnectionMultiplexer.Connect("localhost:6379");
        _db = _redis.GetDatabase();
        _cacheService = new RedisCacheService(_redis);
    }

    public async Task InitializeAsync()
    {
        // Clean up before each test
        await _db.ExecuteAsync("FLUSHDB");
    }

    public async Task DisposeAsync()
    {
        await _redis.CloseAsync();
        _redis.Dispose();
    }

    [Fact]
    public async Task SetAsync_And_GetAsync_StoresAndRetrievesObject()
    {
        // Arrange
        var key = "test-key";
        var value = new TestObject { Id = 1, Name = "Test" };

        // Act
        await _cacheService.SetAsync(key, value);
        var retrieved = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(value.Id, retrieved.Id);
        Assert.Equal(value.Name, retrieved.Name);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_WithExpiration_ExpiresAfterTimeSpan()
    {
        // Arrange
        var key = "expiring-key";
        var value = new TestObject { Id = 2, Name = "Expiring" };
        var expiration = TimeSpan.FromSeconds(1);

        // Act
        await _cacheService.SetAsync(key, value, expiration);
        var immediate = await _cacheService.GetAsync<TestObject>(key);
        await Task.Delay(1100); // Wait for expiration
        var afterExpiry = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        Assert.NotNull(immediate);
        Assert.Null(afterExpiry);
    }

    [Fact]
    public async Task RemoveAsync_DeletesKey()
    {
        // Arrange
        var key = "removable-key";
        var value = new TestObject { Id = 3, Name = "ToRemove" };
        await _cacheService.SetAsync(key, value);

        // Act
        await _cacheService.RemoveAsync(key);
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueWhenKeyExists()
    {
        // Arrange
        var key = "exists-key";
        var value = new TestObject { Id = 4, Name = "Exists" };
        await _cacheService.SetAsync(key, value);

        // Act
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalseWhenKeyDoesNotExist()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        Assert.False(exists);
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
