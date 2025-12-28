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
        var shortUrl = await _linksService.CreateShortUrlAsync(data);
        return Ok(shortUrl);
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