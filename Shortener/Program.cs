using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Scalar.AspNetCore;
using Serilog;
using Shortener.Data;
using Shortener.Middlewares;
using Shortener.Options;
using Shortener.Services;
using Shortener.Services.Analytics;
using StackExchange.Redis;

namespace Shortener;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

            ConfigureServices(builder);

            var app = builder.Build();

            CheckDatabaseConnection(builder.Configuration);
            CheckRedisConnection(builder.Configuration);

            app.UseSerilogRequestLogging();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
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

    private static void CheckDatabaseConnection(IConfiguration configuration)
    {
        const int maxAttempts = 3;
        const int delaySeconds = 5;

        var connectionString = configuration.GetConnectionString("DATABASE");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string 'DATABASE' is not configured.");
        }

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                Log.Information("Successfully connected to the database on attempt {Attempt}.", attempt);
                return; // Success, exit the method
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Database connection attempt {Attempt} failed. Retrying in {DelaySeconds} seconds...", attempt, delaySeconds);

                if (attempt < maxAttempts)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
                }
                else
                {
                    Log.Error(ex, "Failed to connect to the database after {MaxAttempts} attempts.", maxAttempts);
                    throw; // Re-throw the exception to terminate the application
                }
            }
        }
    }

    private static void CheckRedisConnection(IConfiguration configuration)
    {
        const int maxAttempts = 3;
        const int delaySeconds = 5;

        var connectionString = configuration.GetConnectionString("REDIS");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Redis connection string 'REDIS' is not configured.");
        }

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var connection = ConnectionMultiplexer.Connect(connectionString);
                connection.Close();

                Log.Information("Successfully connected to Redis on attempt {Attempt}.", attempt);
                return; // Success, exit the method
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Redis connection attempt {Attempt} failed. Retrying in {DelaySeconds} seconds...", attempt, delaySeconds);

                if (attempt < maxAttempts)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
                }
                else
                {
                    Log.Error(ex, "Failed to connect to Redis after {MaxAttempts} attempts.", maxAttempts);
                    throw; // Re-throw the exception to terminate the application
                }
            }
        }
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();

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

        builder.Services.AddSingleton<AnalyticsBufferService>();
        builder.Services.AddHostedService<AnalyticsProcessorService>();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                // options.JsonSerializerOptions.PropertyNamingPolicy =  null;
            });
    }
}
