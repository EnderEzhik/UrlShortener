using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Filters;
using Shortener.Data;
using Shortener.Services;

namespace Shortener;

public class Program
{
    public static void Main(string[] args)
    {
        ConfigureLogging();
        
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSerilog();

        ConfigureServices(builder);

        var app = builder.Build();

        app.MapControllers();

        app.Run();
    }

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: "logs/all/shortener-all.log",
                rollingInterval: RollingInterval.Day,
                shared: true)
            .WriteTo.Logger(lc => lc
                .Enrich.FromLogContext()
                .Filter.ByExcluding(Matching.FromSource("System"))
                .Filter.ByExcluding(Matching.FromSource("Microsoft"))
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/shortener.log",
                    rollingInterval: RollingInterval.Day,
                    shared: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            )
            .CreateLogger();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        
        builder.Services.AddScoped<UrlService>();
        
        builder.Services.AddControllers();
    }
}