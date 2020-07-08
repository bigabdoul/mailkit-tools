using System;
using System.Threading;
using System.Threading.Tasks;
using MailkitTools.Services;
using MimeKit;

namespace MailkitTools
{
    /// <summary>
    /// Provides support for sending emails.
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
            Client = emailClient ?? throw new ArgumentNullException(nameof(emailClient));
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
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
        /// Updates the configuration settings used to connect with the underlying <see cref="IEmailClientService"/>.
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
        public virtual Task SendEmailAsync(string subject, string body, string from, string to, CancellationToken cancellationToken = default)
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
}