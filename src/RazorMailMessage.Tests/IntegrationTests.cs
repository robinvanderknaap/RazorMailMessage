using System.IO;
using NUnit.Framework;
using RazorMailMessage.Tests.Utils;

namespace RazorMailMessage.Tests
{
    [TestFixture]
    class IntegrationTests
    {
        [Test]
        public void CanCreateEmailMessageWithRazorTemplate()
        {
            var razorMailMessageFactory = new RazorMailMessageFactory();

            var mailMessage = razorMailMessageFactory.Create
            (
                "TestTemplates.IntegrationTest.TestTemplate.cshtml",
                new { Name = "Robin" }
            );

            const string expectedResult = @"
                Start layout 
                <p>This is the header</p>
                <b>This is a test</b>
                <p>This is the footer</p>
                End layout
            ";

            const string expectedPlainTextResult = @"
                Start layout 
                This is the header
                This is a test
                This is the footer
                End layout
            ";

            Assert.AreEqual(1, mailMessage.AlternateViews.Count);
            Assert.AreEqual(expectedResult.StripWhiteSpace(), new StreamReader(mailMessage.AlternateViews[0].ContentStream).ReadToEnd().StripWhiteSpace());
            Assert.AreEqual(expectedPlainTextResult.StripWhiteSpace(), mailMessage.Body.StripWhiteSpace());
        }

        
    }
}
