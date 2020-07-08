# mailkit-tools

MailkitTools provides e-mail services built on top of the popular MailKit library.
These services are ideal to use within any .NET Core project that requires e-mail
services, such as sending messages with an SMTP client and receiving e-mails with
a POP3 client.

### Getting started with an email sender:

```C#
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Specifies the data contract required for a service that sends e-mails.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Asynchronously send an e-mail using the specified parameters.
    /// </summary>
    /// <param name="subject">The subject of the message.</param>
    /// <param name="body">The body of the message.</param>
    /// <param name="from">A comma- (or semi-colon) separated list of addresses in the 'From' header.</param>
    /// <param name="to">A comma- (or semi-colon) separated list of addresses in the 'To' header.</param>
    /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
    /// <returns></returns>
    Task SendEmailAsync(string subject, string body, string from, string to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send the specified message asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
    /// <returns></returns>
    Task SendEmailAsync(MimeKit.MimeMessage message, CancellationToken cancellationToken = default);
}
```

An implementation of `IEmailSender`:

```C#
using MailkitTools;
using MailkitTools.Services;
using MimeKit;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Sends emails.
/// </summary>
public class EmailSender : IEmailSender
{
    private bool _initialized;
    private IEmailConfigurationProvider _configProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSender"/> class.
    /// </summary>
    /// <param name="emailClient">An object used to send emails.</param>
    /// <param name="configProvider">An object used to retrieve email configuration settings.</param>
    public EmailSender(IEmailClientService emailClient, IEmailConfigurationProvider configProvider)
    {
        Client = emailClient;
        _configProvider = configProvider;
    }

    /// <summary>
    /// Gets the e-mail client used to send messages.
    /// </summary>
    protected IEmailClientService Client { get; }

    /// <summary>
    /// Performs internal one-time initializations.
    /// </summary>
    /// <returns></returns>
    protected virtual async Task InitAsync()
    {
        if (_initialized) return;
        Client.Configuration = await _configProvider.GetConfigurationAsync();
        _initialized = true;
    }

    /// <summary>
    /// Updates the configuration settings used to connect to the underlying <see cref="IEmailClientService"/>.
    /// </summary>
    /// <param name="config">The new configuration to set.</param>
    public virtual void ChangeConfiguration(IEmailClientConfiguration config)
    {
        Client.Configuration = config;
    }

    /// <summary>
    /// Asynchronously sends an e-mail using the specified parameters.
    /// </summary>
    /// <param name="subject">The subject of the message.</param>
    /// <param name="body">The body of the message.</param>
    /// <param name="from">A comma- (or semi-colon) separated list of addresses in the 'From' header.</param>
    /// <param name="to">A comma- (or semi-colon) separated list of addresses in the 'To' header.</param>
    /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
    /// <returns></returns>
    public Task SendEmailAsync(string subject, string body, string from, string to, CancellationToken cancellationToken = default)
        => SendEmailAsync(EmailClientService.CreateMessage(subject, body, from, to), cancellationToken);

    /// <summary>
    /// Send the specified message asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
    /// <returns></returns>
    public virtual async Task SendEmailAsync(MimeMessage message, CancellationToken cancellationToken = default)
    {
        await InitAsync();
        await Client.SendAsync(message, cancellationToken);
    }
}
```

An implementation of `MailkitTools.IEmailConfigurationProvider`:

```C#
using MailkitTools;
using MailkitTools.Services;

public class EmailConfigurationProvider : IEmailConfigurationProvider
{
    public Task<IEmailClientConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        // normally, you would retrieve the settings from a (file or database) store;
        return Task.Run(() => new EmailClientConfiguration
        {
            Host = "smtp.example.com", // replace with your SMTP server address
            Port = 25, // use 465 (or 587, or whatever is appropriate for you) for a secure SMTP port
            UseSsl = false, // use true if you're using Secure Sockets Layer protocol
            UserName = "user.name@example.com", // replace with a valid user account name
            Password = "password", // adjust appropriately
            RequiresAuth = true, // set appropriately
        });
    }
}

public class EmailClientConfiguration : IEmailClientConfiguration
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public bool RequiresAuth { get; set; }
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
        services.AddTransient<IEmailSender, EmailSender>();
        // ...
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

You can even reinforce reusability using an extension method:

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
