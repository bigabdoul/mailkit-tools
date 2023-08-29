using MimeKit;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MailkitTools.Services
{
    /// <summary>
    /// Defines the contract required for an object that provides pre-configured e-mail client services.
    /// </summary>
    public interface IConfiguredEmailService : IEmailClientService
    {
        /// <summary>
        /// Gets the last error that occured during an attempt to send messages.
        /// </summary>
        Exception LastError { get; }

        /// <summary>
        /// Asynchronously send the specified message.
        /// </summary>
        /// <param name="fromEmail">A comma- (or semi-colon) separated list of addresses in the 'From' header.</param>
        /// <param name="toEmail">A comma- (or semi-colon) separated list of addresses in the 'To' header.</param>
        /// <param name="subject">The subject of the message.</param>
        /// <param name="body">The body of the message.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns>true if the operation succeeds; otherwise, false.</returns>
        Task<bool> SendMessageAsync(string fromEmail, string toEmail, string subject, string body, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously send the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns>true if the operation succeeds; otherwise, false.</returns>
        Task<bool> SendMessageAsync(MimeMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously send the specified messages.
        /// </summary>
        /// <param name="messages">The collection of messages to send.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns>true if the operation succeeds; otherwise, false.</returns>
        Task<bool> SendMessageAsync(IEnumerable<MimeMessage> messages, CancellationToken cancellationToken = default);
    }
}
