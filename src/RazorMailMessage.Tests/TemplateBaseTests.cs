using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using RazorMailMessage.TemplateBase;
using RazorMailMessage.TemplateCache;
using RazorMailMessage.TemplateResolvers;

namespace RazorMailMessage.Tests
{
    [TestFixture]
    class TemplateBaseTests
    {
        [Test]
        public void CanCreateCustomTemplateBaseClass()
        {
            const string template = "@Revert(\"12345\")";

            var templateResolverMock = new Mock<ITemplateResolver>();

            // Request for html template
            templateResolverMock
                .Setup(x => x.ResolveTemplate(It.IsAny<string>(), false))
                .Returns(template);

            // Request for plain text template returns empty string (indicating the plain text template should no be used)
            templateResolverMock
                .Setup(x => x.ResolveTemplate(It.IsAny<string>(), true))
                .Returns("");

            var razorMailMessageFactory = new RazorMailMessageFactory(templateResolverMock.Object, new Mock<ITemplateCache>().Object, typeof(CustomTemplateBase<>));

            var mailMessage = razorMailMessageFactory.Create("TestTemplate", new {});

            const string expectedResult = "54321";
            Assert.AreEqual(expectedResult, mailMessage.Body);
            Assert.AreEqual(expectedResult, new StreamReader(mailMessage.AlternateViews[0].ContentStream).ReadToEnd());
        }
    }

    public class CustomTemplateBase<T> : DefaultTemplateBase<T>
    {
        public string Revert(string value)
        {
            return string.Join("", value.Reverse());
        }
    }
}
