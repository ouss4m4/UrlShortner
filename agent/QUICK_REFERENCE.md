# Quick Reference Guide

**Quick commands and reference for common development tasks**

---

## 🚀 Common Commands

### Testing

```bash
# Run all tests
cd Test
dotnet test

# Run all tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test file
dotnet test --filter "FullyQualifiedName~CacheServiceTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~CacheServiceTests.SetAsync_And_GetAsync_StoresAndRetrievesObject"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Server Management

```bash
# Start server (manual)
cd API
dotnet run

# Start server (script)
./start-server.sh

# Check server is running
curl http://localhost:5011/api/url

# Stop server
# Press Ctrl+C in terminal
```

### Docker Commands

```bash
# Start all services
docker-compose up -d

# Check service status
docker-compose ps

# View logs
docker-compose logs -f postgres
docker-compose logs -f redis

# Stop all services
docker-compose down

# Stop and remove volumes (DANGER: deletes data)
docker-compose down -v

# Restart single service
docker-compose restart postgres
docker-compose restart redis
```

### Database Commands

```bash
# Connect to Postgres
docker exec -it urlshortner_postgres psql -U urlshortner -d urlshortner_db

# Common Postgres queries
\dt                              # List tables
\d "Urls"                        # Describe Urls table
SELECT * FROM "Urls";            # View all URLs
SELECT * FROM "Visits";          # View all visits
SELECT * FROM "Analytics";       # View analytics
\q                               # Quit

# Apply migrations
cd API
dotnet ef database update

# Create new migration
cd API
dotnet ef migrations add MigrationName

# Remove last migration (if not applied)
cd API
dotnet ef migrations remove

# Generate SQL script from migrations
cd API
dotnet ef migrations script > migration.sql
```

### Redis Commands

```bash
# Connect to Redis
docker exec -it urlshortner_redis redis-cli

# Common Redis commands
KEYS *                           # List all keys
KEYS url:shortcode:*             # List cache keys
GET url:shortcode:1              # Get cached value
TTL url:shortcode:1              # Check TTL
DEL url:shortcode:1              # Delete key
FLUSHDB                          # Clear entire database (DANGER)
INFO                             # Server info
EXIT                             # Quit

# Monitor Redis in real-time
docker exec -it urlshortner_redis redis-cli MONITOR
```

### API Testing with curl

```bash
# Create URL (auto-generated short code)
curl -X POST http://localhost:5011/api/url \
  -H "Content-Type: application/json" \
  -d '{
    "originalUrl": "https://example.com",
    "userId": 1
  }'

# Create URL (custom short code)
curl -X POST http://localhost:5011/api/url \
  -H "Content-Type: application/json" \
  -d '{
    "originalUrl": "https://github.com",
    "shortCode": "gh",
    "userId": 1
  }'

# Get URL by short code
curl http://localhost:5011/api/url/short/1

# Test redirect (follow redirects)
curl -L http://localhost:5011/api/url/redirect/1

# Test redirect (show headers)
curl -i http://localhost:5011/api/url/redirect/1

# Get all URLs for user
curl http://localhost:5011/api/url/user/1

# Delete URL
curl -X DELETE http://localhost:5011/api/url/1
```

---

## 📁 File Locations

### Key Source Files

```
API/
├── Controllers/
│   ├── UrlController.cs         # URL endpoints
│   ├── UserController.cs        # User endpoints
│   ├── VisitController.cs       # Visit endpoints
│   └── AnalyticsController.cs   # Analytics endpoints
├── Services/
│   ├── IUrlService.cs           # URL service interface
│   ├── UrlService.cs            # URL business logic
│   ├── ICacheService.cs         # Cache interface
│   ├── RedisCacheService.cs     # Redis implementation
│   ├── IShortCodeGenerator.cs   # Generator interface
│   └── ShortCodeGenerator.cs    # Base62 encoding
├── Models/
│   ├── Url.cs                   # URL entity
│   ├── User.cs                  # User entity
│   ├── Visit.cs                 # Visit entity
│   └── Analytics.cs             # Analytics entity
├── Data/
│   └── UrlShortnerDbContext.cs  # EF Core context
└── Program.cs                   # App startup & DI
```

### Test Files

```
Test/
├── CacheServiceTests.cs         # Redis cache tests
├── UrlCachingTests.cs           # URL caching integration
├── ShortCodeGeneratorTests.cs   # Base62 encoding tests
├── UrlCrudTests.cs              # URL CRUD tests
├── UrlControllerTests.cs        # Controller tests
├── UrlRedirectTests.cs          # Redirect tests
├── UserCrudTests.cs             # User CRUD tests
├── VisitCrudTests.cs            # Visit CRUD tests
├── AnalyticsCrudTests.cs        # Analytics CRUD tests
└── ModelExistenceTests.cs       # Model validation
```

### Documentation

```
agent/
├── RESUME.md                    # Start here (quick context)
├── CURRENT_STATUS.md            # Current project state
├── process.md                   # TDD workflow & current iteration
├── steps.md                     # Roadmap & completed items
├── ITERATION_7_PLAN.md          # Next iteration detailed plan
├── redis-caching-summary.md     # Iteration 6 summary
├── requirements.md              # Original requirements
├── prd.md                       # Product requirements doc
├── instructions.md              # Development guidelines
└── test-strategy.md             # Testing approach

Root/
├── RUNNING.md                   # How to run & test API
├── TEST_RESULTS.md              # Manual test results
├── ITERATION_COMPLETE.md        # Iteration 6 summary
├── start-server.sh              # Server startup script
├── test-api.sh                  # API testing script
└── docker-compose.yaml          # Infrastructure config
```

---

## 🧪 Test Patterns

### Integration Test Template

```csharp
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Services;
using API.Models;

public class MyFeatureTests
{
    private UrlShortnerDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<UrlShortnerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new UrlShortnerDbContext(options);
    }

    [Fact]
    public async Task TestMethod()
    {
        // Arrange
        var context = GetInMemoryContext();
        var service = new MyService(context);

        // Act
        var result = await service.DoSomething();

        // Assert
        Assert.NotNull(result);
    }
}
```

### Controller Test Template

```csharp
using Microsoft.AspNetCore.Mvc;
using API.Controllers;
using API.Services;

public class MyControllerTests
{
    [Fact]
    public async Task ControllerAction_ReturnsExpectedResult()
    {
        // Arrange
        var context = GetInMemoryContext();
        var service = new MyService(context);
        var controller = new MyController(service);

        // Act
        var result = await controller.Action();

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(actionResult.Value);
    }
}
```

---

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=urlshortner_db;Username=urlshortner;Password=urlshortner_pw",
    "RedisConnection": "localhost:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### launchSettings.json (Port Configuration)

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5011",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 📊 Current Project State

### Tests: 44/44 Passing ✅

- CacheServiceTests: 6
- UrlCachingTests: 4
- ShortCodeGeneratorTests: 6
- UrlCrudTests: 8
- UrlControllerTests: 3
- UrlRedirectTests: 3
- UserCrudTests: 4
- VisitCrudTests: 3
- AnalyticsCrudTests: 3
- ModelExistenceTests: 4

### Infrastructure

- **Database:** PostgreSQL 15 (port 5432)
- **Cache:** Redis 7 (port 6379)
- **API:** .NET 10 Minimal API (port 5011)

### Features Implemented

✅ URL Shortening (Base62)  
✅ Auto-generated short codes  
✅ Custom short codes (aliases)  
✅ Collision detection  
✅ URL redirect (302)  
✅ Redis caching (cache-aside pattern)  
✅ Cache invalidation  
✅ Full CRUD (User, URL, Visit, Analytics)

### Next Up

📋 Iteration 7: Visit Tracking Enhancement

- Record IP, User-Agent on redirect
- Optional: GeoIP country lookup
- Update Analytics aggregates

---

## 🆘 Troubleshooting

### Problem: Tests Failing

```bash
# Check Docker services
docker-compose ps

# Restart services
docker-compose restart

# Clear Redis (may affect tests)
docker exec -it urlshortner_redis redis-cli FLUSHDB

# Rebuild and run tests
cd Test
dotnet clean
dotnet build
dotnet test
```

### Problem: Port 5011 Already in Use

```bash
# Find process using port
lsof -i :5011

# Kill process
kill -9 <PID>

# Or change port in API/Properties/launchSettings.json
```

### Problem: Database Migration Issues

```bash
# Check current migration status
cd API
dotnet ef migrations list

# Remove unapplied migration
dotnet ef migrations remove

# Reset database (DANGER: deletes all data)
docker-compose down -v
docker-compose up -d
dotnet ef database update
```

### Problem: Redis Connection Failed

```bash
# Check Redis is running
docker-compose ps redis

# Check Redis logs
docker-compose logs redis

# Test Redis connection
docker exec -it urlshortner_redis redis-cli PING
# Expected: PONG

# Restart Redis
docker-compose restart redis
```

---

## 📚 Additional Resources

### .NET 10 Documentation

- https://learn.microsoft.com/en-us/aspnet/core/

### Entity Framework Core

- https://learn.microsoft.com/en-us/ef/core/

### Redis Documentation

- https://redis.io/docs/

### xUnit Testing

- https://xunit.net/

### StackExchange.Redis

- https://stackexchange.github.io/StackExchange.Redis/

---

**Need more help? Check agent/RESUME.md for project overview!** 🚀
