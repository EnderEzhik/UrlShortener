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
                
                return Problem(
                    title: "Invalid expiration date",
                    detail: "Expires must be in the future",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "errors/invalid-expiration"
                );
            }
        
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
    }

    [HttpGet("{shortCode}")]
    public async Task<ActionResult<ShortUrlResponse>> GetShortUrlByShortCode(string shortCode)
    {
        using (LogContext.PushProperty("ShortCode", shortCode))
        {
            _logger.Information("Getting short url");
            
            ShortUrl? shortUrl = await _linksService.GetCachedShortUrlByShortCodeAsync(shortCode);
            if (shortUrl is null)
            {
                _logger.Warning("Short url not found");
                
                return Problem(
                    title: "Invalid short code",
                    detail: $"Short url by short code \"{shortCode}\" not found",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "errors/invalid-short-code"
                );
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
            
            bool result = await _linksService.DeleteShortUrlByShortCodeAsync(shortCode, parsedUserId);
            if (!result)
            {
                return Problem(
                    title: "Invalid short code",
                    detail: $"Short url by short code \"{shortCode}\" not found",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "errors/invalid-short-code"
                );
            }
            
            _logger.Information("Short url successfully deleted");
            return NoContent();
        }
    }
}