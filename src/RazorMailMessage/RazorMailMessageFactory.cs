using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorMailMessage.TemplateCache;
using RazorMailMessage.TemplateResolvers;
using ITemplateResolver = RazorMailMessage.TemplateResolvers.ITemplateResolver;

namespace RazorMailMessage
{
    public class RazorMailMessageFactory : IRazorMailMessageFactory
    {
        private readonly ITemplateResolver _templateResolver;
        private readonly ITemplateCache _templateCache;
        private readonly ITemplateService _templateService;

        public RazorMailMessageFactory() : this(new DefaultTemplateResolver(), new InMemoryTemplateCache()) { }

        public RazorMailMessageFactory(ITemplateResolver templateResolver, ITemplateCache templateCache)
        {
            _templateResolver = templateResolver;
            _templateCache = templateCache;

            var templateServiceConfiguration = new TemplateServiceConfiguration
            {
                // Layout resolver for razor engine
                // Once resolved, the layout will be cached, so the resolver is called only once
                Resolver = new DelegateTemplateResolver(_templateResolver.ResolveLayout)
            };

            _templateService = new TemplateService(templateServiceConfiguration);
        }

        public MailMessage Create<TModel>(string templateName, TModel model)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentNullException("templateName");
            }

            // Get parsed templates
            var htmlTemplate = ParseTemplate(templateName, model, false);
            var textTemplate = ParseTemplate(templateName, model, true);
            
            var mailMessage = new MailMessage {BodyEncoding = Encoding.UTF8};

            if (!string.IsNullOrWhiteSpace(textTemplate))
            {
                // Text version was found. Plain text version should be set on body property, html version on alternate view
                // http://msdn.microsoft.com/en-us/library/system.net.mail.mailmessage.alternateviews.aspx
                mailMessage.Body = textTemplate;

                mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlTemplate, Encoding.UTF8, "text/html"));
            }
            else
            {
                mailMessage.Body = htmlTemplate;
                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = Encoding.UTF8;
            }

            return mailMessage;
        }

        private string ParseTemplate<TModel>(string templateName, TModel model, bool plainText)
        {
            var templateCacheName = ResolveTemplateCacheName(templateName, plainText);

            // Try to get template from cache
            var template = _templateCache.Get(templateCacheName);

            if (template == null)
            {
                // Resolve template and add to cache
                template = _templateResolver.ResolveTemplate(templateName, plainText);

                // In case template is not resolved (could be the case with plain text templates), we cache an empty string.
                _templateCache.Add(templateCacheName, template ?? "");
            }

            return string.IsNullOrWhiteSpace(template) ? string.Empty : _templateService.Parse(template, model, null, templateCacheName);
        }

        private static string ResolveTemplateCacheName(string templateName, bool plainText)
        {
            // Resolve template cache name based on culture and whether or not it is the plain text version
            var templateCacheNameParts = new List<string> {templateName};

            if (plainText)
            {
                templateCacheNameParts.Add("text");
            }

            var templateCacheName = string.Join(".", templateCacheNameParts);
            return templateCacheName;
        }
    }
}
