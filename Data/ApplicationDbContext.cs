using Microsoft.EntityFrameworkCore;
using Shortener.Entities;

namespace Shortener.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> context) : DbContext(context)
{
    public DbSet<ShortUrl>  Urls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortUrl>().HasIndex(u => u.ShortCode).IsUnique();
        
        base.OnModelCreating(modelBuilder);
    }
}