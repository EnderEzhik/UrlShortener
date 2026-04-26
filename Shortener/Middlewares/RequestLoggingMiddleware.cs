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
        using (Serilog.Context.LogContext.PushProperty("RequestPath", context.Request.Path))
        {
            Log.Information("Incoming HTTP request");

            await _next(context);

            Log.Information("Request finished with status {StatusCode}", context.Response.StatusCode);
        }
    }
}