# Steps (Roadmap)

## 1. Project & Infrastructure Setup

- [x] Initialize .NET minimal API project
- [x] Add Docker Compose for Postgres & Redis
- [x] Configure connection strings

## 2. Core Entities & Database

- [x] Define User, URL, Visit, Analytics models
- [x] Set up EF Core migrations for Postgres

## 3. API & Logic

- [x] User CRUD unit tests (TDD red/green)
- [ ] Implement CRUD endpoints for User
- [ ] Implement CRUD endpoints for URL (shorten, expand, delete, list)
- [ ] Implement Visit tracking
- [ ] Implement Analytics rollup

## 4. Caching & Rate Limiting

- [ ] Integrate Redis for shorturl lookups
- [ ] Add basic rate limiting

## 5. Admin Features

- [ ] Admin endpoints for analytics and CRUD

## 6. API Documentation

- [ ] Add Swagger/OpenAPI (swagger.json, Swagger UI)

## 7. Testing

- [x] Write unit tests for core logic (mocked, no DB)
- [ ] Write integration tests (deferred until service layer exists)

## 8. Deployment

- [ ] Dockerize application
- [ ] Prepare for scaling (Redis, DB)

---

Check off each item after tests pass and with user approval.
