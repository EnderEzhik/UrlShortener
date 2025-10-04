using Shortener.Data;
using Shortener.Services;

namespace Shortener;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddDbContext<ApplicationDbContext>();
        builder.Services.AddScoped<UrlService>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
        
        app.MapControllers();

        app.Run();
    }
}