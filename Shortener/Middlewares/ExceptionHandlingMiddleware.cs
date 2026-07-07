using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shortener.Errors;

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
                Title = ApiErrors.InternalServerError.Title,
                Status = ApiErrors.InternalServerError.StatusCode,
                Detail = ApiErrors.InternalServerError.Detail,
                Type = ApiErrors.InternalServerError.Type
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}