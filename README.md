# mailkit-tools

MailkitTools provides e-mail services built on top of the popular MailKit library.
These services are ideal to use within any .NET Core project that requires e-mail
services, such as sending messages with an SMTP client and receiving e-mails with
a POP3 client.

### Getting started with the email sender:

An implementation of the `IEmailSender` interface is now included in the source.
Here's a simplified implementation of the `MailkitTools.IEmailConfigurationProvider`
interface:

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
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
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

In the appsettings.json configuration file:

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
