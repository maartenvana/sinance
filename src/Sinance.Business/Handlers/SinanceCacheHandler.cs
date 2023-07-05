using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Sinance.Business.Handlers;

/// <summary>
/// Cache helper for caching operations
/// </summary>
public static class SinanceCacheHandler
{
    // Memory cache for all cache operations
    private static readonly MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

    /// <summary>
    /// Places the result of the action if not present in cache
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    /// <param name="key">Key for identifying the cache item</param>
    /// <param name="contentAction">Action to run if no value present</param>
    /// <param name="slidingExpiration">If the expiration is a sliding expiration</param>
    /// <param name="expirationTimeSpan">Timespan for when the cache item expires</param>
    /// <returns>Cached object</returns>
    public static async Task<T> Cache<T>(string key, Func<Task<T>> contentAction, bool slidingExpiration, TimeSpan expirationTimeSpan)
    {
        var cacheObject = await _memoryCache.GetOrCreateAsync(key, async (entry) =>
        {
            if (slidingExpiration)
            {
                entry.SlidingExpiration = expirationTimeSpan;
            }
            else
            {
                entry.AbsoluteExpiration = DateTime.Now.AddSeconds(expirationTimeSpan.TotalSeconds);
            }

            return await contentAction.Invoke();
        });

        return cacheObject;
    }

    /// <summary>
    /// Places the result of the action if not present in cache
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    /// <param name="key">Key for identifying the cache item</param>
    /// <param name="contentAction">Action to run if no value present</param>
    /// <param name="slidingExpiration">If the expiration is a sliding expiration</param>
    /// <param name="expirationTimeSpan">Timespan for when the cache item expires</param>
    /// <returns>Cached object</returns>
    public static T Cache<T>(string key, Func<T> contentAction, bool slidingExpiration, TimeSpan expirationTimeSpan)
    {
        var cacheObject = _memoryCache.GetOrCreate(key, (entry) =>
        {
            if (slidingExpiration)
            {
                entry.SlidingExpiration = expirationTimeSpan;
            }
            else
            {
                entry.AbsoluteExpiration = DateTime.Now.AddSeconds(expirationTimeSpan.TotalSeconds);
            }

            return contentAction.Invoke();
        });

        return cacheObject;
    }

    /// <summary>
    /// Clears the cache for the given key
    /// </summary>
    /// <param name="key">Key to clear from cache</param>
    public static void ClearCache(string key)
    {
        _memoryCache.Remove(key);
    }

    /// <summary>
    /// Retrieves an item from the cache
    /// </summary>
    /// <typeparam name="T">Type of object</typeparam>
    /// <param name="key">Key to search for</param>
    /// <returns>Cacheobject if found</returns>
    public static T RetrieveCache<T>(string key)
    {
        _memoryCache.TryGetValue(key, out var cacheObject);

        return (T)cacheObject;
    }
}