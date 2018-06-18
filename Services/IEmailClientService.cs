using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;

namespace MailkitTools.Services
{
    /// <summary>
    /// Defines the contract required for an object that provides e-mail client services.
    /// </summary>
    public interface IEmailClientService
    {
        /// <summary>
        /// Gets or sets the email client configuration settings.
        /// </summary>
        IEmailClientConfiguration Configuration { get; set; }

        /// <summary>
        /// Asynchronously send the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns></returns>
        Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Asynchronously send the specified messages.
        /// </summary>
        /// <param name="messages">The collection of messages to send.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns></returns>
        Task SendAsync(IEnumerable<MimeMessage> messages, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Receive a collection of messages asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns></returns>
        Task<IList<MimeMessage>> ReceiveAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Event fired when an error occurs.
        /// </summary>
        event Func<SendEventArgs, Task> Error;

        /// <summary>
        /// Event fired when a message was successfully sent.
        /// </summary>
        event Func<SendEventArgs, Task> Success;
    }
}
