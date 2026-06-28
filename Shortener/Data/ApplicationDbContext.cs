using Microsoft.EntityFrameworkCore;
using Shortener.Entities;

namespace Shortener.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> context) : DbContext(context)
{
    public DbSet<ShortUrl>  Urls { get; set; }
    public DbSet<User>  Users { get; set; }
    public DbSet<Redirect> Redirects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortUrl>().HasKey(u => u.ShortCode);
        modelBuilder.Entity<ShortUrl>()
            .HasOne(s => s.User)
            .WithMany(u => u.ShortUrls)
            .HasForeignKey(s => s.OwnerId);
        
        base.OnModelCreating(modelBuilder);
    }
}