using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<ActionResult<ShortUrlResponse>> CreateShortUrl(CreateShortUrlRequest requestData)
    {
        logger.Information("Post request for create new ShortUrl");
        
        if (requestData.ExpiresAt <= DateTime.UtcNow)
        {
            logger.Information("Expires must be in the future");
            return BadRequest(new
            {
                message = "Expires must be in the future"
            });
        }
        
        logger.Information("Checking whether the user is authorized");
        int? parsedUserId = null;
        if (HttpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
            parsedUserId = userId is not null ? int.Parse(userId) : null;
        }
        logger.Information("User is authorized: {isAuthorized}", parsedUserId is not null);
        
        try
        {
            var shortUrl = await _linksService.CreateShortUrlAsync(parsedUserId, requestData.Url, requestData.ExpiresAt);
            return new ShortUrlResponse()
            {
                OriginalUrl = shortUrl.OriginalUrl,
                ShortCode = shortUrl.ShortCode,
                ExpiresAt = shortUrl.ExpiresAt,
                CreatedAt = shortUrl.CreatedAt
            };
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{shortCode}")]
    public async Task<ActionResult<ShortUrlResponse>> GetShortUrlByShortCode(string shortCode)
    {
        logger.Information("Get request for get ShortUrl");
        try
        {
            ShortUrl? shortUrl = await _linksService.GetCachedShortUrlByShortCodeAsync(shortCode);
            if (shortUrl is null)
            {
                return NotFound();
            }

            return new ShortUrlResponse()
            {
                OriginalUrl = shortUrl.OriginalUrl,
                ShortCode = shortUrl.ShortCode,
                ExpiresAt = shortUrl.ExpiresAt,
                CreatedAt = shortUrl.CreatedAt
            };
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<ShortUrlResponse>>> GetShortUrlsWithFilters([FromQuery] UrlsFiltersRequest filters)
    {
        logger.Information("Get request for get ShortUrl list with filters");
        
        var currentUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var parsedUserId = int.Parse(currentUserId);
        
        try
        {
            var shortUrlList =
                await _linksService.GetShortUrlsWithFiltersAsync(parsedUserId, filters.ExcludeExpiredUrls);
            return shortUrlList.Select(u => new ShortUrlResponse()
            {
                OriginalUrl = u.OriginalUrl,
                ShortCode = u.ShortCode,
                CreatedAt = u.CreatedAt,
                ExpiresAt = u.ExpiresAt
            }).ToList();
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [Authorize]
    [HttpDelete("{shortCode}")]
    public async Task<ActionResult> DeleteShortUrlByShortCode(string shortCode)
    {
        logger.Information("Delete request for delete ShortUrl");
        
        var currentUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var parsedUserId = int.Parse(currentUserId);
        
        try
        {
            bool result = await _linksService.DeleteShortUrlByShortCodeAsync(shortCode, parsedUserId);
            return result ? NoContent() : NotFound();
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}