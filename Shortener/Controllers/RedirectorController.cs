using Microsoft.AspNetCore.Mvc;
using Shortener.Services;

namespace Shortener.Controllers;

[ApiController]
public class RedirectorController : ControllerBase
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<RedirectorController>();
    private readonly LinksService _urlService;

    public RedirectorController(LinksService urlService)
    {
        _urlService = urlService;
    }
    
    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectFromShortCode(string shortCode)
    {
        logger.Information("Get Request for redirect from short code");
        
        try
        {
            var url = await _urlService.GetCachedShortUrlByShortCodeAsync(shortCode);
            if (url is null || url.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                return NotFound(new
                {
                    message = "Short code not found or expired"
                });
            }

            return RedirectPermanent(url.OriginalUrl);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}