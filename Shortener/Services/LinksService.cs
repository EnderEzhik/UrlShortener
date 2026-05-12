using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using Serilog.Context;
using Shortener.Common.Utils;
using Shortener.Data;
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

        using (LogContext.PushProperty("ShortCode", shortCode))
        {
            _logger.Debug("Short code generated");
            
            var shortUrl = new ShortUrl()
            {
                OriginalUrl = url,
                ShortCode = shortCode,
                ExpiresAt = expiresAt,
                OwnerId = userId
            };
        
            try
            {
                _db.Urls.Add(shortUrl);
                await _db.SaveChangesAsync();
                
                _logger.Information("Short url saved to database");
                
                return shortUrl;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Database error while saving short url");
                throw;
            }
        }
    }

    public async Task<ShortUrl?> GetUrlByShortCodeAsync(string shortCode)
    {
        try
        {
            return await _db.Urls.SingleOrDefaultAsync(url => url.ShortCode == shortCode);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Database error while getting short url");
            throw;
        }
    }

    public async Task<ShortUrl?> GetCachedShortUrlByShortCodeAsync(string shortCode)
    {
        ShortUrl? shortUrl;
        
        try
        {
            shortUrl = await _cache.GetRecordAsync<ShortUrl?>(shortCode);
            if (shortUrl is not null)
            {
                _logger.Debug("Short url found in cache");
                return shortUrl;
            }
        }
        catch (RedisConnectionException ex)
        {
            _logger.Error(ex, "Cache error while getting short url");
        }
        
        shortUrl = await GetUrlByShortCodeAsync(shortCode);
        if (shortUrl is not null)
        {
            _logger.Debug("Short url found in database");
            try
            {
                await _cache.SetRecordAsync<ShortUrl>(shortCode, shortUrl);
                _logger.Debug("Short url successfully saved in cache");
            }
            catch (RedisConnectionException e)
            {
                _logger.Error(e, "Cache error while saving short url");
            }
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
        
        try
        {
            List<ShortUrl> shortUrls = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return shortUrls;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Database error while getting short urls list");
            throw;
        }
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
        
        try
        {
            await _cache.RemoveAsync(shortCode);
            
            _logger.Debug("Short url deleted from cache");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Cache error while deleting short url");
            throw;
        }
        
        try
        {
            _db.Urls.Remove(shortUrl);
            await _db.SaveChangesAsync();
            
            _logger.Debug("Short url deleted from database");
            
            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Database error while deleting short url");
            throw;
        }
    }
}