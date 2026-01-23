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

## Last completed step

- Fixed all namespace issues across the project to use `UrlShortner.API.*` convention.
- Updated all using directives in Models, Data, Services, Controllers, Migrations, and Tests.
- Fixed model property names (`OriginalUrl` and `ShortCode` in Url model, added `UserId` to Visit model).
- Removed outdated UserApiTests.cs (DbContext mocking issues); kept UserCrudTests.cs (integration tests).
- **All 15 tests now pass successfully!**
- Build successful with no errors (only package version warnings which are expected).

## Current Status

- **Where we are**: User, URL, and Visit CRUD services, controllers, and tests are fully implemented, tested, and passing.
- **What's next**: Implement Analytics CRUD (AnalyticsController, AnalyticsService, IAnalyticsService) following the same TDD pattern.
- **Blockers**: None. Ready to proceed with Analytics implementation.

---

**Note:** The agent must always use and update this process.md and steps.md on every iteration. This is mandatory for all work, not optional.
