using Microsoft.Extensions.Caching.Memory;
using System;

namespace Sinance.Business.Handlers
{
    /// <summary>
    /// Cache helper for caching operations
    /// </summary>
    public static class FinanceCacheHandler
    {
        // Memory cache for all cache operations
        private static readonly MemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

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
            if (contentAction == null)
                throw new ArgumentNullException(nameof(contentAction));

            T cacheObject = (T)memoryCache.GetOrCreate(key, (entry) =>
           {
               if (slidingExpiration)
                   entry.SlidingExpiration = expirationTimeSpan;
               else
                   entry.AbsoluteExpiration = DateTime.Now.AddSeconds(expirationTimeSpan.TotalSeconds);

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
            memoryCache.Remove(key);
        }

        /// <summary>
        /// Retrieves an item from the cache
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="key">Key to search for</param>
        /// <returns>Cacheobject if found</returns>
        public static T RetrieveCache<T>(string key)
        {
            memoryCache.TryGetValue(key, out var cacheObject);

            return (T)cacheObject;
        }
    }
}