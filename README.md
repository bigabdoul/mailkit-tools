# mailkit-tools

MailkitTools provides e-mail services built on top of the popular MailKit library.
These services are ideal to use within any .NET Core project that requires e-mail
services, such as sending messages with an SMTP client and receiving e-mails with
a POP3 client.

## Simplified email services support

If you have a pre-configured email client  (i.e., static configuration), you may 
immediately send messages using the `IConfiguredEmailService`. In a .NET 6 or later
framework, you can initialize all email services using the following statement (in
the `Program.cs` file of an ASP.NET Core 6+ Razor Pages project, for instance):
`builder.Services.AddMailkitTools(builder.Configuration.GetSection(nameof(EmailClientConfiguration)).Get<EmailClientConfiguration>());`

The above statement adds a singleton service of type `IEmailClientConfiguration`, 
a transient service of type `IConfiguredEmailService` and the new default 
`IEmailConfigurationProvider` service, named `DefaultEmailConfigurationProvider.`

```C#
using MailkitTools;
using MailkitTools.DependencyInjection;

namespace MyEmailProject;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        // add default pre-configured email services; this statement relies
        // on a matching configuration section in the appsettings.config file
        // named EmailClientConfiguration.
        builder.Services.AddMailkitTools(builder.Configuration.GetSection(nameof(EmailClientConfiguration)).Get<EmailClientConfiguration>());

        var app = builder.Build();
        // rest of the code omitted for brevity
    }
}
```

## Using `IConfiguredEmailService`

Within a controller or `PageModel`, you can send an email as shown in the following
code sample (in a Contact.cshtml page):

```C#
using MailkitTools;
using MailkitTools.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyEmailProject.Pages;

public class ContactModel : PageModel
{
    private readonly ILogger<ContactModel> _logger;
    private readonly IConfiguredEmailService _emailService;
    private readonly IConfiguration _configuration;

    public ContactModel(ILogger<ContactModel> logger, IConfiguredEmailService emailService, IConfiguration configuration)
    {
        _logger = logger;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<IActionResult> OnPostAsync(ContactFormModel model)
    {
        try
        {
            var fromAddr = _configuration[$"{nameof(EmailClientConfiguration)}:UserName"];
            var sendto = _configuration[$"{nameof(EmailClientConfiguration)}:SendTo"];

            var message = "A contact form has been submitted on example.com<br/>" +
                $"Name : {model.Name}<br/>" +
                $"Email : {model.Email}<br/>" +
                $"Subject : {model.Subject}<br/><br/>" +
                $"{model.Message}";

            var result = await _emailService.SendMessageAsync(fromAddr, sendto, model.Subject, message);
                
            if (result)
                return Content("OK");
            else
            {
                if (_emailService.LastError != null)
                    _logger.LogError("Error while sending an email: {message}", _emailService.LastError);

                return Content("Your message has not been sent.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("{message}", ex);
            return BadRequest(ex);
        }
    }
}

public class ContactFormModel
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Message { get; set; } = null!;
}
```

The `Contact.cshtml` page's content may resemble the following:
```HTML
<form method="post" role="form">
    <div class="row">
        <div class="form-group col-md-6">
            <label for="name">Your Name</label>
            <input type="text" name="Name" class="form-control" id="name" required>
        </div>
        <div class="form-group col-md-6">
            <label for="email">Your Email</label>
            <input type="email" class="form-control" name="Email" id="email" required>
        </div>
    </div>
    <div class="form-group">
        <label for="subject">Subject</label>
        <input type="text" class="form-control" name="Subject" id="subject" required>
    </div>
    <div class="form-group">
        <label for="message">Message</label>
        <textarea class="form-control" name="Message" id="message" rows="10" required></textarea>
    </div>
    <div class="text-center"><button type="submit">Send</button></div>
</form>
```

In the appsettings.json configuration file (update appropriately):

```JSON
{
    "EmailClientConfiguration": {
    "Host": "smtp.example.com",
    "Port": 465,
    "UseSsl": true,
    "RequiresAuth": true,
    "UserName": "username@example.com",
    "Password": "some secure password",
    "SendTo": "info@example.com"
  }
}
```

### Getting started with the email sender:

You can still use the `IEmailSender` service, and an implementation of the interface 
is now included in the source. Here's a simplified implementation of the 
`MailkitTools.IEmailConfigurationProvider` interface:

```C#
using MailkitTools;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

public class EmailConfigurationProvider : EmailConfigurationProviderBase
{
    private readonly IEmailClientConfiguration _clientConfig;

    public EmailConfigurationProvider(IOptions<EmailClientConfiguration> clientConfig)
    {
        _clientConfig = clientConfig.Value;
    }

    public override Task<IEmailClientConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        // normally, you would retrieve the settings from a (file or database) store;
        return Task.Run(() => _clientConfig);
    }
}
```

In `Startup.cs`:

```C#
using MailkitTools.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // ...
        services.AddMailkitTools<EmailConfigurationProvider>();

        // Don't forget to add a section named "EmailClientConfiguration" in the appsettings.json file
        services.Configure<EmailClientConfiguration>(Configuration.GetSection(nameof(EmailClientConfiguration)));
        // ...
    }
}
```

In the appsettings.json configuration file (update appropriately):

```JSON
{
    "EmailClientConfiguration": {
    "Host": "smtp.example.com",
    "Port": 25,
    "UseSsl": false,
    "RequiresAuth": false,
    "UserName": "username@example.com",
    "Password": "some secure password"
  }
}
```

Somewhere in a Models folder...

```C#

public class EmailModel
{
    public string Subject { get; set; }
    public string Body { get; set; }
    public string From { get; set; }
    [System.ComponentModel.DataAnnotations.Required]
    public string To { get; set; }
}

public class TestEmailModel
{
    public EmailModel Message { get; set; }
    public EmailClientConfiguration Config { get; set; }
}
```

... and a controller (if applicable):

```C#
using System.Threading.Tasks;
using MailkitTools;
using MailkitTools.Services;
using Microsoft.AspNetCore.Mvc;

public class EmailController : Controller
{
    private readonly IEmailClientService _emailClient;
    private readonly IEmailConfigurationProvider _emailConfigProvider;

    public EmailController(IEmailClientService emailClient, IEmailConfigurationProvider emailConfigProvider)
    {
        _emailClient = emailClient;
        _emailConfigProvider = emailConfigProvider;
    }

    [HttpPost]
    public Task<IActionResult> Send([FromBody] EmailModel model)
    {
        return SendMessageAsync(model);
    }

    [HttpPost]
    public Task<IActionResult> Test([FromBody] TestEmailModel model)
    {
        return SendMessageAsync(model.Message, model.Config);
    }

    protected async Task<IActionResult> SendMessageAsync(EmailModel model, IEmailClientConfiguration config = null)
    {
        if (config == null)
            config = await _emailConfigProvider.GetConfigurationAsync();
        _emailClient.Configuration = config;

        // check the ControllerExtensions class below
        return await this.SendEmailAsync(model, _emailClient);
    }
}
```

You can even improve reusability using an extension method:

```C#
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using MailKit;
using MailKit.Security;
using MailkitTools.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Provides extension methods for instances of the <see cref="Controller"/> class.
/// </summary>
public static class ControllerExtensions
{
    private static string SmtpServerRequiresAuth = "The SMTP server requires authentication.";
    private static string SmtpServerDoesNotSupportSsl = "The SMTP server does not support SSL.";
    private static string SmtpHostUnreachable = "The SMTP host {0} is not reachable.";

    /// <summary>
    /// Asynchronously sends an e-mail on behalf of the given controller using the specified parameters.
    /// </summary>
    /// <param name="controller">The controller that initiated the action.</param>
    /// <param name="model">An object used to create the message to send.</param>
    /// <param name="emailClient">An object used to send the e-mail.</param>
    /// <returns></returns>
    public static async Task<IActionResult> SendEmailAsync(this Controller controller, EmailModel model, IEmailClientService emailClient)
    {
        var config = emailClient.Configuration;
        try
        {
            var message = EmailClientService.CreateMessage(
                model.Subject,
                model.Body,
                model.From,
                model.To
            );

            await emailClient.SendEmailAsync(message);
            return controller.Ok();
        }
        catch (ServiceNotAuthenticatedException ex)
        {
            if (true == config?.RequiresAuth)
                return controller.BadRequest(new ServiceNotAuthenticatedException(SmtpServerRequiresAuth));
            return controller.BadRequest(ex);
        }
        catch (SslHandshakeException ex)
        {
            if (true == config?.UseSsl)
                return controller.BadRequest(new SslHandshakeException(SmtpServerDoesNotSupportSsl));
            return controller.BadRequest(ex);
        }
        catch (SocketException)
        {
            return controller.BadRequest(new Exception(string.Format(SmtpHostUnreachable, config?.Host)));
        }
        catch (Exception ex)
        {
            return controller.BadRequest(ex);
        }
    }
}
```
