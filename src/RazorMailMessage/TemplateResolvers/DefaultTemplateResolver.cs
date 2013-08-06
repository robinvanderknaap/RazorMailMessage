using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

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

        public string ResolveTemplate(string templateName, bool isPlainText, CultureInfo cultureInfo)
        {
            return ResolveTemplate(templateName, isPlainText, cultureInfo, isLayout:false);
        }

        public string ResolveLayout(string layoutName)
        {
            return ResolveTemplate(layoutName, false, CultureInfo.InvariantCulture, isLayout:true);
        }

        private string ResolveTemplate(string templateName, bool isPlainText, CultureInfo cultureInfo, bool isLayout)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentNullException("templateName");
            }

            // Convention: assemblyName::namespace.templatename.csthml (assembly is optional)
            var templateNameParts = templateName.Split(new[] {"::"}, StringSplitOptions.None);

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

            if (!isLayout) // Don't alter the resource name if specified as layout
            {
                // Remove extension from resource name
                var extensionPart = resourceNameParts.ElementAt(resourceNameParts.Count - 1);
                resourceNameParts.RemoveAt(resourceNameParts.Count - 1);

                // Add culture to resource name
                if (!cultureInfo.Equals(CultureInfo.InvariantCulture))
                {
                    resourceNameParts.Add(cultureInfo.Name);
                }

                // Add template type (text or html) ro resource name
                resourceNameParts.Add(isPlainText ? "text" : "html");

                // Add extension back
                resourceNameParts.Add(extensionPart);
            }

            // Construct full resource name
            var fullResourceName = assembly.GetName().Name + "." + string.Join(".", resourceNameParts);

            using (var template = assembly.GetManifestResourceStream(fullResourceName))
            {
                if (template == null)
                {
                    return null;
                }

                using (var streamReader = new StreamReader(template))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        
    }
}