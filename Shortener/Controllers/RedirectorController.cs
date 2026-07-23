using Microsoft.AspNetCore.Mvc;
using Shortener.DTOs;
using Shortener.Errors;
using Shortener.Services;
using Shortener.Services.Analytics;

namespace Shortener.Controllers;

[ApiController]
[Route("/r")]
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
    public async Task<IActionResult> RedirectFromShortCodeAsync(string shortCode)
    {
        using var _ = Serilog.Context.LogContext.PushProperty("ShortCode", shortCode);
        _logger.Information("Redirecting");

        var url = await _urlService.GetCachedShortUrlAsync(shortCode);

        var clickAnalytic = new RedirectAnalytics()
        {
            ShortCode = shortCode,
            RedirectedAt = DateTimeOffset.UtcNow
        };

        _analyticsBufferService.WriteAsync(clickAnalytic);

        if (url is null || url.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _logger.Warning("Short url not found or expired");
            return this.Problem(ApiErrors.ShortCode.IncorrectOrExpiredShortCode);
        }

        _logger.Information("Successfully redirected");
        return Redirect(url.OriginalUrl);
    }
}