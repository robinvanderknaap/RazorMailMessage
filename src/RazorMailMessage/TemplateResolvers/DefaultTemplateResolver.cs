using System;
using System.IO;
using System.Linq;
using System.Reflection;
using RazorMailMessage.Exceptions;

namespace RazorMailMessage.TemplateResolvers
{
    public class DefaultTemplateResolver : ITemplateResolver
    {
        private readonly Assembly _assembly;
        private readonly string _assemblyName;
        private readonly string _nameSpace;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>Uses current assembly as template source</remarks>
        public DefaultTemplateResolver() : this(Assembly.GetCallingAssembly(), string.Empty) {}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nameSpace">Namespace containing templates</param>
        /// <remarks>Uses current assembly as template source</remarks>
        public DefaultTemplateResolver(string nameSpace) : this(Assembly.GetCallingAssembly(), nameSpace) {}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assemblyName">Name of assembly containing templates</param>
        /// <param name="nameSpace">Namespace containing templates</param>
        public DefaultTemplateResolver(string assemblyName, string nameSpace) : this(Assembly.Load(assemblyName), nameSpace) {}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembly">Assembly containing templates</param>
        /// <param name="nameSpace">Namespace containing templates</param>
        public DefaultTemplateResolver(Assembly assembly, string nameSpace)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            _assembly = assembly;
            _assemblyName = _assembly.GetName().Name;
            _nameSpace = (nameSpace ?? string.Empty).Trim();
        }

        public virtual string ResolveTemplate(string templateName, bool isPlainText)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentNullException("templateName");
            }

            var templateNameParts = templateName.Split('.').ToList();

            // Add assembly
            templateNameParts.Insert(0, _assemblyName);
            
            // Addname namespace
            if (!string.IsNullOrWhiteSpace(_nameSpace))
            {
                templateNameParts.Insert(1, _nameSpace);
            }

            // Add text to filename in case of text version
            if (isPlainText)
            {
                templateNameParts.Insert(templateNameParts.Count - 1, "text");
            }

            // Construct full resource name
            var fullResourceName = string.Join(".", templateNameParts);

            using (var template = _assembly.GetManifestResourceStream(fullResourceName))
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

        public virtual string ResolveLayout(string layoutName)
        {
            var layout = ResolveTemplate(layoutName, false);

            if (string.IsNullOrWhiteSpace(layout))
            {
                throw new TemplateNotFoundException(layoutName);
            }

            return layout;
        }
    }
}