using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortner.Models;
using UrlShortner.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var isTest = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";
if (!isTest)
{
    builder.Services.AddDbContext<UrlShortner.Data.UrlShortnerDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.MapPost("/api/users", async ([FromBody] User user, [FromServices] UrlShortnerDbContext db) =>
{
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(user);
});

app.MapGet("/api/users/{id}", async (int id, [FromServices] UrlShortnerDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    return user == null ? Results.NotFound() : Results.Ok(user);
});

app.MapPut("/api/users/{id}", async (int id, [FromBody] User updated, [FromServices] UrlShortnerDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user == null) return Results.NotFound();
    user.Username = updated.Username;
    user.Email = updated.Email;
    db.Users.Update(user);
    await db.SaveChangesAsync();
    return Results.Ok(user);
});

app.MapDelete("/api/users/{id}", async (int id, [FromServices] UrlShortnerDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user == null) return Results.NotFound();
    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}