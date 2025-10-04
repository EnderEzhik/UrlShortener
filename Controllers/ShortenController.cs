using Microsoft.AspNetCore.Mvc;
using Shortener.Models;
using Shortener.Services;

namespace Shortener.Controllers;

[ApiController]
[Route("[controller]")]
public class ShortenController : ControllerBase
{
    private readonly UrlService _urlService;
    
    public ShortenController(UrlService urlService)
    {
        _urlService = urlService;
    }
    
    // Создать сокращенный код для ссылки
    [HttpPost]
    public async Task<IActionResult> ShortenUrl(ShorteningUrl requestData)
    {
        var shortenCode = await _urlService.CreateShortenCode(requestData.Url);
        return Ok(new { shortenCode });
    }
}