namespace Shortener.Services;

public class UrlService
{
    // Получить оригинальную ссылку по сокращенному коду
    public async Task<string> Shorten(string shortenCode)
    {
        throw new NotImplementedException();
    }

    // Создать сокращенный код для ссылки
    public async Task<string> CreateShortenUrl(string url)
    {
        throw new NotImplementedException();
    }

    // Сохранить пару "сокращенный код - оригинальная ссылка" в хранилище
    public async Task SaveShortenUrl(string shortenCode, string originalUrl)
    {
        throw new NotImplementedException();
    }
}