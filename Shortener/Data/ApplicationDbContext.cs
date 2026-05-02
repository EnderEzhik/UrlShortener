using Microsoft.EntityFrameworkCore;
using Shortener.DTOs;
using Shortener.Entities;

namespace Shortener.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> context) : DbContext(context)
{
    public DbSet<ShortUrl>  Urls { get; set; }
    public DbSet<User>  Users { get; set; }
    public DbSet<ClickAnalytics> Clicks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortUrl>().HasKey(u => u.ShortCode);
        
        base.OnModelCreating(modelBuilder);
    }
}