# Process Guidelines

This project follows a strict test-driven development (TDD) workflow.

## Iteration Checklist (MANDATORY)

- [ ] Read all agent/\*.md files (especially process.md and CURRENT_STATUS.md) **before** starting any work.
- [ ] Write or update unit tests first (RED phase).
- [ ] Run tests and confirm they fail (RED).
- [ ] Implement or update the code (GREEN phase).
- [ ] Run tests and confirm they pass (GREEN).
- [ ] Update CURRENT_STATUS.md with completed features
- [ ] Commit changes with clear commit message
- [ ] Document what was done, why, and any issues in CURRENT_STATUS.md
- [ ] Do not proceed to the next step until current one is tested and documented

**No code changes are allowed unless this TDD checklist is followed.**

## Development Workflow

1. **RED**: Write failing tests that specify desired behavior
2. **GREEN**: Implement minimal code to make tests pass
3. **REFACTOR**: Improve code quality while keeping tests green
4. **COMMIT**: Commit working code with all tests passing
5. **DOCUMENT**: Update CURRENT_STATUS.md with what was completed

## Key Principles

- Always write tests before implementation
- Keep tests simple and focused on one behavior
- All tests must pass before committing
- Update documentation after each feature
- Follow layered architecture (Controllers → Services → Data)
- Use dependency injection for all services
- Keep business logic in service layer, not controllers

**All 165 tests passing!**

## Current Status

- **Where we are**: Phase 3 complete - Authentication & Authorization! **202 tests passing**.
- **Features Complete**:
  - ✅ Base62 short code generation
  - ✅ Auto-generated + custom short codes
  - ✅ Root redirect endpoint (/{shortCode})
  - ✅ Fire-and-forget visit tracking (non-blocking)
  - ✅ GeoIP integration (IP-API)
  - ✅ Real-time computed analytics
  - ✅ Background hourly aggregation (IHostedService)
  - ✅ Redis caching with invalidation
  - ✅ URL expiration/TTL with smart cache TTL
  - ✅ Custom short code validation (alphanumeric, reserved words)
  - ✅ URL categories and tags (search & filter)
  - ✅ Bulk URL creation (batch import with partial success)
  - ✅ Redis-backed distributed rate limiting (fixed window)
  - ✅ Comprehensive input validation (URL format, localhost blocking)
  - ✅ HTTPS enforcement (Production-only, 308 redirect)
  - ✅ CORS policy (origin allowlist, credentials, exposed headers)
  - ✅ JWT authentication & token management
  - ✅ User registration/login with password hashing
  - ✅ Protected endpoints with [Authorize] attribute
  - ✅ User isolation (URL ownership validation)
  - ✅ Swagger/OpenAPI documentation
- **Test Coverage**: 202 tests (all passing, ~7s)
- **What's next**: Phase 4 - Production Readiness (health checks, logging, monitoring, connection pooling)
- **Blockers**: None

---

**Note:** The agent must always use and update this process.md on every iteration. This is mandatory for all work, not optional.
