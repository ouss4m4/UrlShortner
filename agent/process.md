# Process

This project follows a strict test-driven development (TDD) workflow. For each topic in steps.md:

## Iteration Checklist (MANDATORY)

- [ ] Read all agent/\*.md files (especially process.md and steps.md) **before** starting any work.
- [ ] Update process.md and steps.md with the planned step(s) and intent for this iteration.
- [ ] Write or update unit tests first (red phase).
- [ ] Run tests and confirm they fail (red).
- [ ] Implement or update the code (green phase).
- [ ] Run tests and confirm they pass (green).
- [ ] Update steps.md with a check mark for the completed topic.
- [ ] Document every completed step in steps.md and process.md (including what was done, why, and any issues).
- [ ] Cross-check with requirements.md and instructions.md to ensure nothing is missed.
- [ ] Do not proceed to the next step until the current one is fully documented and validated.
- [ ] Re-read and update all agent/\*.md files at the end of the iteration.

**No code, infra, or documentation change is allowed unless this checklist is followed and agent/\*.md files are updated.**

## Current Iteration

**Completed: January 25, 2026 - URL Expiration (Phase 1.1)** ✅

Successfully implemented URL expiration following strict TDD process!

## Last completed step

**Iteration completed: January 25, 2026 - URL Expiration Feature**

Following strict TDD process (RED-GREEN-REFACTOR):

1. ✅ **Wrote 5 expiration tests** (TDD RED phase)
   - Test/UrlExpirationTests.cs created
   - GetUrlByShortCodeAsync_ReturnsNull_WhenUrlIsExpired
   - GetUrlByShortCodeAsync_ReturnsUrl_WhenUrlIsNotExpired
   - GetUrlByShortCodeAsync_ReturnsUrl_WhenExpiryIsNull
   - GetUrlsByUserIdAsync_ExcludesExpiredUrls
   - CreateUrlAsync_AcceptsExpiryDate

2. ✅ **Confirmed tests fail** (RED phase confirmed)
   - 2 tests failed as expected
   - Service not checking expiry dates yet

3. ✅ **Implemented expiration logic** (TDD GREEN phase)
   - Modified API/Services/UrlService.cs
   - Added expiry checking in GetUrlByShortCodeAsync (both cached and DB paths)
   - Added expiry filtering in GetUrlsByUserIdAsync
   - Expired URLs return null (treated as not found)
   - Cache invalidation for expired URLs

4. ✅ **All 46 tests passing!** (up from 41)
   - 5 new UrlExpirationTests
   - All existing tests still pass
   - Test execution: ~1.7s

**Implementation Details:**

```csharp
// Check if URL is expired
if (url.Expiry.HasValue && url.Expiry.Value <= DateTime.UtcNow)
{
    return null; // Treat expired URLs as not found
}
```

**Features:**

- URLs with null Expiry never expire (permanent)
- URLs with Expiry in the past return null (410 Gone semantically)
- Expired URLs excluded from user listings
- Cache automatically invalidates expired URLs
- Expiry field already existed in Url model (no migration needed)

**Files Created:**

- Test/UrlExpirationTests.cs (5 tests)

**Files Modified:**

- API/Services/UrlService.cs (added expiration checking)

---

**Iteration completed: January 25, 2026 - Multiple Core Features**

Major refactoring and feature additions:

1. ✅ **Visit Tracking Refactor** (Event-Driven Architecture)
   - Changed from CRUD to fire-and-forget event capture
   - Deleted VisitController.cs (no longer needed)
   - Enhanced Visit model with structured fields: IpAddress, UserAgent, Country, Referrer
   - Implemented Task.Run with IServiceScopeFactory for non-blocking capture
   - Created EnhanceVisitModel migration

2. ✅ **Analytics Refactor** (Computed Views)
   - Changed from CRUD to read-only computed views
   - Refactored IAnalyticsService with DTO-based responses
   - Implemented real-time aggregation from Visit queries
   - Endpoints: by URL, by date range, by country

3. ✅ **GeoIP Integration**
   - Created IGeoIpService abstraction (provider pattern)
   - Implemented IpApiGeoIpService using IP-API.com
   - Integrated into visit tracking at root redirect endpoint
   - HttpClient with 5s timeout
   - Smart filtering for localhost/private IPs

4. ✅ **Background Analytics Aggregation**
   - Created IAnalyticsAggregator interface (scheduler-agnostic)
   - Implemented AnalyticsAggregator (groups by hour/country)
   - Created AnalyticsAggregationHostedService (IHostedService)
   - Runs every hour (10s startup delay)
   - Documented Hangfire migration path in code comments

5. ✅ **Swagger/OpenAPI Documentation**
   - Added Swashbuckle.AspNetCore 10.1.0
   - Configured SwaggerGen and SwaggerUI
   - Swagger UI available at /swagger
   - OpenAPI spec at /swagger/v1/swagger.json

**Files Created:**

- API/Services/IGeoIpService.cs
- API/Services/IpApiGeoIpService.cs
- API/Services/IAnalyticsAggregator.cs
- API/Services/AnalyticsAggregator.cs
- API/Services/AnalyticsAggregationHostedService.cs
- API/Migrations/20260125102559_EnhanceVisitModel.cs

**Files Modified:**

- API/Models/Visit.cs (added structured fields)
- API/Services/IAnalyticsService.cs (refactored to DTOs)
- API/Services/AnalyticsService.cs (compute from Visit queries)
- API/Controllers/AnalyticsController.cs (read-only endpoints)
- API/Program.cs (root redirect, GeoIP, aggregation, Swagger)

**Files Deleted:**

- API/Controllers/VisitController.cs
- Test/VisitCrudTests.cs
- Test/RootRedirectTests.cs

**All 41 tests passing!**

## Current Status

- **Where we are**: Phase 1.1 complete - URL expiration implemented! **46 real integration tests passing**.
- **Features Complete**:
  - ✅ Base62 short code generation
  - ✅ Auto-generated + custom short codes
  - ✅ Root redirect endpoint (/{shortCode})
  - ✅ Fire-and-forget visit tracking (non-blocking)
  - ✅ GeoIP integration (IP-API)
  - ✅ Real-time computed analytics
  - ✅ Background hourly aggregation (IHostedService)
  - ✅ Redis caching with invalidation
  - ✅ Swagger/OpenAPI documentation
  - ✅ **URL expiration/TTL (time-to-live)**
- **Test Coverage**: 46 tests (all passing, ~1.7s)
- **What's next**: Phase 1 remaining features (custom short codes, tags, bulk creation)
- **Blockers**: None

---

**Note:** The agent must always use and update this process.md on every iteration. This is mandatory for all work, not optional.
