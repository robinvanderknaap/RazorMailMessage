using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using RazorMailMessage.TemplateResolvers;

namespace RazorMailMessage.Example
{
    class Program
    {
        // Provide your smtp settings here
        // Smtp4Dev is a nice dummy smtp server for localhost to quickly view the messages without actually sending them.
        // http://smtp4dev.codeplex.com/
        
        private static readonly SmtpClient SmtpClient = new SmtpClient("localhost", 8025);
        private const string FromEmailAddress = "robin@skaele.nl";
        private const string ToEmailAddress = "daan@skaele.nl";


        static void Main()
        {
            string option;
            do
            {
                // Create menu
                var menu = new StringBuilder();
                menu.AppendLine("(1) Send mail message with default settings");
                menu.AppendLine("(2) Send mail message with specific assembly and namespace");
                menu.AppendLine("(3) Send mail message with embedded image");
                menu.AppendLine("(4) Send mail message with layout and sections");
                menu.AppendLine("(x) Exit");

                // Display menu and wait for user input
                Console.WriteLine(menu);
                option = Console.ReadKey().KeyChar.ToString();
                Console.WriteLine();
                ExecuteOption(option);
                Console.WriteLine();
            } while (option.ToLower() != "x");
        }

        private static void ExecuteOption(string option)
        {
            switch (option.ToLower())
            {
                case "1":
                    SendMailMessageWithDefaultSettings();
                    break;
                case "2":
                    SendMailMessageWithSpecificAssemblyAndNameSpace();
                    break;
                case "3":
                    SendMailMessageWithEmbeddedImage();
                    break;
                case "4":
                    SendMailMessageWithLayoutAndSections();
                    break;
                case "x":
                    break;
                default:
                    Console.WriteLine("That's not an option!");
                    break;
            }
        }

        private static void SendMailMessageWithDefaultSettings()
        {
            var razorMailMessageFactory = new RazorMailMessageFactory();

            var mailMessage = razorMailMessageFactory.Create("MailTemplates.SendMailMessageWithDefaultSettings.TestTemplate.cshtml", new { Name = "Robin" });

            mailMessage.From = new MailAddress(FromEmailAddress);
            mailMessage.To.Add(new MailAddress(ToEmailAddress));
            mailMessage.Subject = "Test template";

            SmtpClient.Send(mailMessage);
        }

        private static void SendMailMessageWithSpecificAssemblyAndNameSpace()
        {
            var razorMailMessageFactory = new RazorMailMessageFactory
            (
                new DefaultTemplateResolver(Assembly.GetExecutingAssembly(), "MailTemplates")
            );

            var mailMessage = razorMailMessageFactory.Create("SendMailMessageWithSpecificAssemblyAndNameSpace.TestTemplate.cshtml", new { Name = "Robin" });

            mailMessage.From = new MailAddress(FromEmailAddress);
            mailMessage.To.Add(new MailAddress(ToEmailAddress));
            mailMessage.Subject = "Test template";

            SmtpClient.Send(mailMessage);
        }

        private static void SendMailMessageWithLayoutAndSections()
        {
            // Use namespace: MailTemplates.SendMailMessageWithEmbeddedImage
            var razorMailMessageFactory = new RazorMailMessageFactory
            (
                new DefaultTemplateResolver("MailTemplates")
            );

            var mailMessage = razorMailMessageFactory.Create
            (
                "SendMailMessageWithLayoutAndSections.TestTemplate.cshtml",
                new { Name = "Robin" }
            );

            mailMessage.From = new MailAddress(FromEmailAddress);
            mailMessage.To.Add(new MailAddress(ToEmailAddress));
            mailMessage.Subject = "Test template";

            SmtpClient.Send(mailMessage);
        }

        private static void SendMailMessageWithEmbeddedImage()
        {
            // Use namespace: MailTemplates.SendMailMessageWithEmbeddedImage
            var razorMailMessageFactory = new RazorMailMessageFactory
            (
                new DefaultTemplateResolver("MailTemplates")
            );

            var mailMessage = razorMailMessageFactory.Create
            (
                "SendMailMessageWithEmbeddedImage.TestTemplate.cshtml", 
                new { Name = "Robin" },
                new List<LinkedResource> { new LinkedResource("MailTemplates/SendMailMessageWithEmbeddedImage/chuck_mailheader.png") { ContentId = "chuckNorrisImage" } }
            );

            mailMessage.From = new MailAddress(FromEmailAddress);
            mailMessage.To.Add(new MailAddress(ToEmailAddress));
            mailMessage.Subject = "Test template";

            SmtpClient.Send(mailMessage);
        }
    }
}
