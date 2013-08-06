using System;
using System.Globalization;
using System.IO;
using Moq;
using NUnit.Framework;
using RazorMailMessage.TemplateBase;
using RazorMailMessage.TemplateCache;
using RazorMailMessage.TemplateResolvers;

namespace RazorMailMessage.Tests
{
    [TestFixture]
    public class RazorMailFactoryTests
    {
        [Test]
        public void CanCreateMailMessage()
        {
            var templateResolverMock = new Mock<ITemplateResolver>();

            // Request for html template
            templateResolverMock
                .Setup(x => x.ResolveTemplate(It.IsAny<string>(), false, It.IsAny<CultureInfo>()))
                .Returns("<b>Welcome @Model.Name</b>");

            // Request for plain text template returns empty string (indicating the plain text template should no be used)
            templateResolverMock
                .Setup(x => x.ResolveTemplate(It.IsAny<string>(), true, It.IsAny<CultureInfo>()))
                .Returns("");

            var razorMailMessageFactory = new RazorMailMessageFactory(templateResolverMock.Object, new Mock<ITemplateCache>().Object, typeof(DefaultTemplateBase<>));
            var model = new { Name = "Robin" };

            var mailMessage = razorMailMessageFactory.Create("TestTemplate", model);

            const string expectedResult = "<b>Welcome Robin</b>";
            Assert.AreEqual(expectedResult, mailMessage.Body);
            Assert.AreEqual(expectedResult, new StreamReader(mailMessage.AlternateViews[0].ContentStream).ReadToEnd());
        }

        [Test]
        public void CanCreateMultipartMailMessage()
        {
            var templateResolverMock = new Mock<ITemplateResolver>();

            // Request for html template
            templateResolverMock
                .Setup(x => x.ResolveTemplate(It.IsAny<string>(), false, It.IsAny<CultureInfo>()))
                .Returns("<b>Welcome @Model.Name</b>");
            
            // Setup plain text template request
            templateResolverMock
                    .Setup(x => x.ResolveTemplate(It.IsAny<string>(), true, It.IsAny<CultureInfo>()))
                    .Returns("Welcome @Model.Name");

            var razorMailMessageFactory = new RazorMailMessageFactory(templateResolverMock.Object, new Mock<ITemplateCache>().Object, typeof(DefaultTemplateBase<>));
            var model = new { Name = "Robin" };

            var mailMessage = razorMailMessageFactory.Create("TestTemplate", model);

            const string expectedResult = "<b>Welcome Robin</b>";
            const string expectedPlainTextResult = "Welcome Robin";

            Assert.AreEqual(1, mailMessage.AlternateViews.Count);
            Assert.AreEqual(expectedResult, new StreamReader(mailMessage.AlternateViews[0].ContentStream).ReadToEnd());
            Assert.AreEqual(expectedPlainTextResult, mailMessage.Body);
        }

        [Test]
        public void CanCreateMailMessageWithLayout()
        {
            var templateResolverMock = new Mock<ITemplateResolver>();

            // Request for layout
            templateResolverMock
                .Setup(x => x.ResolveLayout("TestLayout"))
                .Returns("Start Layout @RenderBody() End Layout");

            // Request for html template
            templateResolverMock
                .Setup(x => x.ResolveTemplate("TestTemplate", false, It.IsAny<CultureInfo>()))
                .Returns("@{ Layout = \"TestLayout\";}<b>Welcome @Model.Name</b>");

            // Request for plain text template returns empty string (indicating the plain text template should no be used)
            templateResolverMock
                .Setup(x => x.ResolveTemplate("TestTemplate", true, It.IsAny<CultureInfo>()))
                .Returns("");

            var razorMailMessageFactory = new RazorMailMessageFactory(templateResolverMock.Object, new Mock<ITemplateCache>().Object, typeof(DefaultTemplateBase<>));
            var model = new { Name = "Robin" };

            var mailMessage = razorMailMessageFactory.Create("TestTemplate", model);

            const string expectedResult = "Start Layout <b>Welcome Robin</b> End Layout";
            Assert.AreEqual(expectedResult, mailMessage.Body);
            Assert.AreEqual(expectedResult, new StreamReader(mailMessage.AlternateViews[0].ContentStream).ReadToEnd());
        }

        [Test]
        public void CanCreateMultipartMailMessageWithLayout()
        {
            var templateResolverMock = new Mock<ITemplateResolver>();

            // Request for layout
            templateResolverMock
                .Setup(x => x.ResolveLayout("TestLayout"))
                .Returns("Start Layout @RenderBody() End Layout");

            // Request for html template
            templateResolverMock
                .Setup(x => x.ResolveTemplate("TestTemplate", false, It.IsAny<CultureInfo>()))
                .Returns("@{ Layout = \"TestLayout\";}<b>Welcome @Model.Name</b>");

            // Request for plain text template
            templateResolverMock
                .Setup(x => x.ResolveTemplate("TestTemplate", true, It.IsAny<CultureInfo>()))
                .Returns("@{ Layout = \"TestLayout\";}Welcome @Model.Name");

            var razorMailMessageFactory = new RazorMailMessageFactory(templateResolverMock.Object, new Mock<ITemplateCache>().Object, typeof(DefaultTemplateBase<>));
            var model = new { Name = "Robin" };

            var mailMessage = razorMailMessageFactory.Create("TestTemplate", model);

            const string expectedResult = "Start Layout <b>Welcome Robin</b> End Layout";
            const string expectedPlainTextResult = "Start Layout Welcome Robin End Layout";
            
            Assert.AreEqual(1, mailMessage.AlternateViews.Count);
            Assert.AreEqual(expectedResult, new StreamReader(mailMessage.AlternateViews[0].ContentStream).ReadToEnd());
            Assert.AreEqual(expectedPlainTextResult, mailMessage.Body);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void TemplateNameCannotBeEmpty()
        {
            new RazorMailMessageFactory(new Mock<ITemplateResolver>().Object, new Mock<ITemplateCache>().Object, typeof(DefaultTemplateBase<>)).Create(" ", new { Name = "Robin" });
        }
        
        [Test]
        public void TemplatesAreOnlyResolvedWhenNotCached()
        {
            const string layoutTemplate = "Start Layout @RenderBody() End Layout";
            const string htmlTemplate = "@{ Layout = \"TestLayout\";}<b>Welcome @Model.Name</b>";
            const string textTemplate = "@{ Layout = \"TestLayout\";}Welcome @Model.Name";
            
            var templateCacheMock = new Mock<ITemplateCache>();

            templateCacheMock.Setup(x => x.Get("TestTemplate")).Returns(htmlTemplate);
            templateCacheMock.Setup(x => x.Get("TestTemplate.text")).Returns(textTemplate);

            var templateResolverMock = new Mock<ITemplateResolver>();

            // Request for layout, caching of layouts is done by razor engine.
            templateResolverMock
                .Setup(x => x.ResolveLayout("TestLayout"))
                .Returns(layoutTemplate);

            var razorMailMessageFactory = new RazorMailMessageFactory(templateResolverMock.Object, templateCacheMock.Object, typeof(DefaultTemplateBase<>));
            var model = new { Name = "Robin" };

            var mailMessage = razorMailMessageFactory.Create("TestTemplate", model);

            const string expectedResult = "Start Layout <b>Welcome Robin</b> End Layout";
            const string expectedPlainTextResult = "Start Layout Welcome Robin End Layout";
            
            Assert.AreEqual(1, mailMessage.AlternateViews.Count);
            Assert.AreEqual(expectedResult, new StreamReader(mailMessage.AlternateViews[0].ContentStream).ReadToEnd());
            Assert.AreEqual(expectedPlainTextResult, mailMessage.Body);

            templateResolverMock.Verify(x => x.ResolveTemplate("TestTemplate", false, It.IsAny<CultureInfo>()), Times.Never());
            templateResolverMock.Verify(x => x.ResolveTemplate("TestTemplate", true, It.IsAny<CultureInfo>()), Times.Never());

            // Layout is used twice, once for html, once for text. Layout should be resolved once though, because of the caching of the razor engine itself.
            templateResolverMock.Verify(x => x.ResolveLayout("TestLayout"), Times.Once()); 

        }
    }
}
