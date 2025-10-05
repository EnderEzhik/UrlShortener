using Microsoft.AspNetCore.Mvc;
using Shortener.Services;

namespace Shortener.Controllers;

[ApiController]
public class RedirectorController : ControllerBase
{
    private readonly UrlService _urlService;

    public RedirectorController(UrlService urlService)
    {
        _urlService = urlService;
    }
    
    // Переадресация с сокращенного кода на оригинальную ссылку
    [HttpGet("{shortenCode}")]
    public async Task<IActionResult> RedirectFromShortCode(string shortenCode)
    {
        var url = await _urlService.GetOriginalUrlByShortCode(shortenCode);
        return url is null ? NotFound() : RedirectPermanent(url);
    }
}