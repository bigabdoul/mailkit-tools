using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

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
        Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously send the specified messages.
        /// </summary>
        /// <param name="messages">The collection of messages to send.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns></returns>
        Task SendAsync(IEnumerable<MimeMessage> messages, CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines the number of messages available on the server.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <param name="certificateValidator">A callback function to validate the server certificate. Can be null.</param>
        /// <returns></returns>
        Task<int> CountMessagesAsync(CancellationToken cancellationToken = default, RemoteCertificateValidationCallback certificateValidator = null);

        /// <summary>
        /// Asynchronously receives all of messages from the inbox or mail spool.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns></returns>
        Task<IList<MimeMessage>> ReceiveAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously receives all messages from the specified folder.
        /// </summary>
        /// <param name="folder">The special folder to use. If null, defaults to the inbox.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <param name="certificateValidator">A callback function to validate the server certificate. Can be null.</param>
        /// <param name="progress">The progress reporting mechanism.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">The incoming mail client is not supported.</exception>
        Task<IList<MimeMessage>> ReceiveAsync(SpecialFolder? folder, CancellationToken cancellationToken = default, RemoteCertificateValidationCallback certificateValidator = null, ITransferProgress progress = null);

        /// <summary>
        /// Asynchronously get all message headers.
        /// </summary>
        /// <param name="headersReceived">
        /// A callback function to invoke each headers of a message are received. Returning true cancels the operation gracefully.
        /// </param>
        /// <param name="folder">The special folder to use. If null, defaults to the inbox.</param>
        /// <param name="startIndex">The zero-based lower index at which to start fetching headers.</param>
        /// <param name="endIndex">
        /// The upper, exclusive index at which to stop fetching headers. Falls back to the number
        /// of available headers, if zero, negative or higher than the number of available headers.
        /// </param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <param name="certificateValidator">A callback function to validate the server certificate.</param>
        /// <param name="progress">The progress reporting mechanism.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">
        /// The incoming mail client is not supported, or <paramref name="progress"/> 
        /// is not null and the client is an instance of <see cref="IMailSpool"/>.
        /// </exception>
        Task<int> ReceiveHeadersAsync(Func<HeaderListInfo, Task<bool>> headersReceived, SpecialFolder? folder = null, int startIndex = 0, int endIndex = -1, CancellationToken cancellationToken = default, RemoteCertificateValidationCallback certificateValidator = null, ITransferProgress progress = null);

        /// <summary>
        /// Asynchronously get the specified message headers. If you intend to fetch the headers of more than one message, use the extension method 
        /// <see cref="MailServiceExtensions.ReceiveHeadersAsync(IMailFolder, int, CancellationToken, ITransferProgress)"/>
        /// after creating an instance of the <see cref="IMailService"/> client (see <see cref="CreateIncomingClientAsync(CancellationToken, RemoteCertificateValidationCallback)"/>.
        /// </summary>
        /// <param name="index">The index of the message.</param>
        /// <param name="folder">The special folder to use. If null, defaults to the inbox.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <param name="certificateValidator">A callback function to validate the server certificate.</param>
        /// <param name="progress">The progress reporting mechanism.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">
        /// The incoming mail client is not supported, or <paramref name="progress"/> 
        /// is not null and the client is an instance of <see cref="IMailSpool"/>.
        /// </exception>
        Task<HeaderListInfo> ReceiveHeadersAsync(int index, SpecialFolder? folder = null, CancellationToken cancellationToken = default, RemoteCertificateValidationCallback certificateValidator = null, ITransferProgress progress = null);

        /// <summary>
        /// Asynchronously creates and returns a connected instance of the <see cref="ImapClient"/> class.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <param name="certificateValidator">A callback function to validate the server certificate.</param>
        /// <returns></returns>
        Task<IMailService> CreateIncomingClientAsync(CancellationToken cancellationToken = default, RemoteCertificateValidationCallback certificateValidator = null);

        /// <summary>
        /// Asynchronously creates and returns a connected instance of the <see cref="SmtpClient"/> class.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <param name="certificateValidator">A callback function to validate the server certificate.</param>
        /// <returns></returns>
        Task<IMailTransport> CreateOutgoingClientAsync(CancellationToken cancellationToken = default, RemoteCertificateValidationCallback certificateValidator = null);

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
