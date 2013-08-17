using System.Linq;
using System.Runtime.Caching;
using NUnit.Framework;

namespace RazorMailMessage.Tests.Utils
{
    [TestFixture]
    public class BaseTestFixture
    {
        [SetUp]
        public void SetUp()
        {
            ClearCache();
        }

        [TearDown]
        public void TearDown()
        {
            ClearCache();
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
