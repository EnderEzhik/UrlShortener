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
            
            try
            {
                var url = await _urlService.GetCachedShortUrlByShortCodeAsync(shortCode);
                if (url is null || url.ExpiresAt <= DateTimeOffset.UtcNow)
                {
                    _logger.Warning("Short url not found or expired");

                    return NotFound(new
                    {
                        message = "Short code not found or expired"
                    });
                }

                _logger.Information("Short url found");

                return RedirectPermanent(url.OriginalUrl);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while redirecting to {shortCode}", shortCode);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}