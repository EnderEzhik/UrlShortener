using System.IdentityModel.Tokens.Jwt;
using Serilog;

namespace Shortener.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var rawUserId = context.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
        int? userId = rawUserId is not null ? int.Parse(rawUserId) : null;
        
        using (Serilog.Context.LogContext.PushProperty("UserId", userId?.ToString() ?? "null"))
        using (Serilog.Context.LogContext.PushProperty("RequestPath", context.Request.Path))
        {
            Log.Information("Incoming HTTP request");

            await _next(context);

            Log.Information("Request finished with status {StatusCode}", context.Response.StatusCode);
        }
    }
}