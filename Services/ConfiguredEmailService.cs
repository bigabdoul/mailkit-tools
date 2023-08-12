using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Security;
using MimeKit;

namespace MailkitTools.Services
{
    /// <summary>
    /// Represents an email client service that supports a pre-configured email client 
    /// configuration and reports the last exception that occured while sending messages.
    /// </summary>
    public class ConfiguredEmailService : EmailClientService, IConfiguredEmailService
    {
        #region fields

        private Exception _lastError;

        #endregion

        #region constants

        private const string SmtpServerRequiresAuth = "The SMTP server requires authentication.";
        private const string SmtpServerDoesNotSupportSsl = "The SMTP server does not support SSL.";
        private const string SmtpHostUnreachable = "The SMTP host {0} is not reachable.";

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfiguredEmailService"/> class.
        /// </summary>
        /// <param name="configuration">The email client configuration to use.</param>
        public ConfiguredEmailService(IEmailClientConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        #region properties

        /// <inheritdoc/>
        public Exception LastError => _lastError;

        #endregion

        #region methods

        /// <inheritdoc/>
        public virtual Task<bool> SendMessageAsync(string fromEmail, string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            var msg = CreateMessage(subject, body, fromEmail, toEmail);
            return SendMessageAsync(msg, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual Task<bool> SendMessageAsync(MimeMessage message, CancellationToken cancellationToken = default)
                => SendMessageAsync(new MimeMessage[] { message }, cancellationToken);

        /// <inheritdoc/>
        public virtual async Task<bool> SendMessageAsync(IEnumerable<MimeMessage> messages, CancellationToken cancellationToken = default)
        {
            _lastError = null;
            try
            {
                await SendAsync(messages, cancellationToken);
                return true;
            }
            catch (ServiceNotAuthenticatedException ex)
            {
                if (Configuration.RequiresAuth)
                    _lastError = new ServiceNotAuthenticatedException(SmtpServerRequiresAuth);
                else
                    _lastError = ex;
            }
            catch (SslHandshakeException ex)
            {
                if (Configuration.UseSsl)
                    _lastError = new SslHandshakeException(SmtpServerDoesNotSupportSsl);
                else
                    _lastError = ex;
            }
            catch (SocketException)
            {
                _lastError = new Exception(string.Format(SmtpHostUnreachable, Configuration.Host));
            }
            catch (Exception ex)
            {
                _lastError = ex;
            }

            return false;
        }

        #endregion
    }
}
