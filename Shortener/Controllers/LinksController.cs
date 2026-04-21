using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using Shortener.Entities;
using Shortener.Models;
using Shortener.Models.DTOs;
using Shortener.Services;

namespace Shortener.Controllers;

[ApiController]
[Route("api/links")]
public class LinksController : ControllerBase
{
    private readonly Serilog.ILogger _logger;
    private readonly LinksService _linksService;
    
    public LinksController(LinksService linksService)
    {
        _linksService = linksService;
        _logger = Serilog.Log.ForContext<LinksController>();
    }

    [HttpPost]
    public async Task<ActionResult<ShortUrlResponse>> CreateShortUrl(CreateShortUrlRequest requestData)
    {
        int? parsedUserId = null;
        
        if (HttpContext.User.Identity?.IsAuthenticated is true)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
            parsedUserId = userId is not null ? int.Parse(userId) : null;
        }
        
        using(LogContext.PushProperty("UserId", parsedUserId))
        using (LogContext.PushProperty("Url", requestData.Url))
        {
            _logger.Information("Creating short url");
            
            if (requestData.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                _logger.Warning("Invalid expiration date");
                
                return BadRequest(new
                {
                    message = "Expires must be in the future"
                });
            }
        
            try
            {
                var shortUrl = await _linksService.CreateShortUrlAsync(parsedUserId, requestData.Url, requestData.ExpiresAt);
                
                _logger.Information("Short url successfully created");
                
                return new ShortUrlResponse()
                {
                    OriginalUrl = shortUrl.OriginalUrl,
                    ShortCode = shortUrl.ShortCode,
                    ExpiresAt = shortUrl.ExpiresAt,
                    CreatedAt = shortUrl.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while creating short url");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }

    [HttpGet("{shortCode}")]
    public async Task<ActionResult<ShortUrlResponse>> GetShortUrlByShortCode(string shortCode)
    {
        using (LogContext.PushProperty("ShortCode", shortCode))
        {
            _logger.Information("Getting short url");
            
            try
            {
                ShortUrl? shortUrl = await _linksService.GetCachedShortUrlByShortCodeAsync(shortCode);
                if (shortUrl is null)
                {
                    _logger.Warning("Short url not found");
                    
                    return NotFound();
                }
                
                _logger.Information("Short url successfully got");

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
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<ShortUrlResponse>>> GetShortUrlsWithFilters([FromQuery] UrlsFiltersRequest filters)
    {
        var currentUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var parsedUserId = int.Parse(currentUserId);

        using (LogContext.PushProperty("UserId", parsedUserId))
        {
            _logger.Information("Getting short urls list with filters {@Filters}", filters);
            
            try
            {
                var shortUrlsList = await _linksService.GetShortUrlsWithFiltersAsync(parsedUserId, filters.ExcludeExpiredUrls);
                
                _logger.Information("Found {ShortUrlsCount} short urls", shortUrlsList.Count);
                
                return shortUrlsList.Select(u => new ShortUrlResponse()
                {
                    OriginalUrl = u.OriginalUrl,
                    ShortCode = u.ShortCode,
                    CreatedAt = u.CreatedAt,
                    ExpiresAt = u.ExpiresAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while getting short urls list");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }

    [Authorize]
    [HttpDelete("{shortCode}")]
    public async Task<ActionResult> DeleteShortUrlByShortCode(string shortCode)
    {
        var currentUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var parsedUserId = int.Parse(currentUserId);

        using (LogContext.PushProperty("UserId", parsedUserId))
        using (LogContext.PushProperty("ShortCode", shortCode))
        {
            _logger.Information("Deleting short url");
            
            try
            {
                bool result = await _linksService.DeleteShortUrlByShortCodeAsync(shortCode, parsedUserId);
                if (!result)
                {
                    return NotFound();
                }
                
                _logger.Information("Short url successfully deleted");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while deleting short url");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}