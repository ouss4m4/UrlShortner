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
var isTest = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";
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

app.MapControllers();

app.Run();
