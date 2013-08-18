RazorMailMessage
================
RazorMailMessage enables you to create .NET MailMessage objects based on Razor templates without a depedency on ASP.NET MVC. Support for layouts, sections, viewbag and multi-part messages.

## NuGet

	Install-Package RazorMailMessage
	
## License
All source code is licensed under the [MIT License](https://github.com/robinvanderknaap/RazorMailMessage/blob/master/LICENSE)

## Usage
Messages are created by the RazorMailMessage factory. 

	var razorMailMessageFactory = new RazorMailMessageFactory();
	var mailMessage = razorMailMessageFactory.Create("MailTemplates.Template.cshtml", new { Name = "Robin" });

The default implementation of RazorMailMessage uses a template resolver based on embedded resources, so all templates need to have build action set to embedded resource.

Use the create method on the factory to create a mailmessage. The first parameter is the template name, this is the name including namespace and file extension. The second parameter is the model which can be used inside the template.

When templates are contained in another assembly, you have to configure the default template resolver

	var templateResolver = new DefaultTemplateResolver("NameOfAssembly", "MailTemplates");
	var razorMailMessageFactory = new RazorMailMessageFactory(templateResolver);
	var mailMessage = razorMailMessageFactory.Create("Template.cshtml", new { Name = "Robin" });

The constructor of the default template resolver takes an assemblyname and a namespace as parameters. This way we have cleaner template names when creating a mailmessage.

## Layouts and sections
Layouts are resolved like the regular templates. The layout is specified within the template, make sure to include the namespace and file extension in the layoutname.

	@{
		Layout = "Layouts.Layout.cshtml";
	}

Sections are also supported, and work the same way as in ASP.NET MVC razor templates.

## Template resolver
The default template resolver uses embedded resources to resolve templates. It's possible to use a custom template resolver. This allows for other ways of resolving templates, for example from the database or from the filesystem.
A custom resolver should implement the ITemplateResolver interface:

	public interface ITemplateResolver
    {
        string ResolveTemplate(string templateName, bool isPlainText);
        string ResolveLayout(string layoutName);
    }

The resolver should be specified when instatiating the RazorMessageFactory

	var razorMailMessageFactory = new RazorMailMessageFactory
    (
        new CustomTemplateResolver()
    );

## Caching
By default templates and layouts and templates are resolved only once and are then cached in memory. The caching mechanism can be overriden by a class implementing ITemplateCache

	public interface ITemplateCache
    {
        string Get(string templateCacheName);
        void Add(string templateCacheName, string template);
    }

The custom template cache can be set in the constructor of the RazorMailMessageFactory

## Baseclass
The constructor of the RazorMailMessage factory can also take an alternative baseclass for the templates in the constructor. The baseclass should inherit from TemplateBase<T>

	public class CustomTemplateBase<T> : TemplateBase<T>
    {

    }

To enable dependency injection in the baseclass, include a Func<Type, Object> method in the constructor in the constructor. Which will be called for every constructor parameter of the base class.

## ViewBag
ViewBag is supported. You can specify a ViewBag in the create method of the RazorMailMessageFactory.

	dynamic viewBag = new DynamicViewBag();
    viewBag.Name = "Robin";

    var mailMessage = razorMailMessageFactory.Create("TestTemplate", model, viewBag);

## Examples
In the source code you will find an example project. Which is a console application showcasing the possibilties of the library.





    



