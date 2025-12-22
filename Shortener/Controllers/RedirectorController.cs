using Microsoft.AspNetCore.Mvc;
using Shortener.Services;

namespace Shortener.Controllers;

[ApiController]
public class RedirectorController : ControllerBase
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<RedirectorController>();
    private readonly UrlService _urlService;

    public RedirectorController(UrlService urlService)
    {
        _urlService = urlService;
    }
    
    // Переадресация с сокращенного кода на оригинальную ссылку
    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectFromShortCode(string shortCode)
    {
        logger.Information("GET Request for redirect from short code");
        try
        {
            var url = await _urlService.GetCachedOriginalUrlByShortCode(shortCode);
            if (url is null)
            {
                logger.Information("No original url for short code");
                return NotFound();
            }

            logger.Information("Redirecting from short code");
            return Redirect(url);
        }
        catch (Exception e)
        {
            logger.Error(e, "Error redirecting. Error: {ErrorMessage}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}