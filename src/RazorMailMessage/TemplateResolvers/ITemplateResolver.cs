namespace RazorMailMessage.TemplateResolvers
{
    public interface ITemplateResolver
    {
        string ResolveTemplate(string templateName, bool isPlainText);
        string ResolveLayout(string layoutName);
    }
}