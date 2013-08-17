using NUnit.Framework;
using RazorMailMessage.TemplateCache;
using RazorMailMessage.Tests.Utils;

namespace RazorMailMessage.Tests
{
    public class InMemoryTemplateCacheTests : BaseTestFixture
    {
        [Test]
        public void CanCacheTemplate()
        {
            var cache = new InMemoryTemplateCache();

            Assert.IsNull(cache.Get("TestTemplate"));
            
            cache.Add("TestTemplate", "<b>Hello @Model.Name</b>");

            Assert.AreEqual("<b>Hello @Model.Name</b>", cache.Get("TestTemplate"));
        }
    }
}
