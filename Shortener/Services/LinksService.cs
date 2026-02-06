using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Shortener.Common.Utils;
using Shortener.Data;
using Shortener.Entities;
using Shortener.Extensions;
using Shortener.Models;
using StackExchange.Redis;

namespace Shortener.Services;

public class LinksService
{
    private readonly Serilog.ILogger logger = Log.ForContext<LinksService>();
    private readonly ApplicationDbContext _db;
    private readonly IDistributedCache _cache;
    private const int SHORT_CODE_LENGTH = 8;

    public LinksService(ApplicationDbContext db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<ShortUrl> CreateShortUrlAsync(CreateShortUrlRequest request)
    {
        logger.Information("Generating short code");

        if (request.ExpiresAt <= DateTime.UtcNow)
        {
            logger.Information("Expires must be in the future");
            throw new ArgumentException("Expires must be in the future");
        }
        
        string shortCode = ShortCodeGenerator.GenerateCode(SHORT_CODE_LENGTH);
        logger.Information("Short code generated: {shortCode}", shortCode);

        ShortUrl shortUrl = new ShortUrl()
        {
            OriginalUrl = request.Url,
            ShortCode = shortCode,
            ExpiresAt = request.ExpiresAt
        };
        
        logger.Information("Saving short url to database. Short code: {shortCode}", shortUrl);

        try
        {
            _db.Urls.Add(shortUrl);
            await _db.SaveChangesAsync();
            logger.Information("Short url saved successfully. Short code: {shortCode}", shortUrl);
        }
        catch (Exception e)
        {
            logger.Error(e, "Database error while saving short url. Url: {url}, Short code: {shortCode}", request.Url, shortUrl);
            throw;
        }
        
        return shortUrl;
    }

    public async Task<ShortUrl?> GetShortUrlByShortCodeAsync(string shortCode)
    {
        ShortUrl? shortUrl = await _db.Urls.FirstOrDefaultAsync(url => url.ShortCode == shortCode
                                                                       && (!url.ExpiresAt.HasValue || url.ExpiresAt > DateTime.UtcNow));
        return shortUrl;
    }

    public async Task<ShortUrl?> GetCachedShortUrlByShortCodeAsync(string shortCode)
    {
        ShortUrl? shortUrl;
        
        try
        {
            shortUrl = await _cache.GetRecordAsync<ShortUrl?>(shortCode);
            if (shortUrl is not null)
            {
                logger.Information("ShortCode found in cache");
                return shortUrl;
            }
            logger.Information("ShortCode not found in cache");
        }
        catch (RedisConnectionException e)
        {
            logger.Error("Redis connection error");
            throw;
        }
        
        logger.Information("Get shortCode from db");
        shortUrl = await GetShortUrlByShortCodeAsync(shortCode);
        if (shortUrl is not null)
        {
            await _cache.SetRecordAsync<ShortUrl>(shortCode, shortUrl);
        }

        if (shortUrl is not null)
        {
            logger.Information("Short code found in db");
        }
        else
        {
            logger.Information("Short code found in db");
        }

        return shortUrl;
    }

    public async Task<List<ShortUrl>> GetAllShortUrlsAsync(string? containsSubstring, bool excludeExpiredUrls = true)
    {
        var query = _db.Urls.AsQueryable();
        if (containsSubstring is not null)
        {
            query = query.Where(url => url.OriginalUrl.Contains(containsSubstring));
        }
        if (excludeExpiredUrls)
        {
            query = query.Where(url => !url.ExpiresAt.HasValue || url.ExpiresAt > DateTime.Now);
        }
        
        List<ShortUrl> shortUrls = await query.ToListAsync();
        return shortUrls;
    }

    public async Task<bool> DeleteShortUrlByShortCodeAsync(string shortCode)
    {
        ShortUrl? shortUrl = await GetShortUrlByShortCodeAsync(shortCode);
        if (shortUrl is null)
        {
            return false;
        }
        
        await _cache.RemoveAsync(shortCode);
        
        _db.Urls.Remove(shortUrl);
        await _db.SaveChangesAsync();
        return true;
    }
}