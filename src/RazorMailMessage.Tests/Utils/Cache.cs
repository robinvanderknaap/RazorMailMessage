using System.Linq;
using System.Runtime.Caching;

namespace RazorMailMessage.Tests.Utils
{
    public static class Cache
    {
        public static void Clear()
        {
            var cacheKeys = MemoryCache.Default.Select(cacheItem => cacheItem.Key).ToList();

            foreach (var cacheKey in cacheKeys)
            {
                MemoryCache.Default.Remove(cacheKey);
            }
        }
    }
}
