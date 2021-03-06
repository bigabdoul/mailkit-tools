using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;

namespace MailkitTools.Services
{
    /// <summary>
    /// Represents a POP3 e-mail client service provider.
    /// </summary>
    public class Pop3ClientService : EmailClientService, IPop3ClientService
    {
        /// <summary>
        /// If true, uses the base implementation of <see cref="CreateIncomingMailClientAsync(CancellationToken, RemoteCertificateValidationCallback)"/>, which uses the <see cref="ImapClient"/> by default.
        /// Otherwise, uses the <see cref="Pop3Client"/>.
        /// </summary>
        public bool UseImapClient { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pop3ClientService"/> class.
        /// </summary>
        public Pop3ClientService()
        {
        }

        /// <summary>
        /// Asynchronously creates and returns a connected instance of the <see cref="Pop3Client"/> class.
        /// </summary>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <param name="certificateValidator">A callback function to validate the server certificate.</param>
        /// <returns></returns>
        protected override async Task<IMailService> CreateIncomingMailClientAsync(CancellationToken cancellationToken = default(CancellationToken), RemoteCertificateValidationCallback certificateValidator = null)
          => UseImapClient ? 
            await base.CreateIncomingMailClientAsync(cancellationToken) : 
            await new Pop3Client().ConnectAsync(Configuration, cancellationToken, certificateValidator);
    }
}
