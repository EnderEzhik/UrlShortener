using Microsoft.AspNetCore.Mvc;
using Shortener.Entities;
using Shortener.Models;
using Shortener.Models.DTOs;
using Shortener.Services;
using StackExchange.Redis;

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
    public async Task<ActionResult<ShortCodeResponse>> CreateShortUrl(CreateShortUrlRequest requestData)
    {
        logger.Information("Create short url request. Url: {url}", requestData.Url);
        try
        {
            var shortUrl = await _linksService.CreateShortUrlAsync(requestData);
            logger.Information("Short url successfully created. Short code: {shortCode}", shortUrl);
            return Ok(shortUrl);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error while creating new short url. Url: {url}", requestData.Url);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{shortCode}")]
    public async Task<ActionResult<ShortCodeResponse>> GetShortUrlByShortCode(string shortCode)
    {
        try
        {
            ShortUrl? shortUrl = await _linksService.GetCachedShortUrlByShortCodeAsync(shortCode);
            return shortUrl is not null ? Ok(shortUrl) : NotFound();
        }
        catch (RedisConnectionException e)
        {
            return StatusCode(503);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ShortCodeResponse>>> GetAllShortUrls([FromQuery] UrlsFiltersRequest filters)
    {
        var shortUrlList = await _linksService.GetAllShortUrlsAsync(filters.containsSubstring, filters.excludeExpiredUrls);
        return Ok(shortUrlList);
    }

    [HttpDelete("{shortCode}")]
    public async Task<ActionResult> DeleteShortUrlByShortCode(string shortCode)
    {
        bool result = await _linksService.DeleteShortUrlByShortCodeAsync(shortCode);
        return result ? NoContent() : NotFound();
    }
}