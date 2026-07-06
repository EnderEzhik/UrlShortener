using Microsoft.AspNetCore.Mvc;
using Shortener.DTOs;
using Shortener.Services;
using Shortener.Services.Analytics;

namespace Shortener.Controllers;

[ApiController]
public class RedirectorController : ControllerBase
{
    private readonly Serilog.ILogger _logger;
    private readonly LinksService _urlService;
    private readonly AnalyticsBufferService _analyticsBufferService;

    public RedirectorController(LinksService urlService, AnalyticsBufferService analyticsBufferService)
    {
        _logger = Serilog.Log.ForContext<RedirectorController>();
        _urlService = urlService;
        _analyticsBufferService = analyticsBufferService;
    }

    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectFromShortCode(string shortCode)
    {
        using (Serilog.Context.LogContext.PushProperty("ShortCode", shortCode))
        {
            _logger.Information("Redirecting");

            var url = await _urlService.GetCachedShortUrlByShortCodeAsync(shortCode);

            var clickAnalytic = new RedirectAnalytics()
            {
                ShortCode = shortCode,
                RedirectedAt = DateTimeOffset.UtcNow
            };

            if (url is null || url.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                _logger.Warning("Short url not found or expired");

                return Problem(
                    title: "Invalid short code",
                    detail: $"Short url by short code \"{shortCode}\" not found or expired",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "errors/invalid-short-code"
                );
            }

            _analyticsBufferService.WriteAsync(clickAnalytic);

            _logger.Information("Short url found");

            return Redirect(url.OriginalUrl);
        }
    }
}