using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorMailMessage.Exceptions
{
    public class TemplateNotFoundException : Exception
    {
        public TemplateNotFoundException(string templateName) : base(string.Format("Template '{0}' was not found", templateName))
        {
        }
    }
}
