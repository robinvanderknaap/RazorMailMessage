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

The default implementation of RazorMailMessage uses a template resolver based on embedded resources, so all templates need to have build action set to embedded resource.
Use the create method on the factory to create a mailmessage. The first parameter is the template name, this is the name including namespace. The second parameter is the model which can be used inside the template.

    var mailMessage = razorMailMessageFactory.Create("MailTemplates.Template.cshtml", new { Name = "Robin" });



