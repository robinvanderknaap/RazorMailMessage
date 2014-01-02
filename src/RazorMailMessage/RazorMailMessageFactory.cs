using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorMailMessage.Exceptions;
using RazorMailMessage.TemplateBase;
using RazorMailMessage.TemplateCache;
using RazorMailMessage.TemplateResolvers;
using ITemplateResolver = RazorMailMessage.TemplateResolvers.ITemplateResolver;

namespace RazorMailMessage
{
    public class RazorMailMessageFactory : IRazorMailMessageFactory, IDisposable
    {
        private readonly ITemplateResolver _templateResolver;
        private readonly ITemplateCache _templateCache;
        private readonly ITemplateService _templateService;

        public RazorMailMessageFactory() : this(new DefaultTemplateResolver(Assembly.GetCallingAssembly(), string.Empty), typeof(DefaultTemplateBase<>), null, new InMemoryTemplateCache()) { }

        public RazorMailMessageFactory(ITemplateResolver templateResolver) : this(templateResolver, typeof(DefaultTemplateBase<>), null, new InMemoryTemplateCache()) { }

        public RazorMailMessageFactory(ITemplateResolver templateResolver, Type templateBase) : this(templateResolver, templateBase, null, new InMemoryTemplateCache()) { }

        public RazorMailMessageFactory(ITemplateResolver templateResolver, Type templateBase, Func<Type, object> dependencyResolver) : this(templateResolver, templateBase, dependencyResolver, new InMemoryTemplateCache()) { }
       
        public RazorMailMessageFactory(ITemplateResolver templateResolver, Type templateBase, Func<Type, object> dependencyResolver, ITemplateCache templateCache)
        {
            if (templateResolver == null)
            {
                throw new ArgumentNullException("templateResolver");
            }
            if (templateCache == null)
            {
                throw new ArgumentNullException("templateCache");
            }
            if (templateBase == null)
            {
                throw new ArgumentNullException("templateBase");
            }
            
            _templateResolver = templateResolver;
            _templateCache = templateCache;

            var templateServiceConfiguration = new TemplateServiceConfiguration
            {
                // Layout resolver for razor engine
                // Once resolved, the layout will be cached by the razor engine, so the resolver is called only once during the lifetime of this factory
                // However, we want the ability to cache the layout even when the factory is instatiated multiple times
                Resolver = new DelegateTemplateResolver(layoutName =>
                    {
                        var layout = _templateCache.Get(layoutName);

                        if (layout == null)
                        {
                            layout = _templateResolver.ResolveLayout(layoutName);
                            _templateCache.Add(layoutName, layout);
                        }
                        return layout;
                    }),
                
                // Set view base class
                BaseTemplateType = templateBase
            };

            if (dependencyResolver != null)
            {
                templateServiceConfiguration.Activator = new Activators.Activator(dependencyResolver);
            }
            
            _templateService = new TemplateService(templateServiceConfiguration);
        }

        public virtual MailMessage Create<TModel>(string templateName, TModel model)
        {
            return Create(templateName, model, null, null);
        }

        public virtual MailMessage Create<TModel>(string templateName, TModel model, DynamicViewBag viewBag)
        {
            return Create(templateName, model, viewBag, null);
        }

        public virtual MailMessage Create<TModel>(string templateName, TModel model, IEnumerable<LinkedResource> linkedResources)
        {
            return Create(templateName, model, null, linkedResources);
        }

        public virtual MailMessage Create<TModel>(string templateName, TModel model, DynamicViewBag viewBag, IEnumerable<LinkedResource> linkedResources)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentNullException("templateName");
            }

            linkedResources = linkedResources ?? new List<LinkedResource>();

            // Get parsed templates
            var htmlTemplate = ParseTemplate(templateName, model, false, viewBag);
            var textTemplate = ParseTemplate(templateName, model, true, viewBag);

            var hasHtmlTemplate = !string.IsNullOrWhiteSpace(htmlTemplate);
            var hasTextTemplate = !string.IsNullOrWhiteSpace(textTemplate);

            if (!hasHtmlTemplate && !hasTextTemplate)
            {
                throw new TemplateNotFoundException(templateName);
            }

            var mailMessage = new MailMessage { BodyEncoding = Encoding.UTF8 };

            if (hasTextTemplate)
            {
                // Text version was found. Plain text version should be set on body property, html version on alternate view
                // http://msdn.microsoft.com/en-us/library/system.net.mail.mailmessage.alternateviews.aspx
                mailMessage.Body = textTemplate;
            }

            if (hasHtmlTemplate)
            {
                // Always create alternate view for html templates, linked resources can only be added to alternate view
                mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlTemplate, Encoding.UTF8, MediaTypeNames.Text.Html));

                foreach (var linkedResource in linkedResources)
                {
                    mailMessage.AlternateViews[0].LinkedResources.Add(linkedResource);
                }

                // If no text template available, html template should also be set on body
                if (!hasTextTemplate)
                {
                    mailMessage.Body = htmlTemplate;
                }
            }

            mailMessage.IsBodyHtml = !hasTextTemplate;

            return mailMessage;
        }

        private string ParseTemplate<TModel>(string templateName, TModel model, bool plainText, DynamicViewBag viewBag)
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

            return string.IsNullOrWhiteSpace(template) ? null : _templateService.Parse(template, model, viewBag, templateCacheName);
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

        public void Dispose()
        {
            if (_templateService != null)
            {
                _templateService.Dispose();
            }
        }
    }
}
