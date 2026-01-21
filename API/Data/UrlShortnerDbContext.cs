using Microsoft.EntityFrameworkCore;
using UrlShortner.Models;

namespace UrlShortner.Data;

public class UrlShortnerDbContext : DbContext
{
    public UrlShortnerDbContext(DbContextOptions<UrlShortnerDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Url> Urls => Set<Url>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Analytics> Analytics => Set<Analytics>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Url>()
            .HasIndex(u => u.Short)
            .IsUnique();
    }
}
