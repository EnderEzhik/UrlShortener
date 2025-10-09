using Microsoft.AspNetCore.Mvc;
using Shortener.Models;
using Shortener.Models.DTOs;
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
    
    // Создать сокращенную ссылку
    [HttpPost]
    public async Task<ActionResult<ShortCodeResponse>> ShortenUrl(CreateShortUrlRequest requestData)
    {
        if (string.IsNullOrEmpty(requestData.Url))
        {
            return BadRequest("Url can not be empty");
        }

        if (requestData.ExpiresAt.HasValue && requestData.ExpiresAt.Value < DateTime.Now)
        {
            return BadRequest("Expires date must be in the future");
        }
            
        var shortCode = await _urlService.CreateShortUrl(requestData.Url, requestData.ExpiresAt);
        var shortCodeResponse = new ShortCodeResponse()
        {
            ShortCode = shortCode,
            ExpiresAt = requestData.ExpiresAt
        };
        
        return Ok(shortCodeResponse);
    }
}