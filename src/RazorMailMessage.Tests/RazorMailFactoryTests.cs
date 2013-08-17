using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using Moq;
using NUnit.Framework;
using RazorEngine.Templating;
using RazorMailMessage.Exceptions;
using RazorMailMessage.TemplateBase;
using RazorMailMessage.TemplateCache;
using ITemplateResolver = RazorMailMessage.TemplateResolvers.ITemplateResolver;

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
                .Setup(x => x.ResolveTemplate(It.IsAny<string>(), false))
                .Returns("<b>Welcome @Model.Name</b>");

            // Request for plain text template returns empty string (indicating the plain text template should no be used)
            templateResolverMock
                .Setup(x => x.ResolveTemplate(It.IsAny<string>(), true))
                .Returns("");

            var razorMailMessageFactory = new RazorMailMessageFactory(templateResolverMock.Object, typeof(DefaultTemplateBase<>), null, new Mock<InMemoryTemplateCache>().Object);
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
                .Setup(x => x.ResolveTemplate(It.IsAny<string>(), false))
                .Returns("<b>Welcome @Model.Name</b>");
            
            // Setup plain text template request
            templateResolverMock
                    .Setup(x => x.ResolveTemplate(It.IsAny<string>(), true))
                    .Returns("Welcome @Model.Name");

            var razorMailMessageFactory = new RazorMailMessageFactory(templateResolverMock.Object, typeof(DefaultTemplateBase<>), null, new Mock<InMemoryTemplateCache>().Object);
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
                .Setup(x => x.ResolveTemplate("TestTemplate", false))
                .Returns("@{ Layout = \"TestLayout\";}<b>Welcome @Model.Name</b>");

            // Request for plain text template returns empty string (indicating the plain text template should no be used)
            templateResolverMock
                .Setup(x => x.ResolveTemplate("TestTemplate", true))
                .Returns("");

            var razorMailMessageFactory = new RazorMailMessageFactory(templateResolverMock.Object, typeof(DefaultTemplateBase<>), null, new Mock<InMemoryTemplateCache>().Object);
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
                .Setup(x => x.ResolveTemplate("TestTemplate", false))
                .Returns("@{ Layout = \"TestLayout\";}<b>Welcome @Model.Name</b>");

            // Request for plain text template
            templateResolverMock
                .Setup(x => x.ResolveTemplate("TestTemplate", true))
                .Returns("@{ Layout = \"TestLayout\";}Welcome @Model.Name");

            var razorMailMessageFactory = new RazorMailMessageFactory(templateResolverMock.Object, typeof(DefaultTemplateBase<>), null, new Mock<InMemoryTemplateCache>().Object);
            var model = new { Name = "Robin" };

            var mailMessage = razorMailMessageFactory.Create("TestTemplate", model);

            const string expectedResult = "Start Layout <b>Welcome Robin</b> End Layout";
            const string expectedPlainTextResult = "Start Layout Welcome Robin End Layout";
            
            Assert.AreEqual(1, mailMessage.AlternateViews.Count);
            Assert.AreEqual(expectedResult, new StreamReader(mailMessage.AlternateViews[0].ContentStream).ReadToEnd());
            Assert.AreEqual(expectedPlainTextResult, mailMessage.Body);
        }

        [Test]
        public void CanAddViewBag()
        {
            var templateResolverMock = new Mock<ITemplateResolver>();

            // Request for html template
            templateResolverMock
                .Setup(x => x.ResolveTemplate(It.IsAny<string>(), false))
                .Returns("<b>Welcome @ViewBag.Name</b>");

            // Request for plain text template returns empty string (indicating the plain text template should no be used)
            templateResolverMock
                .Setup(x => x.ResolveTemplate(It.IsAny<string>(), true))
                .Returns("");

            var razorMailMessageFactory = new RazorMailMessageFactory(templateResolverMock.Object, typeof(DefaultTemplateBase<>), null, new Mock<InMemoryTemplateCache>().Object);
            var model = new { };

            dynamic viewBag = new DynamicViewBag();
            viewBag.Name = "Robin";

            var mailMessage = razorMailMessageFactory.Create("TestTemplate", model, viewBag);

            const string expectedResult = "<b>Welcome Robin</b>";
            Assert.AreEqual(expectedResult, mailMessage.Body);
            Assert.AreEqual(expectedResult, new StreamReader(mailMessage.AlternateViews[0].ContentStream).ReadToEnd());
        }

        [Test]
        public void CanAddLinkedResourcesToEmail()
        {
            var templateResolverMock = new Mock<ITemplateResolver>();

            // Request for html template
            templateResolverMock
                .Setup(x => x.ResolveTemplate(It.IsAny<string>(), false))
                .Returns("<b>Welcome @Model.Name</b>");

            // Request for plain text template returns empty string (indicating the plain text template should not be used)
            templateResolverMock
                .Setup(x => x.ResolveTemplate(It.IsAny<string>(), true))
                .Returns("");

            var razorMailMessageFactory = new RazorMailMessageFactory(templateResolverMock.Object, typeof(DefaultTemplateBase<>), null, new Mock<InMemoryTemplateCache>().Object);
            var model = new { Name = "Robin" };

            var linkedResources = new List<LinkedResource>
                {
                    new LinkedResource(new MemoryStream(Encoding.ASCII.GetBytes("resource1"))) { ContentId = "resource1"},
                    new LinkedResource(new MemoryStream(Encoding.ASCII.GetBytes("resource2"))) { ContentId = "resource2"}
                };

            var mailMessage = razorMailMessageFactory.Create("TestTemplate", model, linkedResources);

            Assert.AreEqual(2, mailMessage.AlternateViews[0].LinkedResources.Count);
            Assert.AreEqual("resource1", new StreamReader(mailMessage.AlternateViews[0].LinkedResources[0].ContentStream).ReadToEnd());
            Assert.AreEqual("resource2", new StreamReader(mailMessage.AlternateViews[0].LinkedResources[1].ContentStream).ReadToEnd());
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
            templateCacheMock.Setup(x => x.Get("TestLayout")).Returns(layoutTemplate);

            var templateResolverMock = new Mock<ITemplateResolver>();
            
            var razorMailMessageFactory = new RazorMailMessageFactory(templateResolverMock.Object, typeof(DefaultTemplateBase<>), null, templateCacheMock.Object);
            var model = new { Name = "Robin" };

            var mailMessage = razorMailMessageFactory.Create("TestTemplate", model);

            const string expectedResult = "Start Layout <b>Welcome Robin</b> End Layout";
            const string expectedPlainTextResult = "Start Layout Welcome Robin End Layout";

            Assert.AreEqual(1, mailMessage.AlternateViews.Count);
            Assert.AreEqual(expectedResult, new StreamReader(mailMessage.AlternateViews[0].ContentStream).ReadToEnd());
            Assert.AreEqual(expectedPlainTextResult, mailMessage.Body);

            templateResolverMock.Verify(x => x.ResolveTemplate("TestTemplate", false), Times.Never());
            templateResolverMock.Verify(x => x.ResolveTemplate("TestTemplate", true), Times.Never());
            templateResolverMock.Verify(x => x.ResolveLayout("TestLayout"), Times.Never());
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(TemplateNotFoundException))]
        public void TemplateNotFoundExceptionIsThrownWhenTextAndHtmlTemplateAreMissing()
        {
            new RazorMailMessageFactory(new Mock<ITemplateResolver>().Object, typeof(DefaultTemplateBase<>), null, new Mock<InMemoryTemplateCache>().Object)
                .Create("test.cshtml", new { Name = "Robin" });
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void TemplateNameCannotBeEmpty()
        {
            new RazorMailMessageFactory(new Mock<ITemplateResolver>().Object, typeof(DefaultTemplateBase<>), null, new Mock<InMemoryTemplateCache>().Object)
                .Create(" ", new { Name = "Robin" });
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void TemplateResolverCannotBeNull()
        {
            new RazorMailMessageFactory(null);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void TemplateCacheCannotBeNull()
        {
            new RazorMailMessageFactory(new Mock<ITemplateResolver>().Object, typeof(DefaultTemplateBase<>), null, null);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void TemplateBaseTypeCannotBeNull()
        {
            new RazorMailMessageFactory(new Mock<ITemplateResolver>().Object, null, null, new Mock<InMemoryTemplateCache>().Object);
        }
    }
}
