using Microsoft.EntityFrameworkCore;
using Shortener.Data;
using Shortener.Entities;

namespace Shortener.Services;

public class UrlService
{
    private readonly ApplicationDbContext _db;

    public UrlService(ApplicationDbContext db)
    {
        _db = db;
    }
    
    // Получить оригинальную ссылку по сокращенному коду
    public async Task<string?> GetUrlByShortenCode(string shortenCode)
    {
        var url = await _db.Urls.FirstOrDefaultAsync(u => u.ShortenCode == shortenCode);
        return url?.OriginalUrl;
    }

    // Создать сокращенный код для ссылки
    public async Task<string> CreateShortenCode(string url)
    {
        var shortenCode = Guid.NewGuid().ToString().Substring(0, 8);
        
        var urlEntity = new Url()
        {
            OriginalUrl = url,
            ShortenCode = shortenCode
        };
        
        await SaveShortenUrl(urlEntity);
        return shortenCode;
    }

    // Сохранить пару "сокращенный код - оригинальная ссылка" в хранилище
    public async Task SaveShortenUrl(Url urlEntity)
    {
        _db.Urls.Add(urlEntity);
        await _db.SaveChangesAsync();
    }
}