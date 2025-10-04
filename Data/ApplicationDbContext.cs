using Microsoft.EntityFrameworkCore;
using Shortener.Entities;

namespace Shortener.Data;

public class ApplicationDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    
    public ApplicationDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public DbSet<Url>  Urls { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_configuration.GetConnectionString("DefaultConnection"));
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Url>().HasIndex(u => u.ShortenCode).IsUnique();
        
        base.OnModelCreating(modelBuilder);
    }
}