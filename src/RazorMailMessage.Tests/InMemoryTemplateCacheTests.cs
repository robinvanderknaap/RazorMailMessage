using System.Linq;
using System.Runtime.Caching;
using NUnit.Framework;
using RazorMailMessage.TemplateCache;

namespace RazorMailMessage.Tests
{
    [TestFixture]
    public class InMemoryTemplateCacheTests
    {
        [Test]
        public void CanCacheTemplate()
        {
            ClearCache();
            
            var cache = new InMemoryTemplateCache();

            Assert.IsNull(cache.Get("TestTemplate"));
            
            cache.Add("TestTemplate", "<b>Hello @Model.Name</b>");

            Assert.AreEqual("<b>Hello @Model.Name</b>", cache.Get("TestTemplate"));
        }

        private static void ClearCache()
        {
            var cacheKeys = MemoryCache.Default.Select(cacheItem => cacheItem.Key).ToList();
            
            foreach (var cacheKey in cacheKeys)
            {
                MemoryCache.Default.Remove(cacheKey);
            }
        }
    }
}
