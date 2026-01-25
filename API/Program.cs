using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortner.API.Models;
using UrlShortner.API.Data;
using UrlShortner.API.Services;
using API.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IShortCodeGenerator, ShortCodeGenerator>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Register Redis
var isTest = builder.Environment.EnvironmentName == "Test";
if (!isTest)
{
    var redisConnection = ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("RedisConnection") ?? "localhost:6379");
    builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();

    builder.Services.AddDbContext<UrlShortner.API.Data.UrlShortnerDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

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
    var userAgent = httpContext.Request.Headers["User-Agent"].ToString() ?? "unknown";
    var referrer = httpContext.Request.Headers["Referer"].ToString() ?? "";
    var urlId = url.Id;

    // Fire-and-forget visit tracking (non-blocking)
    _ = Task.Run(async () =>
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var visitService = scope.ServiceProvider.GetRequiredService<IVisitService>();

            var visit = new Visit
            {
                UrlId = urlId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Referrer = referrer,
                Country = "", // TODO: GeoIP lookup in future iteration
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
