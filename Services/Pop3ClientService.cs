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
        /// If true, uses the base implementation of <see cref="CreateIncomingMailClientAsync(RemoteCertificateValidationCallback, CancellationToken)"/>, 
        /// which uses the <see cref="ImapClient"/> by default. Otherwise, uses the <see cref="Pop3Client"/>.
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
        /// <param name="certificateValidator">A callback function to validate the server certificate.</param>
        /// <param name="cancellationToken">The token used to cancel an ongoing async operation.</param>
        /// <returns></returns>
        protected override Task<IMailService> CreateIncomingMailClientAsync(RemoteCertificateValidationCallback certificateValidator = null, CancellationToken cancellationToken = default)
          => UseImapClient ? 
            base.CreateIncomingMailClientAsync(cancellationToken: cancellationToken) : 
            new Pop3Client().ConnectAsync(Configuration, certificateValidator, cancellationToken);
    }
}
