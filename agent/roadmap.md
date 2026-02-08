# Roadmap (Milestones)

## Phase 1: Core URL Features

- [x] Base62 short codes
- [x] Auto and custom aliases
- [x] URL CRUD + redirect endpoint
- [x] URL expiration and smart TTL
- [x] Categories and tags
- [x] Bulk URL creation

## Phase 2: Security and Reliability

- [x] URL validation (protocol, length, private IP block)
- [x] Short code validation and reserved words
- [x] Redis rate limiting + headers
- [x] HTTPS enforcement (prod only)
- [x] CORS policy allowlist

## Phase 3: Authentication and Authorization

- [x] JWT auth + refresh tokens
- [x] Register and login
- [x] Password hashing
- [x] User isolation guards
- [x] Protected endpoints

## Phase 4: Production Readiness

- [x] Health checks: /health, /health/live, /health/ready
- [x] Structured logging (Serilog + InstanceId)
- [x] Multi-instance API (2 replicas on Railway)
- [ ] Redis multi-instance + consistent hashing
- [ ] Monitoring (Application Insights or similar)
- [ ] DB connection pooling review
- [ ] Load testing (k6)

## Current Status

- Tests: 207 passing
- Deployment: 2 API replicas live
- Next: Redis multi-instance + monitoring
