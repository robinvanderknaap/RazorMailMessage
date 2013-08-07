using System.Collections.Generic;
using System.Net.Mail;

namespace RazorMailMessage
{
    public interface IRazorMailMessageFactory
    {
        MailMessage Create<TModel>(string templateName, TModel model);
        MailMessage Create<TModel>(string templateName, TModel model, IEnumerable<LinkedResource> linkedResources);
    }
}