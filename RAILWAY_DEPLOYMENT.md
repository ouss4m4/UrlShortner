# Railway Deployment Guide

## Prerequisites

- GitHub account
- Railway account (sign up at https://railway.app with GitHub)

## Step 1: Push to GitHub

```bash
# If you haven't already pushed
git remote -v  # Check if remote exists
git push origin main
```

## Step 2: Create Railway Project

1. Go to https://railway.app
2. Click "New Project"
3. Select "Deploy from GitHub repo"
4. Choose your `UrlShortner` repository
5. Railway will detect the Dockerfile automatically

## Step 3: Add PostgreSQL Database

1. In your Railway project, click "+ New"
2. Select "Database" → "PostgreSQL"
3. Railway will automatically create a PostgreSQL instance
4. Connection string will be available as `DATABASE_URL`

## Step 4: Add Redis

1. Click "+ New" again
2. Select "Database" → "Redis"
3. Railway will provision Redis
4. Connection string available as `REDIS_URL`

## Step 5: Configure Environment Variables

Click on your API service → "Variables" tab → Add these:

```bash
# Connection Strings (Railway auto-injects DATABASE_URL and REDIS_URL)
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
ConnectionStrings__RedisConnection=${{Redis.REDIS_URL}}

# JWT Configuration (CRITICAL - generate a secure random string!)
Jwt__Secret=YOUR_SUPER_SECRET_KEY_MINIMUM_32_CHARACTERS_LONG
Jwt__Issuer=UrlShortner
Jwt__Audience=UrlShortner

# ASP.NET Environment
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080

# Optional: Rate Limiting
RateLimiting__PermitLimit=100
RateLimiting__WindowMinutes=1
```

### Generate JWT Secret

Run this to generate a secure secret:

```bash
openssl rand -base64 32
```

## Step 6: Deploy

Railway will automatically deploy when you push to GitHub!

```bash
git add railway.json RAILWAY_DEPLOYMENT.md
git commit -m "Add Railway deployment config"
git push origin main
```

Railway will:

1. Detect the Dockerfile
2. Build your Docker image
3. Deploy to a public URL (e.g., `https://your-app.railway.app`)
4. Auto-redeploy on every push to main

## Step 7: Apply Database Migrations

After first deployment, you need to run migrations:

### Option A: Using Railway CLI

```bash
# Install Railway CLI
npm i -g @railway/cli  # or: brew install railway

# Login
railway login

# Link to your project
railway link

# Run migrations
railway run dotnet ef database update --project API/UrlShortner.csproj
```

### Option B: Using Railway Shell

1. Go to your API service in Railway dashboard
2. Click "Settings" → "Deploy"
3. Under "Deploy Logs", find the deployment
4. Click the three dots → "Shell"
5. Run:
   ```bash
   cd /app
   dotnet ef database update
   ```

### Option C: Add migration to Dockerfile (Automated)

Edit Dockerfile to run migrations on startup (see Dockerfile.migrations example)

## Step 8: Test Your Deployment

```bash
# Get your Railway URL from the dashboard
export RAILWAY_URL="https://your-app.railway.app"

# Test registration
curl -X POST $RAILWAY_URL/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@test.com","password":"Test123!"}'

# Test URL shortening (use the access token from registration)
curl -X POST $RAILWAY_URL/api/url \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -d '{"originalUrl":"https://example.com","alias":"mylink"}'
```

## Free Tier Limits

Railway free tier includes:

- $5 free credit per month
- PostgreSQL database (500MB storage)
- Redis (25MB storage)
- 500GB bandwidth
- Auto-sleep after inactivity (can be disabled in settings)

## Monitoring

Railway provides:

- Real-time logs (click on service → "Deployments" → "View Logs")
- Metrics (CPU, Memory, Network usage)
- Deployment history
- Automatic HTTPS certificates

## Domain Setup (Optional)

1. Go to your service → "Settings" → "Domains"
2. Click "Generate Domain" for a Railway subdomain
3. Or add your custom domain

## Troubleshooting

### Build Fails

- Check "Deploy Logs" for errors
- Verify Dockerfile builds locally: `docker build -t test .`

### App Crashes

- Check "Deploy Logs" and "Runtime Logs"
- Verify environment variables are set correctly
- Check connection strings format

### Database Connection Fails

- Ensure PostgreSQL service is running
- Verify `ConnectionStrings__DefaultConnection` is set
- Check if migrations were applied

### Redis Connection Fails

- Ensure Redis service is running
- Verify `ConnectionStrings__RedisConnection` is set
- Format should be: `host:port` (Railway provides this automatically)

## Next Steps

Once deployed:

1. Update CORS origins in Program.cs with your Railway URL
2. Set up custom domain
3. Enable branch deployments for staging
4. Configure GitHub Actions for CI/CD
5. Build frontend and deploy separately
