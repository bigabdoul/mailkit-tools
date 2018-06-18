using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MimeKit;

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
        public static async Task<IMailService> ConnectAsync(this IMailService client, IEmailClientConfiguration cfg, CancellationToken cancellationToken = default(CancellationToken), RemoteCertificateValidationCallback certificateValidator = null)
        {
            client.ServerCertificateValidationCallback = certificateValidator ?? _certValidator;

            await client.ConnectAsync(cfg.Host, cfg.Port, cfg.UseSsl, cancellationToken);

            if (cfg.RequiresAuth)
                await client.AuthenticateAsync(cfg.UserName, cfg.Password);

            return client;
        }

        /// <summary>
        /// Asynchronously retrieve all messages using the specified mailbox folder.
        /// </summary>
        /// <param name="folder">The mailbox folder used to retrieve the messages.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns></returns>
        public static async Task<IList<MimeMessage>> GetMessagesAsync(this IMailFolder folder, CancellationToken cancellationToken = default(CancellationToken))
        {
            var list = new List<MimeMessage>();
            for (int i = 0; i < folder.Count; i++)
            {
                var message = await folder.GetMessageAsync(i, cancellationToken);
                list.Add(message);
            }
            return list;
        }

        /// <summary>
        /// Asynchronously retrieve all messages from the specified spool.
        /// </summary>
        /// <param name="spool">An object that retrieves the messages from a mail spool.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns></returns>
        public static async Task<IList<MimeMessage>> GetMessagesAsync(this IMailSpool spool, CancellationToken cancellationToken = default(CancellationToken))
        {
            var list = new List<MimeMessage>();
            for (int i = 0; i < spool.Count; i++)
            {
                var message = await spool.GetMessageAsync(i, cancellationToken);
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
    }
}
