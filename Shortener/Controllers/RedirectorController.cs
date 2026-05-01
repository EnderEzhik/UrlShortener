using Microsoft.AspNetCore.Mvc;
using Shortener.Services;

namespace Shortener.Controllers;

[ApiController]
public class RedirectorController : ControllerBase
{
    private readonly Serilog.ILogger _logger;
    private readonly LinksService _urlService;

    public RedirectorController(LinksService urlService)
    {
        _urlService = urlService;
        _logger = Serilog.Log.ForContext<RedirectorController>();
    }
    
    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectFromShortCode(string shortCode)
    {
        using (Serilog.Context.LogContext.PushProperty("ShortCode", shortCode))
        {
            _logger.Information("Redirecting");
            
            var url = await _urlService.GetCachedShortUrlByShortCodeAsync(shortCode);
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

            _logger.Information("Short url found");

            return RedirectPermanent(url.OriginalUrl);
        }
    }
}