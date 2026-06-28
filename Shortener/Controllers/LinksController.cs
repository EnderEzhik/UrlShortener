using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using Shortener.DTOs;
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
        using (LogContext.PushProperty("Url", requestData.Url))
        {
            _logger.Information("Creating short url");

            if (!requestData.Url.StartsWith("https://") && !requestData.Url.StartsWith("http://"))
            {
                _logger.Warning("Invalid url");
                return Problem(
                    title: "Invalid url",
                    detail: "Url should start with 'https://' or 'http://'",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "errors/invalid-url"
                );
            }

            if (requestData.Url.Length < 4 || requestData.Url.Length > 1000)
            {
                _logger.Warning("Incorrect url length");
                return Problem(
                    title: "Invalid url length",
                    detail: "Url length can not be less than 4 or greater than 1000",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "errors/invalid-url"
                );
            }
            
            if (requestData.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                _logger.Warning("Invalid expiration date");
                return Problem(
                    title: "Invalid expiration date",
                    detail: "Expires should be in the future",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "errors/invalid-expiration"
                );
            }
            
            var rawUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
            int? userId = rawUserId is not null ? int.Parse(rawUserId) : null;
        
            var shortUrl = await _linksService.CreateShortUrlAsync(userId, requestData.Url, requestData.ExpiresAt);
            
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

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<ShortUrlResponse>>> GetShortUrlsWithFilters([FromQuery] UrlsFiltersRequest filters)
    {
        _logger.Information("Getting short urls list with filters {@Filters}", filters);

        if (filters.Page <= 0)
        {
            _logger.Information("Page number is less than or equal to 0");
            return Problem(
                title: "Invalid page number",
                detail: "Page number should be greater than 0",
                statusCode: StatusCodes.Status400BadRequest,
                type: "errors/invalid-page-number");
        }
        
        if (filters.PageSize <= 0)
        {
            _logger.Information("Page size is less than or equal to 0");
            return Problem(
                title: "Invalid page size",
                detail: "Page size should be greater than 0",
                statusCode: StatusCodes.Status400BadRequest,
                type: "errors/invalid-page-size");
        }
        
        if (filters.PageSize > 100)
        {
            _logger.Information("Page size is greater than 100");
            return Problem(
                title: "Invalid page size",
                detail: "Page size should be less than or equal to 100",
                statusCode: StatusCodes.Status400BadRequest,
                type: "errors/invalid-page-size");
        }
        
        var rawUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
        int userId = int.Parse(rawUserId);
        
        var shortUrlsList = await _linksService.GetShortUrlsWithFiltersAsync(
            userId,
            filters.ExcludeExpiredUrls,
            filters.Page,
            filters.PageSize);
        
        _logger.Information("Found {ShortUrlsCount} short urls", shortUrlsList.Count);
        
        return shortUrlsList.Select(u => new ShortUrlResponse()
        {
            OriginalUrl = u.OriginalUrl,
            ShortCode = u.ShortCode,
            CreatedAt = u.CreatedAt,
            ExpiresAt = u.ExpiresAt
        }).ToList();
    }

    [Authorize]
    [HttpDelete("{shortCode}")]
    public async Task<ActionResult> DeleteShortUrlByShortCode(string shortCode)
    {
        using (LogContext.PushProperty("ShortCode", shortCode))
        {
            _logger.Information("Deleting short url");
            
            var rawUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var userId = int.Parse(rawUserId);
            
            bool result = await _linksService.DeleteShortUrlByShortCodeAsync(shortCode, userId);
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