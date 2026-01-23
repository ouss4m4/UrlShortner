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

**Starting: January 23, 2026 - Redis Caching Integration**

Following strict TDD process to integrate Redis for fast shortcode lookups:

**Plan:**
1. Add Redis connection to docker-compose.yaml (already exists)
2. Install StackExchange.Redis package
3. Create ICacheService interface
4. Create RedisCacheService implementation
5. Write tests for cache hit/miss scenarios
6. Update UrlService to use caching on lookups
7. Add cache invalidation on URL updates/deletes
8. Register CacheService in DI (Program.cs)
9. Run all tests and verify they pass

**Expected Outcome:**
- Short code lookups check Redis first before database
- Cache automatically populated on first lookup (cache-aside pattern)
- Cache invalidated when URLs are updated or deleted
- All existing tests continue to pass + new caching tests pass

## Last completed step

**Iteration completed: January 23, 2026 - Collision Detection & Error Handling**

Following strict TDD process for short code collision handling:

1. ✅ **Added collision detection test** (TDD RED phase)
   - ThrowsExceptionWhenCustomShortCodeAlreadyExists test in UrlCrudTests
2. ✅ **Implemented duplicate short code detection** (TDD GREEN phase)
   - Pre-insert check for existing short codes in UrlService
   - Throws InvalidOperationException with clear message
   - Works with both InMemory (tests) and Postgres (production) databases
   - Also catches DbUpdateException for database-level unique constraint violations
3. ✅ **Added controller error handling**:
   - Try-catch block in UrlController.Create()
   - Returns 409 Conflict status code with structured JSON error
   - Error includes: error code, message, and shortCode fields
4. ✅ **Created UrlControllerTests.cs** with 3 integration tests:
   - CreateReturns409ConflictWhenShortCodeExists
   - CreateReturns201CreatedWhenShortCodeIsUnique
   - CreateAutoGeneratesShortCodeWhenEmpty
5. ✅ **All 34 tests passing!** (up from 30)

**Files Created/Modified:**
- Modified: API/Services/UrlService.cs (added duplicate detection)
- Modified: API/Controllers/UrlController.cs (added error handling)
- Modified: Test/UrlCrudTests.cs (added collision test)
- Created: Test/UrlControllerTests.cs (3 new controller tests)

**Error Response Format for UI:**
```json
HTTP 409 Conflict
{
  "error": "ShortCodeAlreadyExists",
  "message": "Short code 'fb' is already taken. Please choose a different short code.",
  "shortCode": "fb"
}
```

**Previous Iteration: URL Shortening & Redirect Implementation**

1. ✅ Created IShortCodeGenerator interface with Encode/Decode methods
2. ✅ Wrote 6 tests for base62 encoding (TDD RED phase)
3. ✅ Implemented ShortCodeGenerator with base62 algorithm (TDD GREEN phase)
4. ✅ Integrated into UrlService (auto-generates short codes from URL ID)
5. ✅ Updated all UrlCrudTests + added 2 new tests
6. ✅ Created UrlRedirectTests.cs with 3 tests
7. ✅ Added redirect endpoint (GET /api/url/redirect/{shortCode})
8. ✅ Registered ShortCodeGenerator in DI as Singleton
9. ✅ All 30 tests passing

## Current Status

- **Where we are**: Core URL shortening with collision detection complete. **34 real integration tests passing**.
- **Features Complete**:
  - ✅ Base62 short code generation (0-9, a-z, A-Z)
  - ✅ Auto-generated short codes from database IDs
  - ✅ Manual/custom short code support (aliases)
  - ✅ Duplicate short code detection with user-friendly errors
  - ✅ URL expansion by short code
  - ✅ Redirect endpoint (302 to original URL)
  - ✅ Proper error handling (409 Conflict for duplicates)
- **Test Coverage**: 34 tests across 8 test files
  - All tests use real InMemory database (no mocks)
  - Fast execution (~220ms total)
  - Comprehensive coverage: CRUD + shortening + redirect + error handling
- **What's next**:
  1. **[NEXT]** Integrate Redis for caching shortcode lookups
  2. Add visit tracking on redirect (record IP, browser, country)
  3. Add basic rate limiting middleware
  4. Add Swagger/OpenAPI documentation
  5. Add authentication and admin features
- **Blockers**: None. Ready to add caching layer for performance.

---

**Note:** The agent must always use and update this process.md and steps.md on every iteration. This is mandatory for all work, not optional.
