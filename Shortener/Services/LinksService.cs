using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Shortener.Common.Utils;
using Shortener.Data;
using Shortener.Entities;
using Shortener.Extensions;
using Shortener.Models;

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
        ShortUrl? shortUrl = await _db.Urls.FirstOrDefaultAsync(url => url.ShortCode == shortCode);
        return shortUrl;
    }

    public async Task<ShortUrl?> GetCachedShortUrlByShortCodeAsync(string shortCode)
    {
        ShortUrl? shortUrl = await _cache.GetRecordAsync<ShortUrl?>(shortCode);
        if (shortUrl is not null)
        {
            return shortUrl;
        }
        
        shortUrl = await GetShortUrlByShortCodeAsync(shortCode);
        if (shortUrl is not null)
        {
            await _cache.SetRecordAsync<ShortUrl>(shortCode, shortUrl);
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

        _db.Urls.Remove(shortUrl);
        await _db.SaveChangesAsync();
        return true;
    }
}