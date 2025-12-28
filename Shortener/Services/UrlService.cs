using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Shortener.Data;
using Shortener.Entities;
using Shortener.Extensions;
using Shortener.Common.Utils;

namespace Shortener.Services;

public class UrlService
{
    private readonly Serilog.ILogger logger = Log.ForContext<UrlService>();
    private readonly ApplicationDbContext _db;
    private readonly IDistributedCache _cache;
    private const int SHORT_CODE_LENGTH = 8;

    public UrlService(ApplicationDbContext db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    // Сохранить короткую ссылку в базу данных
    private async Task SaveShortUrlToDatabase(ShortUrl shortUrlEntity)
    {
        var checkShortCode = await GetShortUrlByShortCode(shortUrlEntity.ShortCode);
        if (checkShortCode is not null)
        {
            throw new ArgumentException("Short code already exists");
        }
        
        _db.Urls.Add(shortUrlEntity);
        await _db.SaveChangesAsync();
    }
    
    // Получить оригинальную ссылку по коду
    public async Task<string?> GetOriginalUrlByShortCode(string shortCode)
    {
        logger.Information("Getting original url for short code");
        var url = await _db.Urls.FirstOrDefaultAsync(u => u.ShortCode == shortCode &&
                                                          (!u.ExpiresAt.HasValue || u.ExpiresAt!.Value > DateTimeOffset.UtcNow));
        return url?.OriginalUrl;
    }

    // Получить оригинальную ссылку по коду с предварительной проверкой её наличия в кэше
    public async Task<string?> GetCachedOriginalUrlByShortCode(string shortCode)
    {
        var originalUrl = await _cache.GetRecordAsync<string?>(shortCode);
        if (originalUrl is not null)
        {
            return originalUrl;
        }
        
        originalUrl = await GetOriginalUrlByShortCode(shortCode);
        if (originalUrl is not null)
        {
            await _cache.SetRecordAsync<string>(shortCode, (string)originalUrl);
        }
        
        return originalUrl;
    }

    // Создать короткую ссылку
    public async Task<string> CreateShortUrl(string originalUrl, DateTimeOffset? expiresAt = null)
    {
        logger.Information("Creating short url");
        
        logger.Information("Generating short code");
        var shortCode = ShortCodeGenerator.GenerateCode(SHORT_CODE_LENGTH);
        logger.Information("Short code generated");
        
        var urlEntity = new ShortUrl()
        {
            OriginalUrl = originalUrl,
            ShortCode = shortCode,
            ExpiresAt = expiresAt
        };
        
        logger.Information("Saving short url to database");
        await SaveShortUrlToDatabase(urlEntity);
        logger.Information("Short url saved");
        return shortCode;
    }

    // Получить все созданные ссылки
    public async Task<List<ShortUrl>> GetAllShortUrls()
    {
        var urls = await _db.Urls.ToListAsync();
        return urls;
    }
    
    // Получить ссылку по короткому коду
    public async Task<ShortUrl?> GetShortUrlByShortCode(string shortCode)
    {
        var url = await _db.Urls.FirstOrDefaultAsync(url => url.ShortCode == shortCode);
        return url;
    }

    // Удалить короткую ссылку по короткому коду
    public async Task<bool> DeleteShortUrl(string shortCode)
    {
        var url = await _db.Urls.FirstOrDefaultAsync(url => url.ShortCode == shortCode);
        if (url is null) return false;
        
        _db.Urls.Remove(url);
        await _db.SaveChangesAsync();
        return true;
    }
}