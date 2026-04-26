using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Shortener.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception");

            var problem = new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred",
                Type = "errors/internal-error"//TODO: позже заменить на документированную ошибку
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}