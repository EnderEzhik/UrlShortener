using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using Shortener.Data;
using Shortener.Middlewares;
using Shortener.Options;
using Shortener.Services;

namespace Shortener;

public class Program
{
    public static void Main(string[] args)
    {
        ConfigureLogging();

        try
        {
            Log.Information("Application starting...");
            
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder);

            var app = builder.Build();

            app.UseSerilogRequestLogging();
            app.UseMiddleware<RequestLoggingMiddleware>();
            
            app.UseCors(options => options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }
            
            Log.Information("Application started");
            app.Run();
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Application terminated unexpectedly. Error: {Message}", e.Message);
        }
        finally
        {
            Log.Information("Application stopped");
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.File(
                formatter: new CompactJsonFormatter(),
                path: "logs/all/shortener-all.log",
                rollingInterval: RollingInterval.Day,
                shared: true)
            .WriteTo.Logger(lc => lc
                .Enrich.FromLogContext()
                .Filter.ByExcluding(Matching.FromSource("System"))
                .Filter.ByExcluding(Matching.FromSource("Microsoft"))
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    formatter: new CompactJsonFormatter(),
                    path: "logs/shortener.log",
                    rollingInterval: RollingInterval.Day,
                    shared: true)
            )
            .CreateLogger();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        Log.Debug("Configuring services...");
        builder.Services.AddOpenApi();
        builder.Services.AddSerilog();
        
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DATABASE"));
        });

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            var redisConnectionString = builder.Configuration.GetConnectionString("REDIS");
            options.Configuration = redisConnectionString;
            options.InstanceName = "UrlShortener_";
        });

        builder.Services.ConfigureOptions<JwtOptionsSetup>();
        
        builder.Services.AddScoped<LinksService>();
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<JwtService>();
        
        var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
                         ?? throw new InvalidOperationException("Jwt options are not configured. Missing 'Jwt' section in configuration.");

        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };
            });
        
        builder.Services.AddControllers();
        
        Log.Debug("Services configured");
    }
}