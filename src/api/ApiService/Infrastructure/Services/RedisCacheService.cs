using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using AspireAppTemplate.ApiService.Services;

namespace AspireAppTemplate.ApiService.Infrastructure.Services;

public class RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger) : ICacheService
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await cache.GetAsync(key, cancellationToken);
        if (bytes == null)
        {
            return default;
        }

        try 
        {
            return JsonSerializer.Deserialize<T>(bytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deserializing cache value for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
        };

        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        await cache.SetAsync(key, bytes, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(key, cancellationToken);
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (!EqualityComparer<T>.Default.Equals(cachedValue, default))
        {
            logger.LogInformation("Cache Hit: {Key}", key);
            return cachedValue;
        }

        logger.LogInformation("Cache Miss: {Key}", key);
        var value = await factory(cancellationToken);
        
        if (!EqualityComparer<T>.Default.Equals(value, default))
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }
}
