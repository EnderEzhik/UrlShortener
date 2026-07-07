using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using Serilog.Context;
using Shortener.Common.Utils;
using Shortener.Data;
using Shortener.DTOs;
using Shortener.Entities;
using Shortener.Extensions;

namespace Shortener.Services;

public class LinksService
{
    private readonly Serilog.ILogger _logger;
    private readonly ApplicationDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly int SHORT_CODE_LENGTH;

    public LinksService(ApplicationDbContext db, IDistributedCache cache, IConfiguration config)
    {
        _db = db;
        _cache = cache;
        _logger = Serilog.Log.ForContext<LinksService>();
        SHORT_CODE_LENGTH = config.GetValue<int>("SHORT_CODE_LENGTH");
    }

    public async Task<ShortUrl> CreateShortUrlAsync(int? userId, string url, DateTimeOffset? expiresAt)
    {
        string shortCode = ShortCodeGenerator.GenerateCode(SHORT_CODE_LENGTH);

        using var _ = LogContext.PushProperty("ShortCode", shortCode);
        _logger.Debug("Short code generated");

        var shortUrl = new ShortUrl()
        {
            OriginalUrl = url,
            ShortCode = shortCode,
            ExpiresAt = expiresAt,
            OwnerId = userId
        };

        _db.Urls.Add(shortUrl);
        await _db.SaveChangesAsync();

        _logger.Information("Short url saved to database");

        return shortUrl;
    }

    public async Task<ShortUrl?> GetUrlByShortCodeAsync(string shortCode)
    {
        return await _db.Urls.SingleOrDefaultAsync(url => url.ShortCode == shortCode);
    }

    public async Task<ShortUrl?> GetCachedShortUrlByShortCodeAsync(string shortCode)
    {
        ShortUrl? shortUrl;

        shortUrl = await _cache.GetRecordAsync<ShortUrl?>(shortCode);
        if (shortUrl is not null)
        {
            _logger.Debug("Short url found in cache");
            return shortUrl;
        }

        shortUrl = await GetUrlByShortCodeAsync(shortCode);
        if (shortUrl is not null)
        {
            _logger.Debug("Short url found in database");
            await _cache.SetRecordAsync<ShortUrl>(shortCode, shortUrl);
            _logger.Debug("Short url successfully saved in cache");
        }

        return shortUrl;
    }

    public async Task<List<ShortUrl>> GetShortUrlsWithFiltersAsync(
        int? userId,
        bool excludeExpiredUrls,
        int page,
        int pageSize)
    {
        var query = _db.Urls.AsQueryable();
        if (userId.HasValue)
        {
            query = query.Where(url => url.OwnerId == userId.Value);
        }
        if (excludeExpiredUrls)
        {
            query = query.Where(url => !url.ExpiresAt.HasValue || url.ExpiresAt > DateTimeOffset.UtcNow);
        }

        List<ShortUrl> shortUrls = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return shortUrls;
    }

    public async Task<ShortUrl?> UpdateUrlAsync(string shortCode, int userId, UpdateUrlRequest requestData)
    {
        var url = await _db.Urls.SingleOrDefaultAsync(url => url.ShortCode == shortCode);
        if (url is null)
        {
            _logger.Warning("Short url not found");
            return null;
        }

        if (url.OwnerId != userId)
        {
            _logger.Warning("User is trying to update a short url that is not his own");
            return null;
        }

        if (requestData.OriginalUrl is not null && requestData.OriginalUrl != url.OriginalUrl)
        {
            url.OriginalUrl = requestData.OriginalUrl;
        }
        if (requestData.ExpiresAt is not null && requestData.ExpiresAt != url.ExpiresAt)
        {
            url.ExpiresAt = requestData.ExpiresAt;
        }

        await _db.SaveChangesAsync();
        return url;
    }

    public async Task<bool> DeleteShortUrlByShortCodeAsync(string shortCode, int userId)
    {
        ShortUrl? shortUrl = await GetUrlByShortCodeAsync(shortCode);
        if (shortUrl is null)
        {
            _logger.Warning("Short url not found");
            return false;
        }

        if (shortUrl.OwnerId != userId)
        {
            _logger.Warning("User is trying to delete a short url that is not his own");
            return false;
        }

        await _cache.RemoveAsync(shortCode);

        _logger.Debug("Short url deleted from cache");

        _db.Urls.Remove(shortUrl);
        await _db.SaveChangesAsync();

        _logger.Debug("Short url deleted from database");

        return true;
    }
}