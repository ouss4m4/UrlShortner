# Deployment Guide

## Local Development with Docker

1. **Start all services:**

```bash
docker-compose up -d
```

2. **Run migrations:**

```bash
dotnet ef database update --project API
```

3. **Access the API:**

- API: http://localhost:8080
- PostgreSQL: localhost:5432
- Redis: localhost:6379

4. **Stop services:**

```bash
docker-compose down
```

## Production Deployment

### Railway.app (Recommended - Free Tier Available)

1. **Install Railway CLI:**

```bash
npm install -g @railway/cli
```

2. **Login and initialize:**

```bash
railway login
railway init
```

3. **Add services:**

```bash
railway add postgresql
railway add redis
```

4. **Set environment variables:**

```bash
railway variables set Jwt__Key="your-super-secret-key-at-least-32-characters-long"
railway variables set ASPNETCORE_ENVIRONMENT=Production
```

5. **Deploy:**

```bash
railway up
```

### Render.com (Alternative Free Option)

1. Create new Web Service from Git repo
2. Add PostgreSQL database
3. Add Redis instance
4. Set environment variables in dashboard:
   - `Jwt__Key`
   - `ConnectionStrings__DefaultConnection` (auto-filled)
   - `ConnectionStrings__RedisConnection` (auto-filled)
5. Deploy

### Docker Deployment (Any Platform)

1. **Build image:**

```bash
docker build -t urlshortner-api .
```

2. **Run with environment variables:**

```bash
docker run -d \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=your-db;Database=urlshortner;Username=user;Password=pass" \
  -e ConnectionStrings__RedisConnection="your-redis:6379" \
  -e Jwt__Key="your-secret-key" \
  urlshortner-api
```

### Azure App Service

1. Create App Service (Linux, .NET 10)
2. Add Azure Database for PostgreSQL
3. Add Azure Cache for Redis
4. Configure connection strings in App Service settings
5. Deploy via Docker or Git

## Environment Variables Reference

Required for production:

- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `ConnectionStrings__RedisConnection`: Redis connection string
- `Jwt__Key`: Secret key (min 32 characters)
- `ASPNETCORE_ENVIRONMENT`: Set to "Production"

Optional:

- `Jwt__AccessTokenExpirationMinutes`: Default 60
- `Jwt__RefreshTokenExpirationDays`: Default 7
- `Jwt__Issuer`: Default "UrlShortner"
- `Jwt__Audience`: Default "UrlShortner"

## Database Migrations

After deploying, run migrations:

```bash
# If using Railway
railway run dotnet ef database update --project API

# If using direct connection
dotnet ef database update --project API --connection "your-connection-string"
```

## Health Check

Check if API is running:

```bash
curl http://your-domain:8080/api/url
```

## Security Notes

1. **Never commit secrets** to Git
2. **Use strong JWT keys** in production (32+ characters)
3. **Enable HTTPS** (Railway/Render provide this automatically)
4. **Set proper CORS** origins in production
5. **Use managed database services** (automatic backups)
