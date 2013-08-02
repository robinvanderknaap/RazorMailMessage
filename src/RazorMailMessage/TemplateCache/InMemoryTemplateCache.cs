using System.Runtime.Caching;

namespace RazorMailMessage.TemplateCache
{
    public class InMemoryTemplateCache : ITemplateCache
    {
        private readonly MemoryCache _cache = MemoryCache.Default;

        public string Get(string templateCacheName)
        {
            var cacheItem = _cache.Get(templateCacheName);
            return cacheItem == null ? null : cacheItem.ToString();
        }

        public void Add(string templateCacheName, string template)
        {
            _cache.Add(templateCacheName, template, new CacheItemPolicy());
        }
    }
}