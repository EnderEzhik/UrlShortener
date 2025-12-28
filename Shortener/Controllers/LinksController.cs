using Microsoft.AspNetCore.Mvc;
using Shortener.Entities;
using Shortener.Models;
using Shortener.Models.DTOs;
using Shortener.Services;

namespace Shortener.Controllers;

[ApiController]
[Route("api/links")]
public class LinksController : ControllerBase
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<LinksController>();
    private readonly LinksService _linksService;
    
    public LinksController(LinksService linksService)
    {
        _linksService = linksService;
    }

    [HttpPost]
    public async Task<ActionResult<ShortCodeResponse>> CreateShortUrl(CreateShortUrlRequest data)
    {
        logger.Information("Create short url request. Url: {url}", data.Url);
        try
        {
            var shortUrl = await _linksService.CreateShortUrlAsync(data);
            logger.Information("Short url successfully created. Short code: {shortCode}", shortUrl);
            return Ok(shortUrl);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error while  creating new short url. Url: {url}", data.Url);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{shortCode}")]
    public async Task<ActionResult<ShortCodeResponse>> GetShortUrlByShortCode(string shortCode)
    {
        ShortUrl? shortUrl = await _linksService.GetCachedShortUrlByShortCodeAsync(shortCode);
        return shortUrl is not null ? Ok(shortUrl) : NotFound();
    }

    [HttpGet]
    public async Task<ActionResult<List<ShortCodeResponse>>> GetAllShortUrls()
    {
        var shortUrlList = await _linksService.GetAllShortUrlsAsync();
        return Ok(shortUrlList);
    }

    [HttpDelete("{shortCode}")]
    public async Task<ActionResult> DeleteShortUrlByShortCode(string shortCode)
    {
        bool result = await _linksService.DeleteShortUrlByShortCodeAsync(shortCode);
        return result ? NoContent() : NotFound();
    }
}