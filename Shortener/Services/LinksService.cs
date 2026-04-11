using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using Serilog;
using Shortener.Common.Utils;
using Shortener.Data;
using Shortener.Entities;
using Shortener.Extensions;

namespace Shortener.Services;

public class LinksService
{
    private readonly Serilog.ILogger logger = Log.ForContext<LinksService>();
    private readonly ApplicationDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly int SHORT_CODE_LENGTH;

    public LinksService(ApplicationDbContext db, IDistributedCache cache, IConfiguration config)
    {
        _db = db;
        _cache = cache;
        SHORT_CODE_LENGTH = config.GetValue<int>("SHORT_CODE_LENGTH");
    }

    public async Task<ShortUrl> CreateShortUrlAsync(int? userId, string url, DateTimeOffset? expiresAt)
    {
        logger.Information("Creating new ShortUrl");
        
        logger.Information("Generating short code");
        string shortCode = ShortCodeGenerator.GenerateCode(SHORT_CODE_LENGTH);
        logger.Information("Short code generated: {shortCode}", shortCode);

        var shortUrl = new ShortUrl()
        {
            OriginalUrl = url,
            ShortCode = shortCode,
            ExpiresAt = expiresAt,
            UserId = userId
        };
        
        logger.Information("Saving ShortUrl to database");
        _db.Urls.Add(shortUrl);
        
        try
        {
            await _db.SaveChangesAsync();
            logger.Information("successfully saved ShortUrl to database");
            return shortUrl;
        }
        catch (Exception e)
        {
            logger.Error(e, "Error when saving ShortUrl to database");
            throw;
        }
    }

    public async Task<ShortUrl?> GetUrlByShortCodeAsync(string shortCode)
    {
        logger.Information("Searching ShortUrl in database");
        try
        {
            ShortUrl? shortUrl = await _db.Urls.SingleOrDefaultAsync(url => url.ShortCode == shortCode);
            logger.Information("ShortUrl found in database: {shortUrlFound}", shortUrl is not null);
            return shortUrl;
        }
        catch (Exception e)
        {
            logger.Error(e, "Error when searching ShortUrl in database");
            throw;
        }
    }

    public async Task<ShortUrl?> GetCachedShortUrlByShortCodeAsync(string shortCode)
    {
        logger.Information("Searching ShortUrl in cache");
        
        ShortUrl? shortUrl;
        
        try
        {
            shortUrl = await _cache.GetRecordAsync<ShortUrl?>(shortCode);
            if (shortUrl is not null)
            {
                logger.Information("ShortUrl found in cache");
                return shortUrl;
            }
            logger.Information("ShortUrl not found in cache");
        }
        catch (RedisConnectionException e)
        {
            logger.Error(e, "Error when searching ShortUrl in cache");
        }
        
        shortUrl = await GetUrlByShortCodeAsync(shortCode);
        if (shortUrl is not null)
        {
            logger.Information("Saving ShortUrl to cache");
            try
            {
                await _cache.SetRecordAsync<ShortUrl>(shortCode, shortUrl);
                logger.Information("successfully saved ShortUrl to cache");
            }
            catch (RedisConnectionException e)
            {
                logger.Error(e, "Error when saving ShortUrl to cache");
            }
        }

        return shortUrl;
    }

    public async Task<List<ShortUrl>> GetShortUrlsWithFiltersAsync(int? userId, bool excludeExpiredUrls)
    {
        logger.Information("Getting ShortUrl list in database with filters");
        var query = _db.Urls.AsQueryable();
        if (userId.HasValue)
        {
            query = query.Where(url => url.UserId == userId.Value);
        }
        if (excludeExpiredUrls)
        {
            query = query.Where(url => !url.ExpiresAt.HasValue || url.ExpiresAt > DateTimeOffset.UtcNow);
        }
        
        try
        {
            List<ShortUrl> shortUrls = await query.ToListAsync();
            logger.Information("Found in database: {countShortUrls} ShortUrl", shortUrls.Count);
            return shortUrls;
        }
        catch (Exception e)
        {
            logger.Error(e, "Error when searching ShortUrl list with filters");
            throw;
        }
    }

    public async Task<bool> DeleteShortUrlByShortCodeAsync(string shortCode, int userId)
    {
        logger.Information("Searching ShortUrl to delete");
        ShortUrl? shortUrl = await GetUrlByShortCodeAsync(shortCode);
        if (shortUrl is null)
        {
            return false;
        }
        
        if (shortUrl.UserId != userId)
        {
            logger.Warning("User is trying to delete ShortUrl that is not his own");
            return false;
        }
        
        logger.Information("Deleting ShortUrl from cache");

        try
        {
            await _cache.RemoveAsync(shortCode);
            logger.Information("ShortUrl deleted from cache");
        }
        catch (Exception e)
        {
            logger.Error(e, "Error when deleting ShortUrl from database");
            throw;
        }
        
        logger.Information("Deleting ShortUrl from database");

        _db.Urls.Remove(shortUrl);
        
        try
        {
            await _db.SaveChangesAsync();
            logger.Information("ShortUrl deleted from database");
            return true;
        }
        catch (Exception e)
        {
            logger.Error(e, "Error when deleting ShortUrl from database");
            throw;
        }
    }
}