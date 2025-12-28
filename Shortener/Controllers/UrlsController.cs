using Microsoft.AspNetCore.Mvc;
using Shortener.Models;
using Shortener.Models.DTOs;
using Shortener.Services;

namespace Shortener.Controllers;

[ApiController]
[Route("api/urls")]
public class UrlsController : ControllerBase
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<UrlsController>();
    private readonly UrlService _urlService;
    
    public UrlsController(UrlService urlService)
    {
        _urlService = urlService;
    }
    
    // Создать сокращенную ссылку
    [HttpPost]
    public async Task<ActionResult<ShortCodeResponse>> ShortenUrl(CreateShortUrlRequest requestData)
    {
        try
        {
            logger.Information("POST Request for creating short url");
            var shortCode = await _urlService.CreateShortUrl(requestData.Url, requestData.ExpiresAt);
            var shortCodeResponse = new ShortCodeResponse()
            {
                ShortCode = shortCode,
                ExpiresAt = requestData.ExpiresAt
            };
            
            logger.Information("Short url created");
            return Ok(shortCodeResponse);
        }
        catch (Exception e)
        {
            logger.Error(e, "Error creating shortcode. Error: {ErrorMessage}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{shortCode}")]
    public async Task<ActionResult<ShortUrlInfoResponse>> GetShortCode(string shortCode)
    {
        logger.Information("GET request for get short link with short code: {shortCode}", shortCode);
        try
        {
            var shortUrl = await _urlService.GetShortUrlByShortCode(shortCode);
            if (shortUrl is null)
            {
                return NotFound();
            }
            
            var mappedUrl = new ShortUrlInfoResponse()
            {
                OriginalUrl = shortUrl.OriginalUrl,
                ShortCode = shortCode,
                CreatedAt = shortUrl.CreatedAt,
                ExpiresAt = shortUrl.ExpiresAt
            };
            return Ok(mappedUrl);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ShortUrlInfoResponse>>> GetAllShortUrls()
    {
        logger.Information("GET request for get all short urls");

        try
        {
            var urls =  await _urlService.GetAllShortUrls();
            var mappedUrls = urls.Select(shortUrl =>
                new ShortUrlInfoResponse()
                {
                    OriginalUrl = shortUrl.OriginalUrl,
                    ShortCode = shortUrl.ShortCode,
                    CreatedAt = shortUrl.CreatedAt,
                    ExpiresAt = shortUrl.ExpiresAt
                }).ToList();
            return Ok(mappedUrls);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpDelete("{shortCode}")]
    public async Task<IActionResult> DeleteShortCode(string shortCode)
    {
        try
        {
            var result = await _urlService.DeleteShortUrl(shortCode);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}