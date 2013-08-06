using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;

namespace RazorMailMessage
{
    public interface IRazorMailMessageFactory
    {
        MailMessage Create<TModel>(string templateName, TModel model);
        MailMessage Create<TModel>(string templateName, TModel model, IEnumerable<LinkedResource> linkedResources);
        MailMessage Create<TModel>(string templateName, TModel model, CultureInfo cultureInfo);
        MailMessage Create<TModel>(string templateName, TModel model, CultureInfo cultureInfo, IEnumerable<LinkedResource> linkedResources);
    }
}