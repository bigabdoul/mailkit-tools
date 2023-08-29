using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MailkitTools.Services
{
    /// <summary>
    /// Provides extension methods to Mailkit objects.
    /// </summary>
    public static class MailServiceExtensions
    {
        #region static & constant fields

        const string EMAIL_PATTERN = @"(?<email>([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3}))";

        /// <summary>
        /// Regular expression pattern that matches emails in the following format: "Abdul R. Kaba"&lt;abdulkaba@example.com>.
        /// Used to extract the 'From' name and email address parts from a string.
        /// </summary>
        const string NAME_EMAIL_PATTERN = @"""?(?<name>[\w\.\-'\s]*)""?\s*\<\s*" + EMAIL_PATTERN + @"\s*>";

        /// <summary>
        /// Default certificate validator that accepts all SSL certificates (in case the server supports STARTTLS)
        /// </summary>
        static readonly RemoteCertificateValidationCallback _certValidator = (s, c, h, e) => true;

        /// <summary>
        /// Characters used to separate a list of email addresses.
        /// </summary>
        static readonly char[] EmailSeparators = new char[] { ',', ';' };

        #endregion

        /// <summary>
        /// Asynchronously establish a connection to the specified mail server.
        /// </summary>
        /// <param name="client">The mail service client used to connect.</param>
        /// <param name="cfg">The configuration settings used to establish a connection.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <param name="certificateValidator">A callback function to validate the server certificate.</param>
        /// <returns></returns>
        public static async Task<IMailService> ConnectAsync(this IMailService client, IEmailClientConfiguration cfg, CancellationToken cancellationToken = default, RemoteCertificateValidationCallback certificateValidator = null)
        {
            client.ServerCertificateValidationCallback = certificateValidator ?? _certValidator;

            await client.ConnectAsync(cfg.Host, cfg.Port, cfg.UseSsl, cancellationToken);

            if (cfg.RequiresAuth)
                await client.AuthenticateAsync(cfg.UserName, cfg.Password);

            return client;
        }

        /// <summary>
        /// Asynchronously gets all message headers from the specified mail folder.
        /// </summary>
        /// <param name="folder">The <see cref="IMailFolder"/> to use.</param>
        /// <param name="headersReceived">
        /// A callback function to invoke each headers of a message are received. Returning true cancels the operation gracefully.
        /// </param>
        /// <param name="startIndex">The zero-based lower index at which to start fetching headers.</param>
        /// <param name="endIndex">
        /// The upper, exclusive index at which to stop fetching headers. Falls back to the number
        /// of available headers, if zero, negative or higher than the number of available headers.
        /// </param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns>A task that returns an integer representing the total number of messages in the inbox folder or the message spool.</returns>
        /// <exception cref="NotSupportedException">
        /// The incoming mail client is not supported, or <paramref name="progress"/> 
        /// is not null and the client is an instance of <see cref="IMailSpool"/>.
        /// </exception>
        /// <param name="progress">The progress reporting mechanism.</param>
        public static async Task<int> ReceiveHeadersAsync(this IMailFolder folder, Func<HeaderListInfo, Task<bool>> headersReceived, 
            int startIndex = 0, int endIndex = -1, CancellationToken cancellationToken = default, ITransferProgress progress = null)
        {
            if (!folder.IsOpen)
                folder.Open(FolderAccess.ReadOnly);

            var count = folder.Count; // the count may change as we download the headers, so dereference it

            if (startIndex < 0 || startIndex >= count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (endIndex <= 0 || endIndex >= count) endIndex = count;

            for (int i = startIndex; i < endIndex; i++)
            {
                var headers = await folder.GetHeadersAsync(i, cancellationToken, progress);
                if (await headersReceived(new HeaderListInfo { MessageIndex = i, MessageCount = count, Headers = headers }))
                    break;
            }

            // the number of headers fetched
            return endIndex - startIndex;
        }

        /// <summary>
        /// Asynchronously gets all message headers from the specified mail spool.
        /// </summary>
        /// <param name="spool">The <see cref="IMailSpool"/> to use.</param>
        /// <param name="headersReceived">
        /// A callback function to invoke each headers of a message are received. Returning true cancels the operation gracefully.
        /// </param>
        /// <param name="startIndex">The zero-based lower index at which to start fetching headers.</param>
        /// <param name="endIndex">
        /// The upper, exclusive index at which to stop fetching headers. Falls back to the number
        /// of available headers, if zero, negative or higher than the number of available headers.
        /// </param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns></returns>
        public static async Task<int> ReceiveHeadersAsync(this IMailSpool spool, Func<HeaderListInfo, Task<bool>> headersReceived, 
            int startIndex = 0, int endIndex = -1, CancellationToken cancellationToken = default)
        {
            var count = spool.Count; // the count may change as we download the headers, so dereference it

            if (startIndex < 0 || startIndex >= count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (endIndex <= 0 || endIndex >= count) endIndex = count;

            for (int i = startIndex; i < endIndex; i++)
            {
                var headers = await spool.GetMessageHeadersAsync(i, cancellationToken);
                if (await headersReceived(new HeaderListInfo { MessageIndex = i, MessageCount = count, Headers = headers }))
                    break;
            }

            // the number of headers fetched
            return endIndex - startIndex;
        }

        /// <summary>
        /// Asynchronously gets the specified message headers from the given folder.
        /// </summary>
        /// <param name="folder">The <see cref="IMailFolder"/> to use.</param>
        /// <param name="index">The index of the message.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <param name="progress">The progress reporting mechanism.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">
        /// The incoming mail client is not supported, or <paramref name="progress"/> 
        /// is not null and the client is an instance of <see cref="IMailSpool"/>.
        /// </exception>
        public static async Task<HeaderListInfo> ReceiveHeadersAsync(this IMailFolder folder, int index, CancellationToken cancellationToken = default, ITransferProgress progress = null)
        {
            if (!folder.IsOpen)
                folder.Open(FolderAccess.ReadOnly);

            var hdrs = await folder.GetHeadersAsync(index, cancellationToken, progress);
            return new HeaderListInfo { MessageIndex = index, MessageCount = folder.Count, Headers = hdrs };
        }

        /// <summary>
        /// Asynchronously retrieves all messages using the specified mailbox folder.
        /// </summary>
        /// <param name="folder">The mailbox folder used to retrieve the messages.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static async Task<IList<MimeMessage>> GetMessagesAsync(this IMailFolder folder, CancellationToken cancellationToken = default, ITransferProgress progress = null)
        {
            var list = new List<MimeMessage>();
            for (int i = 0; i < folder.Count; i++)
            {
                var message = await folder.GetMessageAsync(i, cancellationToken, progress);
                list.Add(message);
            }
            return list;
        }

        /// <summary>
        /// Asynchronously retrieve all messages from the specified spool.
        /// </summary>
        /// <param name="spool">An object that retrieves the messages from a mail spool.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static async Task<IList<MimeMessage>> GetMessagesAsync(this IMailSpool spool, CancellationToken cancellationToken = default, ITransferProgress progress = null)
        {
            var list = new List<MimeMessage>();
            for (int i = 0; i < spool.Count; i++)
            {
                var message = await spool.GetMessageAsync(i, cancellationToken, progress);
                list.Add(message);
            }
            return list;
        }

        /// <summary>
        /// Adds a collection of comma- (or semi-colon) separated email addresses to the specified list.
        /// </summary>
        /// <param name="list">A collection of email addresses.</param>
        /// <param name="addresses">A collection of comma- (or semi-colon) separated email addresses.</param>
        public static void AddRange(this InternetAddressList list, string addresses)
        {
            foreach (var addr in addresses.Split(EmailSeparators))
            {
                MailboxAddress mba;
                var match = Regex.Match(addr, NAME_EMAIL_PATTERN, RegexOptions.Compiled);
                if (match.Success)
                {
                    var name = match.Groups["name"].Value?.Trim();
                    var email = match.Groups["email"].Value;
                    mba = new MailboxAddress(name, email);
                }
                else
                {
                    mba = new MailboxAddress(addr);
                }
                list.Add(mba);
            }
        }

        /// <summary>
        /// Adds a collection of name/value pairs of email addresses to the specified list.
        /// </summary>
        /// <param name="list">A collection of email addresses.</param>
        /// <param name="addresses">A collection of name/value pairs of email addresses where the name represents an email address, and the value represents the full name of the user the address belongs to.</param>
        public static void AddRange(this InternetAddressList list, NameValueCollection addresses)
        {
            foreach (string addr in addresses.Keys)
            {
                list.Add(new MailboxAddress(addresses[addr], addr));
            }
        }

        /// <summary>
        /// Adds a collection of key/value pairs of email addresses to the specified list.
        /// </summary>
        /// <param name="list">A collection of email addresses.</param>
        /// <param name="addresses">A dictionary of key/value pairs of email addresses where key represents an email address, and value the full name of the user the address belongs to.</param>
        public static void AddRange(this InternetAddressList list, IDictionary<string, string> addresses)
        {
            foreach (string addr in addresses.Keys)
            {
                list.Add(new MailboxAddress(addresses[addr], addr));
            }
        }

        /// <summary>
        /// Asynchronously disconnects and disposes the specified mail service client.
        /// </summary>
        /// <param name="client">The mail service client to disconnect and dispose. Can be null.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task DisposeAsync(this IMailService client, CancellationToken cancellationToken = default)
        {
            await (client?.DisconnectAsync(true, cancellationToken) ?? Task.CompletedTask);
            (client as IDisposable)?.Dispose();
        }

        internal static async Task<IMailFolder> OpenFolderAsync(this IMailStore store, SpecialFolder? folder, 
            FolderAccess access = FolderAccess.ReadOnly, CancellationToken cancellation = default)
        {
            var ofolder = folder == null ? store.Inbox : store.GetFolder(folder.Value);
            await ofolder.OpenAsync(access, cancellation);
            return ofolder;
        }
    }
}
