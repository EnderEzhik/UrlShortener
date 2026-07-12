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
                Title = ApiErrors.Server.InternalServerError.Title,
                Status = ApiErrors.Server.InternalServerError.StatusCode,
                Detail = ApiErrors.Server.InternalServerError.Detail,
                Type = ApiErrors.Server.InternalServerError.Type
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}