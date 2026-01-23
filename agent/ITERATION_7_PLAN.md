# Iteration 7 - Visit Tracking Enhancement

**Status:** 📋 Planned (Not Started)  
**Start Date:** TBD  
**Goal:** Record visitor metadata on redirect for analytics and tracking

---

## 🎯 Objectives

1. Capture visitor information when users access shortened URLs
2. Store metadata (IP address, User-Agent, timestamp) in Visit entity
3. Optional: Add GeoIP lookup for country detection
4. Update Analytics aggregates with visit data
5. Follow strict TDD: Write tests first, implement second

---

## 📋 Requirements

### Functional Requirements

1. **Capture on Redirect**

   - When GET /api/url/redirect/{shortCode} is called
   - Extract IP address from HTTP context
   - Extract User-Agent header
   - Record timestamp (UTC)

2. **Create Visit Record**

   - Store in Visit entity
   - Link to URL by UrlId
   - Optional: Link to User by UserId (if authenticated)
   - Store metadata as JSON or separate fields

3. **GeoIP Lookup (Optional)**

   - Install GeoIP library (MaxMind.GeoIP2 recommended)
   - Convert IP address to Country code
   - Store in Visit entity or Metadata field

4. **Update Analytics (Optional)**
   - Increment TotalVisits counter
   - Store per-country visit counts
   - Update last visit timestamp

### Non-Functional Requirements

1. **Performance**

   - Visit creation should not slow down redirects
   - Consider async/fire-and-forget pattern
   - Target: <5ms overhead for redirect

2. **Privacy**

   - Hash or anonymize IP addresses (optional)
   - Comply with GDPR/privacy regulations
   - Document data retention policy

3. **Testing**
   - All features must have tests first (TDD)
   - Real integration tests (no mocks)
   - Test IP extraction, User-Agent parsing, Visit creation

---

## 🔄 TDD Workflow (Strict RED-GREEN-REFACTOR)

### Phase 1: RED (Write Failing Tests)

**Test File:** `Test/VisitTrackingTests.cs`

Tests to write:

1. `RedirectCreatesVisitRecord()` - Verify Visit is created on redirect
2. `VisitContainsIpAddress()` - Verify IP is captured
3. `VisitContainsUserAgent()` - Verify User-Agent is captured
4. `VisitContainsTimestamp()` - Verify timestamp is recorded
5. `VisitLinksToCorrectUrl()` - Verify UrlId is correct
6. `MultipleRedirectsCreateMultipleVisits()` - Verify each redirect creates a new Visit

**Expected Result:** All 6 tests fail (red phase)

### Phase 2: GREEN (Implement Features)

**Files to Modify:**

1. **API/Controllers/UrlController.cs**

   - Modify `Redirect(string shortCode)` method
   - Extract IP from HttpContext.Connection.RemoteIpAddress
   - Extract User-Agent from HttpContext.Request.Headers["User-Agent"]
   - Call VisitService to create Visit record

2. **API/Services/VisitService.cs**

   - Add `CreateVisitFromRedirectAsync(int urlId, string ipAddress, string userAgent)`
   - Create Visit entity with metadata
   - Save to database

3. **API/Models/Visit.cs** (verify structure)
   - Ensure UrlId, IpAddress, UserAgent, VisitedAt fields exist
   - Add Metadata field (JSON) if needed

**Expected Result:** All 6 tests pass (green phase)

### Phase 3: REFACTOR (Optional)

- Extract IP/User-Agent logic to helper class
- Add validation/sanitization
- Optimize database writes
- Add logging

---

## 📦 Dependencies (If Needed)

### GeoIP Library (Optional):

```bash
cd API
dotnet add package MaxMind.GeoIP2
```

**Configuration:**

- Download GeoLite2 database (free)
- Store in API/Data/GeoLite2-Country.mmdb
- Initialize GeoIP2Reader in DI

**Alternative:** Use free API service (ip-api.com, ipinfo.io)

---

## 🧪 Test Plan

### Unit Tests (TDD Approach)

**Test/VisitTrackingTests.cs** - 6 tests

1. **RedirectCreatesVisitRecord**

   - Setup: Create URL in DB, get short code
   - Action: Call GET /api/url/redirect/{shortCode}
   - Assert: Visit record exists in DB with correct UrlId

2. **VisitContainsIpAddress**

   - Setup: Mock HttpContext with IP address
   - Action: Call redirect endpoint
   - Assert: Visit.IpAddress matches request IP

3. **VisitContainsUserAgent**

   - Setup: Mock HttpContext with User-Agent header
   - Action: Call redirect endpoint
   - Assert: Visit.UserAgent matches request header

4. **VisitContainsTimestamp**

   - Setup: Record current time
   - Action: Call redirect endpoint
   - Assert: Visit.VisitedAt is within 1 second of current time

5. **VisitLinksToCorrectUrl**

   - Setup: Create 2 URLs in DB
   - Action: Redirect to URL 1
   - Assert: Visit.UrlId == URL 1 ID (not URL 2)

6. **MultipleRedirectsCreateMultipleVisits**
   - Setup: Create URL in DB
   - Action: Call redirect 3 times
   - Assert: 3 Visit records exist with same UrlId

### Manual Testing

After implementation:

```bash
# Test 1: Redirect and verify Visit created
curl -i http://localhost:5011/api/url/redirect/1

# Test 2: Check database for Visit records
docker exec -it urlshortner_postgres psql -U urlshortner -d urlshortner_db
SELECT * FROM "Visits";

# Test 3: Verify IP and User-Agent captured
curl -H "User-Agent: TestBot/1.0" http://localhost:5011/api/url/redirect/1
SELECT "IpAddress", "UserAgent" FROM "Visits" WHERE "UrlId" = 1;
```

---

## 📊 Success Criteria

- [ ] All 6 new tests pass (RED → GREEN)
- [ ] All 44 existing tests still pass (no regressions)
- [ ] Visit records are created on every redirect
- [ ] IP address and User-Agent are captured correctly
- [ ] Redirect performance remains <50ms (with Visit creation)
- [ ] Manual testing confirms Visit data is accurate
- [ ] Documentation updated (process.md, CURRENT_STATUS.md, steps.md)
- [ ] Git commit with descriptive message

---

## 📝 Implementation Steps

### Step 1: Read Documentation (5 minutes)

- [ ] Read agent/process.md (TDD workflow)
- [ ] Read this file (ITERATION_7_PLAN.md)
- [ ] Review agent/CURRENT_STATUS.md

### Step 2: Verify Environment (2 minutes)

- [ ] Run `dotnet test` - expect 44/44 passing
- [ ] Run `docker-compose ps` - expect Postgres + Redis healthy

### Step 3: RED Phase - Write Tests (20 minutes)

- [ ] Create Test/VisitTrackingTests.cs
- [ ] Write all 6 tests (listed above)
- [ ] Run tests - expect all 6 to FAIL (red phase)
- [ ] Commit: "test: Add 6 visit tracking tests (RED phase)"

### Step 4: GREEN Phase - Implement (30 minutes)

- [ ] Modify API/Controllers/UrlController.cs
- [ ] Modify API/Services/VisitService.cs (or create if needed)
- [ ] Add helper methods for IP/User-Agent extraction
- [ ] Run tests - expect all 50 to PASS (green phase)
- [ ] Commit: "feat: Implement visit tracking on redirect"

### Step 5: Manual Testing (10 minutes)

- [ ] Start server: `./start-server.sh`
- [ ] Test redirects with curl
- [ ] Verify Visit records in database
- [ ] Document results in TEST_RESULTS.md

### Step 6: Documentation (10 minutes)

- [ ] Update agent/process.md with iteration summary
- [ ] Update agent/CURRENT_STATUS.md with new features
- [ ] Update agent/steps.md with checkmarks
- [ ] Create agent/iteration-7-summary.md
- [ ] Commit: "docs: Document iteration 7 completion"

---

## 🚨 Potential Issues

### Issue 1: HttpContext Not Available in Tests

**Problem:** Integration tests may not have real HttpContext.

**Solution:**

- Use Microsoft.AspNetCore.Mvc.Testing for integration tests
- Or mock HttpContext with test IP/User-Agent values
- Or extract IP/User-Agent logic to testable helper methods

### Issue 2: Visit Creation Slows Down Redirects

**Problem:** Synchronous DB writes add latency.

**Solution:**

- Keep synchronous for now (simple, reliable)
- Consider async/fire-and-forget in future
- Measure performance: <50ms is acceptable

### Issue 3: IP Address Privacy Concerns

**Problem:** Storing full IP addresses may violate GDPR.

**Solution:**

- Hash IP addresses before storage
- Or truncate last octet (e.g., 192.168.1.XXX)
- Document in privacy policy
- Consider anonymization in future iteration

---

## 📚 Reference Documentation

- **Current State:** agent/CURRENT_STATUS.md
- **TDD Workflow:** agent/process.md
- **Previous Iteration:** agent/redis-caching-summary.md
- **Testing Strategy:** agent/test-strategy.md
- **Models:** API/Models/Visit.cs, API/Models/Url.cs

---

## 🎓 Key Design Decisions

### Decision 1: Synchronous vs Async Visit Creation

**Chosen:** Synchronous (for now)

**Rationale:**

- Simpler implementation
- Reliable (no lost visits)
- Acceptable performance (<50ms)
- Can optimize later if needed

### Decision 2: IP Storage Format

**Chosen:** Store full IP as string

**Rationale:**

- Simple, no conversion needed
- Useful for debugging
- Can hash/anonymize in future
- Privacy policy required

### Decision 3: User-Agent Storage

**Chosen:** Store raw User-Agent header

**Rationale:**

- Maximum flexibility
- Can parse later for device/browser stats
- Standard practice
- No parsing library needed yet

### Decision 4: GeoIP Integration

**Chosen:** Skip for now (optional)

**Rationale:**

- Not required for MVP
- Adds complexity (database download, library integration)
- Can add in iteration 8 or later
- Focus on core functionality first

---

## ✅ Checklist Before Starting

- [ ] Read this entire plan
- [ ] Understand TDD workflow (RED → GREEN → REFACTOR)
- [ ] Verify all 44 tests passing
- [ ] Docker services running (Postgres + Redis)
- [ ] Ready to follow strict TDD process
- [ ] Know where to update documentation

---

**Ready to start? Begin with Step 3: RED Phase - Write Tests!** 🚀
