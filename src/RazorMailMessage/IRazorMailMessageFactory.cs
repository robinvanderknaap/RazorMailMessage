using System.Globalization;
using System.Net.Mail;

namespace RazorMailMessage
{
    public interface IRazorMailMessageFactory
    {
        MailMessage Create<TModel>(string templateName, TModel model);
    }
}