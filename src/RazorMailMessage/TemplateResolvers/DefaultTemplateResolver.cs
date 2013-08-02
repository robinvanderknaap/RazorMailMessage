using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace RazorMailMessage.TemplateResolvers
{
    public class DefaultTemplateResolver : ITemplateResolver
    {
        private readonly Assembly _assembly;
        private readonly string _nameSpace;

        public DefaultTemplateResolver(){ }

        public DefaultTemplateResolver(Assembly assembly, string nameSpace)
        {
            _assembly = assembly;
            _nameSpace = nameSpace;
        }

        public string ResolveTemplate(string templateName, bool isPlainText)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentNullException("templateName");
            }
            
            // Convention: assembly:namespace.templatename.csthml (assembly is optional)
            var templateNameParts = templateName.Split(new[] { "::" }, StringSplitOptions.None);
            
            // Get assembly containing template
            // Order to look for:
            // - Assembly as specified in template name
            // - Assembly as specified in constuctor of this resolver
            // - Entry assembly
            var assembly = templateNameParts.Count() > 1
                               ? Assembly.Load(templateNameParts[0])
                               : _assembly ?? Assembly.GetEntryAssembly();

            var resourceName = templateNameParts.Count() > 1 ? templateNameParts[1] : templateName;

            var resourceNameParts = !string.IsNullOrWhiteSpace(_nameSpace) && templateNameParts.Count() == 1
                ? _nameSpace.Split('.').Concat(resourceName.Split('.')).ToList()
                : resourceName.Split('.').ToList();

            // Remove .cshtml part
            resourceNameParts.RemoveAt(resourceNameParts.Count - 1);

            if (isPlainText)
            {
                resourceNameParts.Add("text");
            }

            resourceNameParts.Add("cshtml");

            var fullResourceName =  assembly.GetName().Name + "." + string.Join(".", resourceNameParts);

            using (var template = assembly.GetManifestResourceStream(fullResourceName))
            {
                if (template == null)
                {
                    throw new MissingManifestResourceException(string.Format("Could not retrieve resource '{0}'", fullResourceName));
                }

                using (var streamReader = new StreamReader(template))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        public string ResolveLayout(string layoutName)
        {
            return ResolveTemplate(layoutName, false);
        }
    }
}