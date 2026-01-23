# 🚀 Running the URL Shortener API

## Prerequisites

1. **Docker & Docker Compose** - For Postgres and Redis
2. **.NET 10 SDK** - For running the API

## Quick Start

### 1. Start Docker Services

```bash
cd /Users/ouss/RiderProjects/UrlShortner
docker-compose up -d
```

This starts:

- PostgreSQL on port 5432
- Redis on port 6379

### 2. Apply Database Migrations (if needed)

```bash
cd API
dotnet ef database update
```

### 3. Start the API Server

```bash
cd API
dotnet run
```

The API will start on: **http://localhost:5000**

### 4. Run Tests

```bash
# Run all tests
cd Test
dotnet test

# Run specific test categories
dotnet test --filter "FullyQualifiedName~CacheServiceTests"
dotnet test --filter "FullyQualifiedName~UrlCachingTests"
```

## Testing the API

### Option 1: Use the Test Script

```bash
./test-api.sh
```

This script will:

- Create URLs with auto-generated and custom short codes
- Test the redirect endpoint
- Demonstrate collision detection (409 errors)
- Show base62 encoding in action

### Option 2: Manual cURL Commands

#### Create a URL (auto-generated short code)

```bash
curl -X POST http://localhost:5000/api/url \
  -H "Content-Type: application/json" \
  -d '{
    "originalUrl": "https://github.com/dotnet/aspnetcore",
    "userId": 1
  }'
```

#### Create a URL with custom short code

```bash
curl -X POST http://localhost:5000/api/url \
  -H "Content-Type: application/json" \
  -d '{
    "originalUrl": "https://github.com",
    "shortCode": "gh",
    "userId": 1
  }'
```

#### Get URL by short code

```bash
curl http://localhost:5000/api/url/short/gh
```

#### Redirect via short code

```bash
curl -I http://localhost:5000/api/url/redirect/gh
```

#### Get all URLs for a user

```bash
curl http://localhost:5000/api/url/user/1
```

## Verifying Redis Caching

### Check Cache Hit/Miss

1. Create a URL and note the short code
2. First GET request → queries database (~10-20ms)
3. Second GET request → returns from cache (~1-2ms)

### Inspect Redis Cache

```bash
# Connect to Redis
docker exec -it urlshortner_redis redis-cli

# List all cached URL keys
KEYS url:shortcode:*

# Get a specific cached URL
GET url:shortcode:1

# Check TTL (should be ~3600 seconds = 1 hour)
TTL url:shortcode:1

# Clear cache (for testing)
FLUSHDB
```

## API Endpoints

### URL Endpoints

- `POST /api/url` - Create URL
- `GET /api/url/{id}` - Get URL by ID
- `GET /api/url/short/{shortCode}` - Get URL by short code (cached)
- `GET /api/url/redirect/{shortCode}` - Redirect to original URL (302)
- `GET /api/url/user/{userId}` - Get all URLs for user
- `PUT /api/url/{id}` - Update URL (invalidates cache)
- `DELETE /api/url/{id}` - Delete URL (invalidates cache)

### User Endpoints

- `POST /api/user` - Create user
- `GET /api/user/{id}` - Get user
- `PUT /api/user/{id}` - Update user
- `DELETE /api/user/{id}` - Delete user

### Visit Endpoints

- `POST /api/visit` - Create visit record
- `GET /api/visit/url/{urlId}` - Get visits for URL
- `DELETE /api/visit/{id}` - Delete visit

### Analytics Endpoints

- `POST /api/analytics` - Create analytics record
- `GET /api/analytics/date/{date}` - Get analytics by date
- `DELETE /api/analytics/{id}` - Delete analytics

## Architecture Highlights

### Caching Strategy

- **Pattern**: Cache-Aside (Lazy Loading)
- **Key Format**: `url:shortcode:{code}`
- **TTL**: 1 hour
- **Invalidation**: Automatic on update/delete operations

### Base62 Encoding

- **Character Set**: 0-9, a-z, A-Z (62 characters)
- **Example**: URL ID 1 → "1", ID 62 → "10", ID 100 → "1C"
- **URL-safe**: No special characters needed

### Error Handling

- **409 Conflict**: Duplicate short code
- **404 Not Found**: Short code doesn't exist
- **400 Bad Request**: Invalid input

## Performance

- **Without Cache**: ~10-20ms per short code lookup
- **With Cache**: ~1-2ms per short code lookup (10x-20x faster!)
- **Cache Hit Ratio**: Typically 90%+ for popular URLs

## Stopping Services

```bash
# Stop API server: Ctrl+C

# Stop Docker services
docker-compose down

# Stop and remove volumes (clears database)
docker-compose down -v
```

## Troubleshooting

### Port Already in Use

```bash
# Check what's using port 5000
lsof -i :5000

# Kill the process
kill -9 <PID>
```

### Database Connection Issues

```bash
# Restart Docker services
docker-compose restart

# Check logs
docker-compose logs postgres
```

### Redis Connection Issues

```bash
# Check Redis is running
docker exec -it urlshortner_redis redis-cli ping
# Should return: PONG

# Check Redis logs
docker-compose logs redis
```

## Next Steps

1. **Visit Tracking**: Record visitor metadata (IP, User-Agent, Country) on redirects
2. **Rate Limiting**: Add per-IP and per-user rate limits
3. **Authentication**: Add JWT authentication
4. **Swagger/OpenAPI**: Add interactive API documentation
5. **Monitoring**: Add logging and metrics collection
