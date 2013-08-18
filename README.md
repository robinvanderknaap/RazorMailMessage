RazorMailMessage
================
RazorMailMessage enables you to create .NET MailMessage objects based on Razor templates without a depedency on ASP.NET MVC. Support for layouts, sections, multi-part messages.

## NuGet

	Install-Package RazorMailMessage
	
## License
All source code is licensed under the [MIT License](https://github.com/robinvanderknaap/RazorMailMessage/blob/master/LICENSE)

## Usage
Messages are created by the RazorMailMessage factory. 

	var razorMailMessageFactory = new RazorMailMessageFactory();
	var mailMessage = razorMailMessageFactory.Create("MailTemplates.Template.cshtml", new { Name = "Robin" });

The default implementation of RazorMailMessage uses a template resolver based on embedded resources, so all templates need to have build action set to embedded resource.

Use the create method on the factory to create a mailmessage. The first parameter is the template name, this is the name including namespace. The second parameter is the model which can be used inside the template.

When templates are contained in another assembly, you have to configure the default template resolver

	var templateResolver = new DefaultTemplateResolver("NameOfAssembly", "MailTemplates");
	var razorMailMessageFactory = new RazorMailMessageFactory(templateResolver);
	var mailMessage = razorMailMessageFactory.Create("Template.cshtml", new { Name = "Robin" });

The constructor of the default template resolver takes an assemblyname and namespace as parameters. This way we have cleaner template names when creating a mailmessage.



    



