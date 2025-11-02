using Microsoft.EntityFrameworkCore;
using Shortener.Entities;

namespace Shortener.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> context) : DbContext(context)
{
    public DbSet<ShortUrl>  Urls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortUrl>().HasKey(u => u.ShortCode);
        
        base.OnModelCreating(modelBuilder);
    }
}