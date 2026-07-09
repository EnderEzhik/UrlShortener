using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using Shortener.DTOs;
using Shortener.Errors;
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
    public async Task<ActionResult<ShortUrlResponse>> CreateShortUrlAsync(CreateShortUrlRequest requestData)
    {
        using var _ = LogContext.PushProperty("Url", requestData.Url);
        _logger.Information("Creating short url");

        if (!requestData.Url.StartsWith("https://") && !requestData.Url.StartsWith("http://"))
        {
            _logger.Warning("Incorrect url");
            return this.Problem(ApiErrors.IncorrectUrl);
        }

        if (requestData.Url.Length < 4 || requestData.Url.Length > 1000)
        {
            _logger.Warning("Incorrect url length");
            return this.Problem(ApiErrors.IncorrectUrlLength);
        }

        if (requestData.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _logger.Warning("Incorrect expiration date");
            return this.Problem(ApiErrors.IncorrectExpirationDate);
        }

        var rawUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
        int? userId = !string.IsNullOrEmpty(rawUserId) ? int.Parse(rawUserId) : null;

        var shortUrl = await _linksService.CreateShortUrlAsync(userId, requestData);

        _logger.Information("Short url successfully created");

        return new ShortUrlResponse()
        {
            OriginalUrl = shortUrl.OriginalUrl,
            ShortCode = shortUrl.ShortCode,
            ExpiresAt = shortUrl.ExpiresAt,
            CreatedAt = shortUrl.CreatedAt
        };
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<ShortUrlResponse>>> GetShortUrlsListAsync([FromQuery] UrlsFiltersRequest filters)
    {
        using var _ = LogContext.PushProperty("Filters", filters);
        _logger.Information("Getting short urls list");

        if (filters.Page <= 0)
        {
            _logger.Information("Page number is less than or equal to 0");
            return this.Problem(ApiErrors.IncorrectPageNumber);
        }

        if (filters.PageSize <= 0 || filters.PageSize > 100)
        {
            _logger.Information("Page size is less than 1 or greater than 100");
            return this.Problem(ApiErrors.IncorrectPageSize);
        }

        var rawUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)!.Value;
        int userId = int.Parse(rawUserId);

        var shortUrlsList = await _linksService.GetShortUrlsListAsync(userId, filters);

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
    [HttpPut("{shortCode}")]
    public async Task<ActionResult<ShortUrlResponse>> UpdateShortUrlAsync(string shortCode, [FromBody] UpdateUrlRequest requestData)
    {
        using var _ = LogContext.PushProperty("ShortCode", shortCode);
        _logger.Information("Updating short url");

        var rawUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)!.Value;
        int userId = int.Parse(rawUserId);

        var updatedUrl = await _linksService.UpdateShortUrlAsync(shortCode, userId, requestData);
        if (updatedUrl is null)
        {
            return this.Problem(ApiErrors.IncorrectShortCode);
        }

        _logger.Information("Short url successfully updated");

        return new ShortUrlResponse()
        {
            ShortCode = updatedUrl.ShortCode,
            OriginalUrl = updatedUrl.OriginalUrl,
            CreatedAt = updatedUrl.CreatedAt,
            ExpiresAt = updatedUrl.ExpiresAt
        };
    }

    [Authorize]
    [HttpDelete("{shortCode}")]
    public async Task<ActionResult> DeleteShortUrlAsync(string shortCode)
    {
        using var _ = LogContext.PushProperty("ShortCode", shortCode);
        _logger.Information("Deleting short url");

        var rawUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)!.Value;
        var userId = int.Parse(rawUserId);

        bool result = await _linksService.DeleteShortUrlAsync(shortCode, userId);
        if (!result)
        {
            return this.Problem(ApiErrors.IncorrectShortCode);
        }

        _logger.Information("Short url successfully deleted");
        return NoContent();
    }
}