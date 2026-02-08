# Multi-Instance Deployment Guide (Railway)

## Current Setup

- **API Instances**: 2 (configured in railway.json)
- **Database**: PostgreSQL (Railway managed)
- **Cache**: Redis (Railway managed)
- **Load Balancer**: Railway built-in

## How It Works

### 1. Load Balancing (Railway Built-in)

- Railway automatically distributes traffic across 2 API instances
- Uses `/health/ready` endpoint to check instance health
- Only routes traffic to healthy instances
- Instances have different `MachineName` (InstanceId)

### 2. Shared Resources

Both API instances connect to the **same shared**:

- **PostgreSQL database** - Single source of truth for all data
- **Redis cache** - Consistent hashing prevents data duplication

### 3. Logging & Tracing

All logs include:

- `InstanceId` - Which instance generated the log (API-1, API-2)
- `Timestamp` - When the event occurred
- `Level` - INFO, ERROR, etc.
- `Message` - What happened

Example log from Instance 1:

```
[14:23:45 INF] [api-server-1] Created URL shortcode: abc123
[14:23:46 INF] [api-server-1] Cached shortcode in Redis
```

Instance 2 handles next request:

```
[14:23:47 INF] [api-server-2] Retrieved shortcode: abc123 (from Redis cache)
[14:23:48 INF] [api-server-2] Redirected to original URL
```

### 4. Consistent Hashing for Redis

When you deploy 2 Redis instances (Phase 4.2):

- Keys are distributed using consistent hashing algorithm
- If Redis-1 fails, Redis-2 still has ~50% of keys
- Cache miss triggers DB lookup (fallback)
- No data loss, graceful degradation

## Deployment Steps

### Step 1: Ensure Railway Services Exist

On Railway dashboard:

1. Your API service ✓
2. Your PostgreSQL service (should exist)
3. Your Redis service (should exist)

Verify environment variables are connected:

- `ConnectionStrings__DefaultConnection` - Points to PostgreSQL
- `ConnectionStrings__RedisConnection` - Points to Redis

### Step 2: Deploy Multi-Instance Setup

**Option A: Using Dashboard (Manual)**

1. Go to Railway project → API service
2. Click "Scale"
3. Set replicas to 2
4. Click "Apply"

**Option B: Using railway.json (Already Done)**

1. Config already specifies `"replicas": 2`
2. Push to GitHub
3. Railway auto-deploys with 2 instances

### Step 3: Verify Deployment

Check health endpoint:

```bash
# Both instances should respond
curl https://urlshortner-production-ae23.up.railway.app/health
curl https://urlshortner-production-ae23.up.railway.app/health/live
curl https://urlshortner-production-ae23.up.railway.app/health/ready
```

Expected response:

```json
{
  "status": "Healthy",
  "instanceId": "railway-container-1",
  "timestamp": "2026-02-08T14:23:45Z"
}
```

### Step 4: Monitor Instance Distribution

Make multiple requests and check logs:

```bash
for i in {1..10}; do
  curl https://urlshortner-production-ae23.up.railway.app/api/url/test
done
```

Check logs - you should see requests split between instances:

```
[Instance: api-1] Request 1, 3, 5, 7, 9
[Instance: api-2] Request 2, 4, 6, 8, 10
```

## Consistent Hashing Setup (Phase 4.2)

When deploying 2 Redis instances:

```javascript
// Client side (API)
var endpoints = [new EndPoint("redis-1.railway.internal", 6379), new EndPoint("redis-2.railway.internal", 6379)];

var options = ConfigurationOptions.Parse("redis-1,redis-2");
options.ServiceName = "urlshortner";

// Redis automatically uses consistent hashing
var connection = ConnectionMultiplexer.Connect(options);
```

Key distribution example:

```
Key "short_abc123" → Redis-1 (hash bucket 5)
Key "short_def456" → Redis-2 (hash bucket 15)
Key "short_ghi789" → Redis-1 (hash bucket 8)
```

If Redis-1 fails:

- All keys on Redis-1 become unavailable
- App falls back to database
- No errors, just slightly slower performance
- When Redis-1 comes back, keys are re-cached

## Monitoring

### Key Metrics

- Request distribution (should be ~50/50 between instances)
- Cache hit rate (should improve over time as both instances cache)
- Error rate (should stay ~0%)
- Response time (should be <200ms consistent)

### Logging

- All logs tagged with InstanceId
- Centralized logging makes it easy to see which instance failed
- Can filter logs by instance for debugging

## Troubleshooting

### Instance keeps crashing?

- Check `/health/ready` - is it returning 503?
- Verify database connectivity
- Verify Redis connectivity
- Check logs for `InstanceId` to see which crashed

### Cache not working?

- Check if Redis service is healthy
- Verify `ConnectionStrings__RedisConnection` env var
- Check logs for "Failed to connect to Redis"

### Load not balanced?

- Check if one instance is healthier
- Both should respond to `/health/ready`
- Railway health check timeout might be too short

## Next Steps

**Phase 4.2**: Deploy 2 Redis instances with consistent hashing
**Phase 4.3**: Application Insights / Monitoring
**Phase 4.4**: Performance load testing with k6
