using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Shortener.Extensions;

public static class DistributedCacheExtensions
{
    public static async Task SetRecordAsync<T>(this IDistributedCache cache,
        string key,
        T data,
        TimeSpan? absoluteExpireTime = null,
        TimeSpan? unusedExpireTime = null)
    {
        var options = new DistributedCacheEntryOptions();
        options.AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(10);
        options.SlidingExpiration = unusedExpireTime ??  TimeSpan.FromMinutes(10);

        var jsonData = JsonSerializer.Serialize(data);
        await cache.SetStringAsync(key, jsonData, options);
    }

    public static async Task<T?> GetRecordAsync<T>(this IDistributedCache cache, string key)
    {
        string? jsonData = await cache.GetStringAsync(key);

        if (jsonData is null)
        {
            return default(T);
        }
        
        return JsonSerializer.Deserialize<T>(jsonData);
    }
}