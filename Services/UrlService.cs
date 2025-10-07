using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using Shortener.Data;
using Shortener.Entities;

namespace Shortener.Services;

public class UrlService
{
    private readonly ApplicationDbContext _db;
    private const int SHORT_CODE_LENGTH = 8;
    private const int EXTENDED_SHORT_CODE_LENGTH = 10;
    private const int MAX_ATTEMPTS = 5;
    private static readonly char[] URL_SAFE_ALPHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

    public UrlService(ApplicationDbContext db)
    {
        _db = db;
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

    // Сохранить пару "оригинальная ссылка - сокращенная ссылка" в хранилище
    private async Task SaveShortUrlToDatabase(ShortUrl shortUrlEntity)
    {
        _db.Urls.Add(shortUrlEntity);
        await _db.SaveChangesAsync();
    }
    
    // Получить оригинальную ссылку по коду
    public async Task<string?> GetOriginalUrlByShortCode(string shortCode)
    {
        var url = await _db.Urls.FirstOrDefaultAsync(u => u.ShortCode == shortCode &&
                                                          (!u.ExpiresAt.HasValue || u.ExpiresAt!.Value > DateTime.Now));
        return url?.OriginalUrl;
    }

    // Создать короткую ссылку
    public async Task<string> CreateShortUrl(string originalUrl, DateTime? expiresAt = null)
    {
        for (var attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            var shortCode = GenerateCode(SHORT_CODE_LENGTH);
            var urlEntity = new ShortUrl()
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                ExpiresAt = expiresAt
            };
            try
            {
                await SaveShortUrlToDatabase(urlEntity);
                return shortCode;
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqliteException sqliteEx && sqliteEx.SqliteExtendedErrorCode == 2067)
            {
                continue;
            }
        }

        for (var attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            var shortCode = GenerateCode(EXTENDED_SHORT_CODE_LENGTH);
            var urlEntity = new ShortUrl()
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                ExpiresAt = expiresAt
            };
            try
            {
                await SaveShortUrlToDatabase(urlEntity);
                return shortCode;
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqliteException sqliteEx && sqliteEx.SqliteExtendedErrorCode == 2067)
            {
                continue;
            }
        }

        throw new InvalidOperationException("Не удалось создать уникальный короткий код.");
    }
}