namespace RazorMailMessage.TemplateCache
{
    public interface ITemplateCache
    {
        string Get(string templateCacheName);
        void Add(string templateCacheName, string template);
    }
}
