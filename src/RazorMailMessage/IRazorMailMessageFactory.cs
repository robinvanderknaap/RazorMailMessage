using System.Collections.Generic;
using System.Net.Mail;
using RazorEngine.Templating;

namespace RazorMailMessage
{//adasd
    public interface IRazorMailMessageFactory
    {
        MailMessage Create<TModel>(string templateName, TModel model);
        MailMessage Create<TModel>(string templateName, TModel model, DynamicViewBag viewBag);
        MailMessage Create<TModel>(string templateName, TModel model, IEnumerable<LinkedResource> linkedResources);
        MailMessage Create<TModel>(string templateName, TModel model, DynamicViewBag viewBag, IEnumerable<LinkedResource> linkedResources);
    }
}
