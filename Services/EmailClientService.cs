using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace MailkitTools.Services
{
    /// <summary>
    /// Represents an object that provides e-mail client services.
    /// </summary>
    public class EmailClientService : IEmailClientService
    {
        readonly object _configLock = new object();
        IEmailClientConfiguration _configuration;

        /// <summary>
        /// Event fired when an error occurs within the <see cref="SendAsync(IEnumerable{MimeMessage}, CancellationToken)"/> method.
        /// If no event handler is found, any exception will be rethrown.
        /// </summary>
        public event Func<SendEventArgs, Task> Error;

        /// <summary>
        /// Event fired when a message was successfully sent.
        /// </summary>
        public event Func<SendEventArgs, Task> Success;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailClientService"/> class.
        /// </summary>
        public EmailClientService()
        {
        }

        /// <summary>
        /// Gets or sets the email client configuration settings.
        /// </summary>
        public IEmailClientConfiguration Configuration
        {
            get { lock (_configLock) { return _configuration; } }
            set { lock (_configLock) { _configuration = value; } }
        }

        /// <summary>
        /// Sends the specified message asynchronously.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns></returns>
        public virtual Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
            => SendAsync(new MimeMessage[] { message }, cancellationToken);

        /// <summary>
        /// Asynchronously sends the specified messages.
        /// </summary>
        /// <param name="messages">The collection of messages to send.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns></returns>
        public virtual async Task SendAsync(IEnumerable<MimeMessage> messages, CancellationToken cancellationToken = default)
        {
            IMailTransport client = null;
            try
            {
                client = await CreateOutgoingMailClientAsync(cancellationToken);
                foreach (var message in messages)
                {
                    try
                    {
                        await client.SendAsync(message, cancellationToken);
                        await OnSuccessAsync(message);
                    }
                    catch (Exception ex)
                    {
                        if (Error != null)
                        {
                            await Error(new SendEventArgs(new MimeMessage[] { message }, ex));
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var err = Error;
                if (err != null)
                {
                    await err.Invoke(new SendEventArgs(messages, ex));
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                await client.DisposeAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Determines the number of messages available on the server.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <param name="certificateValidator">A callback function to validate the server certificate. Can be null.</param>
        /// <returns></returns>
        public async Task<int> CountMessagesAsync(CancellationToken cancellationToken = default, RemoteCertificateValidationCallback certificateValidator = null)
        {
            IMailService client = null;
            try
            {
                // create an incoming mail client and get the message count
                client = await CreateIncomingMailClientAsync(cancellationToken, certificateValidator);
                if (client is IMailStore store)
                {
                    await store.Inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);
                    return store.Inbox.Count;
                }
                else if (client is IMailSpool spool)
                {
                    return spool.Count;
                }
                else
                {
                    throw new NotSupportedException($"Client type '{client.GetType().FullName}' not supported.");
                }
            }
            finally
            {
                await client.DisposeAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<int> ReceiveHeadersAsync(Func<HeaderListInfo, Task<bool>> headersReceived, SpecialFolder? folder = null, 
            int startIndex = 0, int endIndex = -1, CancellationToken cancellationToken = default, 
            RemoteCertificateValidationCallback certificateValidator = null, ITransferProgress progress = null)
        {
            IMailService client = null;
            try
            {
                client = await CreateIncomingMailClientAsync(cancellationToken, certificateValidator);
                if (client is IMailStore store)
                {
                    var ofolder = folder == null ? store.Inbox : store.GetFolder(folder.Value);
                    await ofolder.OpenAsync(FolderAccess.ReadOnly, cancellationToken);
                    return await ofolder.ReceiveHeadersAsync(headersReceived, startIndex, endIndex, cancellationToken, progress);
                }
                else if (client is IMailSpool spool)
                {
                    if (progress != null)
                        MailSpoolProgressReportingNotSupported();
                    return await spool.ReceiveHeadersAsync(headersReceived, startIndex, endIndex, cancellationToken);
                }
                else
                {
                    throw new NotSupportedException($"Client type '{client.GetType().FullName}' not supported.");
                }
            }
            finally
            {
                await client.DisposeAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Asynchronously gets the specified message headers from the provided folder.
        /// </summary>
        /// <param name="index">The index of the message.</param>
        /// <param name="folder">The special folder to use. If null, defaults to the inbox.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">The incoming mail client is not supported.</exception>
        public virtual Task<HeaderListInfo> ReceiveHeadersAsync(int index, SpecialFolder? folder = null, CancellationToken cancellationToken = default)
            => ReceiveHeadersAsync(index, folder, cancellationToken, null, null);

        /// <inheritdoc/>
        public virtual async Task<HeaderListInfo> ReceiveHeadersAsync(int index, SpecialFolder? folder = null, CancellationToken cancellationToken = default, RemoteCertificateValidationCallback certificateValidator = null, ITransferProgress progress = null)
        {
            IMailService client = null;
            try
            {
                client = await CreateIncomingMailClientAsync(cancellationToken, certificateValidator);
                if (client is IMailStore store)
                {
                    IMailFolder ofolder = await store.OpenFolderAsync(folder, cancellation: cancellationToken);
                    return await ofolder.ReceiveHeadersAsync(index, cancellationToken, progress);
                }
                else if (client is IMailSpool spool)
                {
                    if (progress != null)
                        MailSpoolProgressReportingNotSupported();

                    var hdrs = await spool.GetMessageHeadersAsync(index, cancellationToken);
                    return new HeaderListInfo { MessageIndex = index, MessageCount = spool.Count, Headers = hdrs };
                }
                else
                {
                    throw new NotSupportedException($"Client type '{client.GetType().FullName}' not supported.");
                }
            }
            finally
            {
                await client.DisposeAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public virtual Task<IList<MimeMessage>> ReceiveAsync(CancellationToken cancellationToken = default)
            => ReceiveAsync(null, cancellationToken, null, null);

        /// <inheritdoc/>
        public virtual async Task<IList<MimeMessage>> ReceiveAsync(SpecialFolder? folder, CancellationToken cancellationToken = default, RemoteCertificateValidationCallback certificateValidator = null, ITransferProgress progress = null)
        {
            IList<MimeMessage> list = null;
            IMailService client = null;
            try
            {
                client = await CreateIncomingMailClientAsync(cancellationToken, certificateValidator);
                if (client is IMailStore store)
                {
                    var ofolder = folder == null ? store.Inbox : store.GetFolder(folder.Value);
                    await ofolder.OpenAsync(FolderAccess.ReadOnly, cancellationToken);
                    list = await ofolder.GetMessagesAsync(cancellationToken, progress);
                }
                else if (client is IMailSpool spool)
                {
                    list = await spool.GetMessagesAsync(cancellationToken, progress);
                }
                else
                {
                    throw new NotSupportedException($"Client type '{client.GetType().FullName}' not supported.");
                }
                return list;
            }
            finally
            {
                await client.DisposeAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public virtual Task<IMailService> CreateIncomingClientAsync(CancellationToken cancellationToken = default, RemoteCertificateValidationCallback certificateValidator = null)
            => CreateIncomingMailClientAsync(cancellationToken, certificateValidator);

        /// <inheritdoc/>
        public virtual Task<IMailTransport> CreateOutgoingClientAsync(CancellationToken cancellationToken = default, RemoteCertificateValidationCallback certificateValidator = null)
            => CreateOutgoingMailClientAsync(cancellationToken, certificateValidator);

        /// <summary>
        /// Asynchronously creates and returns a connected instance of the <see cref="ImapClient"/> class.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <param name="certificateValidator">A callback function to validate the server certificate.</param>
        /// <returns></returns>
        protected virtual Task<IMailService> CreateIncomingMailClientAsync(CancellationToken cancellationToken = default, RemoteCertificateValidationCallback certificateValidator = null)
            => new ImapClient().ConnectAsync(Configuration, cancellationToken, certificateValidator);

        /// <summary>
        /// Asynchronously creates and returns a connected instance of the <see cref="SmtpClient"/> class.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <param name="certificateValidator">A callback function to validate the server certificate.</param>
        /// <returns></returns>
        protected virtual async Task<IMailTransport> CreateOutgoingMailClientAsync(CancellationToken cancellationToken = default, RemoteCertificateValidationCallback certificateValidator = null)
          => (IMailTransport)await new SmtpClient().ConnectAsync(Configuration, cancellationToken, certificateValidator);

        #region static

        /// <summary>
        /// Creates and returns a new instance of the <see cref="MimeMessage"/> class using the specified parameters.
        /// </summary>
        /// <param name="subject">The subject of the message.</param>
        /// <param name="body">The body of the message.</param>
        /// <param name="from">A comma- (or semi-colon) separated list of addresses in the 'From' header.</param>
        /// <param name="to">A comma- (or semi-colon) separated list of addresses in the 'To' header.</param>
        /// <param name="bodyFormat">The text format of the message body.</param>
        /// <param name="messageId">The identifier of the message. Can be null.</param>
        /// <returns></returns>
        public static MimeMessage CreateMessage(string subject, string body, string from, string to, TextFormat bodyFormat = TextFormat.Html, string messageId = null)
        {
            var message = new MimeMessage
            {
                Subject = subject,
                Body = new TextPart(bodyFormat)
                {
                    Text = body
                },
                MessageId = string.IsNullOrWhiteSpace(messageId) ? MimeKit.Utils.MimeUtils.GenerateMessageId() : messageId,
            };

            message.From.AddRange(from);
            message.To.AddRange(to);

            return message;
        }

        /// <summary>
        /// Creates and returns a new instance of the <see cref="MimeMessage"/> class using the specified parameters, optionally adding attachments.
        /// </summary>
        /// <param name="subject">The subject of the message.</param>
        /// <param name="body">The body of the message.</param>
        /// <param name="from">A comma- (or semi-colon) separated list of addresses in the 'From' header.</param>
        /// <param name="to">A comma- (or semi-colon) separated list of addresses in the 'To' header.</param>
        /// <param name="attachments">An array of tuples to add as attachments to the message to create.</param>
        /// <returns></returns>
        public static MimeMessage CreateMessage(string subject, string body, string from, string to, params (string fileName, byte[] data, string contentType)[] attachments)
        {
            return CreateMessage(subject, body, from, to, TextFormat.Html, messageId: null, attachments);
        }

        /// <summary>
        /// Creates and returns a new instance of the <see cref="MimeMessage"/> class using the specified parameters, optionally adding attachments.
        /// </summary>
        /// <param name="subject">The subject of the message.</param>
        /// <param name="body">The body of the message.</param>
        /// <param name="from">A comma- (or semi-colon) separated list of addresses in the 'From' header.</param>
        /// <param name="to">A comma- (or semi-colon) separated list of addresses in the 'To' header.</param>
        /// <param name="bodyFormat">The text format of the message body.</param>
        /// <param name="attachments">An array of tuples to add as attachments to the message to create.</param>
        /// <returns></returns>
        public static MimeMessage CreateMessage(string subject, string body, string from, string to, TextFormat bodyFormat, params (string fileName, byte[] data, string contentType)[] attachments)
        {
            return CreateMessage(subject, body, from, to, bodyFormat, messageId: null, attachments);
        }

        /// <summary>
        /// Creates and returns a new instance of the <see cref="MimeMessage"/> class using the specified parameters, optionally adding attachments.
        /// </summary>
        /// <param name="subject">The subject of the message.</param>
        /// <param name="body">The body of the message.</param>
        /// <param name="from">A comma- (or semi-colon) separated list of addresses in the 'From' header.</param>
        /// <param name="to">A comma- (or semi-colon) separated list of addresses in the 'To' header.</param>
        /// <param name="bodyFormat">The text format of the message body.</param>
        /// <param name="messageId">The identifier of the message. Can be null.</param>
        /// <param name="attachments">An array of tuples to add as attachments to the message to create.</param>
        /// <returns></returns>
        public static MimeMessage CreateMessage(string subject, string body, string from, string to, TextFormat bodyFormat, string messageId, params (string fileName, byte[] data, string contentType)[] attachments)
        {
            var message = CreateMessage(subject, body, from, to, bodyFormat, messageId);
            AddAttachments(message, attachments);
            return message;
        }

        /// <summary>
        /// Adds attachments to the specified message.
        /// </summary>
        /// <param name="message">The message to add attachments to.</param>
        /// <param name="attachments">An array of tuples to add as attachments to the specified message.</param>
        public static MimeMessage AddAttachments(MimeMessage message, params (string fileName, byte[] data, string contentType)[] attachments)
        {
            if (attachments?.Length > 0)
            {
                var builder = new BodyBuilder();

                foreach (var (fileName, data, contentType) in attachments)
                {
                    builder.Attachments.Add(fileName, data, ContentType.Parse(contentType));
                }

                message.Body = builder.ToMessageBody();
            }

            return message;
        }

        #endregion

        /// <summary>
        /// Attempts to fire the <see cref="Success"/> event.
        /// </summary>
        /// <param name="message">The message that was sent.</param>
        /// <returns></returns>
        protected Task OnSuccessAsync(MimeMessage message)
        {
            return Success?.Invoke(new SendEventArgs(message)) ?? Task.CompletedTask;
        }

        private void MailSpoolProgressReportingNotSupported()
        {
            throw new NotSupportedException($"A client of type {typeof(IMailSpool).FullName} " +
                "does not support progress reporting while receiving headers.");
        }
    }
}
