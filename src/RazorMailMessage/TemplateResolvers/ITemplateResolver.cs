using System.Globalization;

namespace RazorMailMessage.TemplateResolvers
{
    public interface ITemplateResolver
    {
        string ResolveTemplate(string templateName, bool isPlainText, CultureInfo cultureInfo);
        string ResolveLayout(string layoutName);
    }
}