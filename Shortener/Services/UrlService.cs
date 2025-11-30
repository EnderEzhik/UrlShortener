using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Shortener.Data;
using Shortener.Entities;
using Shortener.Extensions;

namespace Shortener.Services;

public class UrlService
{
    private readonly Serilog.ILogger logger = Log.ForContext<UrlService>();
    private readonly ApplicationDbContext _db;
    private readonly IDistributedCache _cache;
    private const int SHORT_CODE_LENGTH = 8;
    private static readonly char[] URL_SAFE_ALPHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

    public UrlService(ApplicationDbContext db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    // Сгенерировать код для короткой ссылки
    private static string GenerateCode(int length)
    {
        var buffer = new char[length];
        for (var i = 0; i < length; i++)
        {
            var index = RandomNumberGenerator.GetInt32(URL_SAFE_ALPHABET.Length);
            buffer[i] = URL_SAFE_ALPHABET[index];
        }
        return new string(buffer);
    }

    // Сохранить короткую ссылку в базу данных
    private async Task SaveShortUrlToDatabase(ShortUrl shortUrlEntity)
    {
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
        var shortCode = GenerateCode(SHORT_CODE_LENGTH);
        var urlEntity = new ShortUrl()
        {
            OriginalUrl = originalUrl,
            ShortCode = shortCode,
            ExpiresAt = expiresAt
        };
        logger.Information("Short code generated");
        logger.Information("Saving short url to database");
        await SaveShortUrlToDatabase(urlEntity);
        logger.Information("Short url saved");
        return shortCode;
    }
}