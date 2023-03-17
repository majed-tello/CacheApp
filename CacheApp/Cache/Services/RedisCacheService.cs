using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CacheApp.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly DistributedCacheEntryOptions _cacheOptions;

    public RedisCacheService(IDistributedCache distributedCache, CacheOptions options)
    {
        _distributedCache = distributedCache;
        _cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(options.DefaultExpirationInMinutes) };
    }
    public async Task<T?> GetAsync<T>(string cacheKey)
    {
        var stringValue = await _distributedCache.GetStringAsync(cacheKey);

        return string.IsNullOrWhiteSpace(stringValue) ? default! : JsonSerializer.Deserialize<T>(stringValue);
    }

    public async Task<string> GetStringAsync(string cacheKey)
    {
        var stringValue = await _distributedCache.GetStringAsync(cacheKey);

        return string.IsNullOrWhiteSpace(stringValue) ? default! : stringValue;
    }

    public async Task RemoveAsync(string cacheKey)
    {
        await _distributedCache.RemoveAsync(cacheKey);
    }

    public async Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiration = null)
    {
        if (value is null)
            return;

        var serializedValue = JsonSerializer.Serialize(value);

        if (expiration is not null)
        {
            await _distributedCache.SetStringAsync(cacheKey, serializedValue, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration });
            return;
        }

        await _distributedCache.SetStringAsync(cacheKey, serializedValue, _cacheOptions);
    }
}