using Microsoft.EntityFrameworkCore;
using Shortener.Entities;

namespace Shortener.Data;

public class ApplicationDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> context, IConfiguration configuration) : base(context)
    {
        _configuration = configuration;
    }
    
    public DbSet<Url>  Urls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Url>().HasIndex(u => u.ShortenCode).IsUnique();
        
        base.OnModelCreating(modelBuilder);
    }
}