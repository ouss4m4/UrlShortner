using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using UrlShortner.Models;
using UrlShortner.API.Services;
using API.Services;
using StackExchange.Redis;
using UrlShortner.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// OpenAPI/Swagger Configuration
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IShortCodeGenerator, ShortCodeGenerator>();
builder.Services.AddSingleton<IUrlValidator, UrlValidator>();
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
    var redisConnection = ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("RedisConnection") ?? "localhost:6379");
    builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
    builder.Services.AddSingleton<IRedisRateLimiter, RedisRateLimiter>();

    builder.Services.AddDbContext<UrlShortner.API.Data.UrlShortnerDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

var app = builder.Build();

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

app.UseHttpsRedirection();

// Rate limiting middleware (before routing)
if (!app.Environment.IsEnvironment("Test"))
{
    app.UseMiddleware<RateLimitingMiddleware>();
}

// Root-level redirect endpoint: /{shortCode} -> Original URL
// This is the main purpose of a URL shortener - keep it SHORT!
app.MapGet("/{shortCode}", async (string shortCode, IUrlService urlService, HttpContext httpContext, IServiceScopeFactory scopeFactory) =>
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

app.Run();

// Make Program accessible for integration testing
public partial class Program { }
