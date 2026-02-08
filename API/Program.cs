using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using UrlShortner.Models;
using UrlShortner.API.Services;
using API.Services;
using StackExchange.Redis;
using UrlShortner.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
// Swagger OpenAPI types removed to avoid build-time package conflicts

var instanceId = Environment.GetEnvironmentVariable("RAILWAY_INSTANCE_ID")
    ?? Environment.GetEnvironmentVariable("RAILWAY_SERVICE_INSTANCE_ID")
    ?? Environment.MachineName;

// Configure Serilog early
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("InstanceId", instanceId)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{InstanceId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Configure HTTPS redirection
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = (int)System.Net.HttpStatusCode.PermanentRedirect;
    options.HttpsPort = 443;
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://example.com", "https://app.example.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "Retry-After");
    });
});

// JWT Authentication Configuration
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT Secret is not configured. Set Jwt:Secret in appsettings.json or environment variables.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// OpenAPI/Swagger Configuration
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IShortCodeGenerator, ShortCodeGenerator>();
builder.Services.AddSingleton<IUrlValidator, UrlValidator>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAnalyticsAggregator, AnalyticsAggregator>();

// Register background service for hourly analytics aggregation
builder.Services.AddHostedService<AnalyticsAggregationHostedService>();

// TODO: To switch to Hangfire for better job management:
// 1. Add NuGet packages: Hangfire.AspNetCore, Hangfire.PostgreSql
// 2. Remove the line above: AddHostedService<AnalyticsAggregationHostedService>()
// 3. Uncomment and configure Hangfire below:
/*
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();
// After app.Run(), add: RecurringJob.AddOrUpdate<IAnalyticsAggregator>("analytics-aggregation", x => x.AggregateHourlyAnalyticsAsync(null), Cron.Hourly);
// And add: app.UseHangfireDashboard(); (visit /hangfire to see dashboard)
*/

// Register GeoIP service with HttpClient
builder.Services.AddHttpClient<IGeoIpService, IpApiGeoIpService>(client =>
{
    // TODO: its bad to resolve ips at rediret time. it should be postop
    // registe raw ip. try locate it later (background)
    // 0 ms added
    client.Timeout = TimeSpan.FromSeconds(5); // Timeout for GeoIP lookups
});

// Register Redis
var isTest = builder.Environment.EnvironmentName == "Test";
if (!isTest)
{
    var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";
    var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
    redisOptions.AbortOnConnectFail = false; // Allow startup even if Redis isn't ready yet
    redisOptions.ConnectRetry = 5;
    redisOptions.ConnectTimeout = 5000;

    var redisConnection = ConnectionMultiplexer.Connect(redisOptions);
    builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
    builder.Services.AddSingleton<IRedisRateLimiter, RedisRateLimiter>();

    builder.Services.AddDbContext<UrlShortner.API.Data.UrlShortnerDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

var app = builder.Build();

// Stamp responses so we can see which instance handled a request
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Instance-Id"] = instanceId;
        return Task.CompletedTask;
    });

    await next();
});

// Apply migrations automatically on startup in Production
var disableMigrations = builder.Configuration.GetValue<bool>("DisableMigrations");
if (app.Environment.IsProduction() && !app.Environment.IsEnvironment("Test") && !disableMigrations)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<UrlShortner.API.Data.UrlShortnerDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "URL Shortener API v1");
        options.RoutePrefix = "swagger"; // Access at /swagger
        options.DocumentTitle = "URL Shortener API";
    });
}

// HTTPS redirection (disabled for Railway/containerized deployments behind reverse proxy)
// Railway, Heroku, etc. handle SSL termination at the edge - the app receives HTTP internally
// Forcing HTTPS redirect causes infinite loops in these environments
var isRailway = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT"));
var disableHttpsRedirection = builder.Configuration.GetValue<bool>("DisableHttpsRedirection") || isRailway;

if (app.Environment.IsProduction() && !disableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

// CORS (before routing and rate limiting)
app.UseCors();

// Serve static files from wwwroot (frontend)
app.UseStaticFiles();

// Authentication & Authorization (before routing)
app.UseAuthentication();
app.UseAuthorization();

// Rate limiting middleware (before routing)
if (!app.Environment.IsEnvironment("Test"))
{
    app.UseMiddleware<RateLimitingMiddleware>();
}

// Root-level redirect endpoint: /{shortCode} -> Original URL
// Route constraint: only match 4-12 alphanumeric chars (excludes frontend routes like 'dashboard', 'assets', etc.)
app.MapGet("/{shortCode:regex(^[a-zA-Z0-9]{{4,12}}$)}", async (string shortCode, IUrlService urlService, HttpContext httpContext, IServiceScopeFactory scopeFactory) =>
{
    var url = await urlService.GetUrlByShortCodeAsync(shortCode);
    if (url == null)
    {
        return Results.NotFound(new { error = "ShortCodeNotFound", message = $"Short code '{shortCode}' does not exist." });
    }

    // Capture HttpContext values BEFORE starting background task (HttpContext will be disposed after response)
    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var userAgent = httpContext.Request.Headers.UserAgent.ToString() ?? "unknown";
    var referrer = httpContext.Request.Headers.Referer.ToString() ?? "";
    var urlId = url.Id;

    // Fire-and-forget visit tracking (non-blocking)
    _ = Task.Run(async () =>
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var visitService = scope.ServiceProvider.GetRequiredService<IVisitService>();
            var geoIpService = scope.ServiceProvider.GetRequiredService<IGeoIpService>();

            // Lookup country from IP address
            var country = await geoIpService.GetCountryCodeAsync(ipAddress);

            var visit = new Visit
            {
                UrlId = urlId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Referrer = referrer,
                Country = country,
                VisitedAt = DateTime.UtcNow
            };

            await visitService.CreateVisitAsync(visit);
        }
        catch (Exception ex)
        {
            // Log but don't fail the redirect
            Console.WriteLine($"Failed to track visit: {ex.Message}");
        }
    });

    return Results.Redirect(url.OriginalUrl, permanent: false);
})
.WithName("RedirectToOriginalUrl")
.WithTags("Redirect");

app.MapControllers();

// Health check endpoints
app.MapGet("/health", (UrlShortner.API.Data.UrlShortnerDbContext? dbContext) =>
{
    var response = new HealthCheckResponse
    {
        Status = "Healthy",
        InstanceId = instanceId,
        Timestamp = DateTime.UtcNow
    };
    return Results.Ok(response);
})
.WithName("Health")
.WithTags("Health")
.Produces<HealthCheckResponse>(StatusCodes.Status200OK);

app.MapGet("/health/live", () =>
{
    // Liveness probe - just check if app is running
    var response = new HealthCheckResponse
    {
        Status = "Healthy",
        InstanceId = instanceId,
        Timestamp = DateTime.UtcNow,
        Details = new Dictionary<string, object> { { "probe", "liveness" } }
    };
    return Results.Ok(response);
})
.WithName("HealthLive")
.WithTags("Health")
.Produces<HealthCheckResponse>(StatusCodes.Status200OK);

app.MapGet("/health/ready", async (UrlShortner.API.Data.UrlShortnerDbContext dbContext, IConnectionMultiplexer redis) =>
{
    // Readiness probe - check dependencies (DB, Redis)
    try
    {
        // Check database connection
        var dbHealthy = await dbContext.Database.CanConnectAsync();

        // Check Redis connection
        var redisHealthy = redis.IsConnected;

        if (dbHealthy && redisHealthy)
        {
            var response = new HealthCheckResponse
            {
                Status = "Healthy",
                InstanceId = instanceId,
                Timestamp = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    { "database", "healthy" },
                    { "redis", "healthy" },
                    { "probe", "readiness" }
                }
            };
            return Results.Ok(response);
        }
        else
        {
            var response = new HealthCheckResponse
            {
                Status = "Unhealthy",
                InstanceId = instanceId,
                Timestamp = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    { "database", dbHealthy ? "healthy" : "unhealthy" },
                    { "redis", redisHealthy ? "healthy" : "unhealthy" },
                    { "probe", "readiness" }
                }
            };
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }
    catch (Exception ex)
    {
        var response = new HealthCheckResponse
        {
            Status = "Unhealthy",
            InstanceId = instanceId,
            Timestamp = DateTime.UtcNow,
            Details = new Dictionary<string, object>
            {
                { "error", ex.Message },
                { "probe", "readiness" }
            }
        };
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
})
.WithName("HealthReady")
.WithTags("Health")
.Produces<HealthCheckResponse>(StatusCodes.Status200OK)
.Produces<HealthCheckResponse>(StatusCodes.Status503ServiceUnavailable);

// SPA fallback - serve index.html for any routes that don't match:
// - Static files (assets/*)
// - API routes (/api/*)
// - Short code redirects (/{shortCode} with regex constraint)
app.MapFallbackToFile("index.html");

app.Run();

// Make Program accessible for integration testing
public partial class Program { }
