# Architecture (Minimal)

### High-Level Overview

```
┌────────────────────────────────────────────────────────────┐
│                         CLIENT                              │
│                    (Web Browser, API Client)                │
└───────────────────────────┬────────────────────────────────┘
                            │ HTTP/HTTPS
                            ↓
┌────────────────────────────────────────────────────────────┐
│                      .NET 10 API                            │
│                  (Minimal API + Controllers)                │
│                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │UrlController │  │UserController│  │VisitController│     │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘     │
│         │                  │                  │              │
│         └──────────────────┼──────────────────┘              │
│                            ↓                                 │
│  ┌────────────────────────────────────────────────────┐    │
│  │              SERVICE LAYER                          │    │
│  │  - UrlService      - UserService                    │    │
│  │  - VisitService    - AnalyticsService               │    │
│  │  - CacheService    - ShortCodeGenerator             │    │
│  └────────────┬───────────────────┬───────────────────┘    │
│               │                    │                         │
└───────────────┼────────────────────┼─────────────────────────┘
                │                    │
        ┌───────▼──────┐     ┌──────▼────────┐
        │  EF Core     │     │ Redis Cache   │
        │  DbContext   │     │  (Optional)   │
        └───────┬──────┘     └───────────────┘
                │
        ┌───────▼──────┐
        │  PostgreSQL  │
        │   Database   │
        └──────────────┘
```

## Overview

- Client: React app served from API `/wwwroot`
- API: .NET 10 (Controllers + Services)
- Data: PostgreSQL via EF Core
- Cache: Redis (cache-aside)
- Hosting: Railway (multi-instance behind load balancer)

## Layers

- Controllers: HTTP endpoints, request/response shape
- Services: business logic, validation, caching, analytics
- Data: EF Core DbContext + migrations
- Cache: Redis for hot reads and rate limiting

## Data Flow

1. Client calls API
2. Controller validates request
3. Service executes business logic
4. Data stored in Postgres, cached in Redis
5. Analytics recorded asynchronously

## Key Decisions

- Base62 short codes (URL-safe, dense keyspace)
- Cache-aside for short code lookups
- Fire-and-forget visit tracking
- Redis-backed rate limiting
- Health checks + structured logs for multi-instance ops

## Ops Notes

- Health endpoints: `/health`, `/health/live`, `/health/ready`
- Instance tracking: `X-Instance-Id` header + Serilog enrichment
- Railway: multiple API replicas behind edge load balancer
  while (number > 0)
  {
  result.Insert(0, Alphabet[number % 62]);
  number /= 62;
  }
  return result.ToString();
  }

````

**Examples:**

- ID 1 → "1"
- ID 62 → "10"
- ID 3844 → "100"
- ID 100000 → "q0U"

### Decision 2: Insert-Then-Generate Approach

**Chosen:** Create URL in DB first, then generate short code from ID

**Alternative Considered:** Generate short code first, then insert

**Rationale:**

- Database ID guarantees uniqueness
- No collision possibility
- No retry logic needed
- Simpler implementation

**Implementation:**

```csharp
public async Task<Url> CreateUrlAsync(string originalUrl, string? customShortCode, int userId)
{
    var url = new Url
    {
        OriginalUrl = originalUrl,
        UserId = userId,
        CreatedAt = DateTime.UtcNow
    };

    _context.Urls.Add(url);
    await _context.SaveChangesAsync(); // Gets ID from DB

    if (string.IsNullOrWhiteSpace(customShortCode))
    {
        url.ShortCode = _shortCodeGenerator.Encode(url.Id); // Generate from ID
        await _context.SaveChangesAsync();
    }
    else
    {
        url.ShortCode = customShortCode;
        await _context.SaveChangesAsync();
    }

    return url;
}
````

**Trade-offs:**

- Pro: Simple, reliable, no collisions
- Con: Two database writes (mitigated by transaction)
- Con: IDs are sequential (mitigated by Base62 obfuscation)

### Decision 3: Unique Short Code per URL Creation

**Chosen:** Each URL creation gets a new short code (even if same OriginalUrl)

**Alternative Considered:** Deduplicate by OriginalUrl (return existing short code)

**Rationale:**

- **Privacy:** Different users don't share analytics
- **Ownership:** Each user owns their short codes
- **Flexibility:** Users can create multiple short codes for A/B testing
- **Simplicity:** No deduplication logic needed

**Example:**

```
User A creates: https://example.com → Short code "1"
User B creates: https://example.com → Short code "2" (not "1")
```

### Decision 4: Manual Short Code Support

**Chosen:** Allow users to provide custom short codes (aliases)

**Implementation:**

- Check for duplicates before insert
- Return 409 Conflict if taken
- User-friendly error message with suggested alternatives (future)

**Error Response:**

```json
HTTP 409 Conflict
{
  "error": "ShortCodeAlreadyExists",
  "message": "Short code 'fb' is already taken. Please choose a different short code.",
  "shortCode": "fb"
}
```

### Decision 5: Cache TTL Strategy

**Chosen:** 1-hour TTL with cache invalidation on updates/deletes

**Rationale:**

- Balance between performance and staleness
- URLs rarely change → safe to cache
- 1 hour is reasonable for most use cases
- Invalidation prevents stale data on updates

**Alternative Considered:** No expiration (infinite cache)

- Rejected: Risk of memory bloat
- Rejected: Harder to debug stale data issues

### Decision 6: Real Integration Tests (No Mocks)

**Chosen:** Use real EF Core InMemory database and real Redis (Docker)

**Rationale:**

- Tests actual behavior, not mocked behavior
- Catches integration issues early
- Provides confidence for refactoring
- Fast enough (~1.6s for 44 tests)

**Example:**

```csharp
private UrlShortnerDbContext GetInMemoryContext()
{
    var options = new DbContextOptionsBuilder<UrlShortnerDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    return new UrlShortnerDbContext(options);
}
```

### Decision 7: Optional Cache Service

**Chosen:** ICacheService is optional (null-safe) in UrlService

**Rationale:**

- Tests can run without Redis
- Gradual adoption (add Redis later)
- Fallback to DB if Redis unavailable

**Implementation:**

```csharp
public class UrlService : IUrlService
{
    private readonly ICacheService? _cacheService; // Nullable

    public async Task<Url?> GetUrlByShortCodeAsync(string shortCode)
    {
        if (_cacheService != null)
        {
            // Use cache if available
        }

        // Always fall back to DB
        return await _context.Urls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
    }
}
```

### Decision 8: Port Configuration

**Chosen:** Run API on port 5011 (not default 5000)

**Rationale:**

- Port 5000 used by macOS AirPlay Receiver
- Avoids conflicts on developer machines
- Explicit configuration in launchSettings.json

### Decision 9: Strict TDD Process

**Chosen:** Always write tests first (RED), then implement (GREEN)

**Workflow:**

1. Write failing tests (RED phase)
2. Implement minimum code to pass tests (GREEN phase)
3. Refactor if needed (REFACTOR phase)
4. Document in agent/\*.md files

**Benefits:**

- Forces thinking about requirements first
- Prevents over-engineering
- Ensures test coverage
- Catches regressions early

---

## 🗄️ Database Schema

### Entities

#### User

```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<Url> Urls { get; set; } = new List<Url>();
}
```

#### Url

```csharp
public class Url
{
    public int Id { get; set; }
    public string OriginalUrl { get; set; } = null!;
    public string ShortCode { get; set; } = null!; // Unique index
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
```

#### Visit

```csharp
public class Visit
{
    public int Id { get; set; }
    public int UrlId { get; set; }
    public int? UserId { get; set; } // Nullable (anonymous visits)
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referrer { get; set; }
    public DateTime VisitedAt { get; set; }

    // Navigation
    public Url Url { get; set; } = null!;
    public User? User { get; set; }
}
```

#### Analytics

```csharp
public class Analytics
{
    public int Id { get; set; }
    public int UrlId { get; set; }
    public int TotalVisits { get; set; }
    public int UniqueVisitors { get; set; }
    public DateTime LastVisitedAt { get; set; }
    public string? Metadata { get; set; } // JSON

    // Navigation
    public Url Url { get; set; } = null!;
}
```

### Indexes

```sql
-- Unique index on ShortCode (enforced by EF Core)
CREATE UNIQUE INDEX IX_Urls_ShortCode ON Urls (ShortCode);

-- Performance indexes (future)
CREATE INDEX IX_Visits_UrlId ON Visits (UrlId);
CREATE INDEX IX_Visits_VisitedAt ON Visits (VisitedAt);
CREATE INDEX IX_Analytics_UrlId ON Analytics (UrlId);
```

---

## 🔒 Security Considerations

### Current Implementation

✅ **SQL Injection Protection**

- Using EF Core parameterized queries
- No raw SQL execution

✅ **Unique Constraint on ShortCode**

- Database-level constraint
- Application-level pre-check

⚠️ **Not Yet Implemented:**

- Authentication/Authorization (JWT)
- Rate limiting (per IP, per user)
- HTTPS enforcement
- CORS policy
- Input validation (URL format)
- XSS protection (output encoding)
- CSRF protection

### Future Security Enhancements

1. **Authentication:** JWT tokens for user login
2. **Authorization:** Role-based access (user, admin)
3. **Rate Limiting:** Prevent abuse (e.g., 100 requests/hour)
4. **Input Validation:** Validate URL format, length, malicious content
5. **HTTPS:** Enforce TLS in production
6. **CORS:** Whitelist allowed origins
7. **Logging:** Track suspicious activity

---

## 📊 Performance Characteristics

### Current Performance

| Operation         | Without Cache | With Cache | Improvement |
| ----------------- | ------------- | ---------- | ----------- |
| GetUrlByShortCode | ~10-20ms      | ~1-2ms     | **10x-20x** |
| CreateUrl         | ~15-25ms      | N/A        | -           |
| UpdateUrl         | ~10-20ms      | N/A        | -           |
| DeleteUrl         | ~10-15ms      | N/A        | -           |
| Redirect          | ~10-20ms      | ~1-2ms     | **10x-20x** |

### Scalability

**Current Capacity:**

- Single PostgreSQL instance: ~10,000 QPS
- Single Redis instance: ~100,000 QPS
- .NET API: Limited by CPU/memory

**Bottlenecks:**

1. Database writes (CreateUrl, UpdateUrl)
2. Database reads (without cache)
3. Single-instance architecture

**Future Optimizations:**

- Database replication (read replicas)
- Redis cluster (horizontal scaling)
- Load balancer (multiple API instances)
- CDN for static content

---

## 🧪 Testing Strategy

### Test Pyramid

```
         ▲
        / \
       /E2E\         - Manual testing with curl
      /─────\
     /  API  \       - Controller integration tests
    /─────────\
   / Service   \     - Service layer tests (InMemory DB)
  /─────────────\
 /     Unit      \   - Base62 encoding, validation logic
/_________________\
```

### Test Types

1. **Unit Tests:** Logic without dependencies (Base62 encoding)
2. **Integration Tests:** Services with InMemory DB and real Redis
3. **Controller Tests:** Full request/response cycle
4. **Manual Tests:** curl commands for real-world verification

### Coverage

- **44 tests total**
- **100% coverage** of core business logic
- **Real database** (InMemory + Redis Docker)
- **No mocks** (except HttpContext where needed)

---

## 📦 Dependencies & Versions

### Production Dependencies

| Package                               | Version | Purpose         |
| ------------------------------------- | ------- | --------------- |
| Microsoft.AspNetCore.App              | 10.0    | Web framework   |
| Microsoft.EntityFrameworkCore         | 10.0.2  | ORM             |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.0  | Postgres driver |
| StackExchange.Redis                   | 2.10.1  | Redis client    |

### Testing Dependencies

| Package                                | Version | Purpose        |
| -------------------------------------- | ------- | -------------- |
| xunit                                  | 2.9.2   | Test framework |
| xunit.runner.visualstudio              | 2.8.2   | Test runner    |
| Microsoft.EntityFrameworkCore.InMemory | 10.0.2  | Test database  |
| StackExchange.Redis                    | 2.10.1  | Redis testing  |

### Infrastructure

| Component  | Version     | Port |
| ---------- | ----------- | ---- |
| PostgreSQL | 15 (Alpine) | 5432 |
| Redis      | 7 (Alpine)  | 6379 |
| .NET SDK   | 10.0        | -    |

---

## 🚀 Deployment Considerations

### Environment Variables (Future)

```bash
# Database
DATABASE_HOST=localhost
DATABASE_PORT=5432
DATABASE_NAME=urlshortner_db
DATABASE_USER=urlshortner
DATABASE_PASSWORD=***

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379

# Application
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5011
JWT_SECRET=***
```

### Docker Deployment (Future)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5011

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["API/UrlShortner.csproj", "API/"]
RUN dotnet restore "API/UrlShortner.csproj"
COPY . .
WORKDIR "/src/API"
RUN dotnet build "UrlShortner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UrlShortner.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UrlShortner.dll"]
```

### Health Checks (Future)

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddRedis(redisConnection);

app.MapHealthChecks("/health");
```

---

## 📚 References

- [.NET 10 Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Redis Documentation](https://redis.io/docs/)
- [xUnit Testing](https://xunit.net/)

---

**Last Updated:** January 24, 2026  
**Iteration:** 6 Complete (Redis Caching)  
**Next:** Iteration 7 (Visit Tracking)
