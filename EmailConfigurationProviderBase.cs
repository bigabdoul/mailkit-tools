using System.Threading;
using System.Threading.Tasks;
using MailkitTools.Services;

namespace MailkitTools
{
    /// <summary>
    /// Enables retrieval of email configuration settings from a store.
    /// </summary>
    public abstract class EmailConfigurationProviderBase : IEmailConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailConfigurationProviderBase" /> class.
        /// </summary>
        protected EmailConfigurationProviderBase()
        {
        }

        /// <summary>
        /// Asynchronously retrieve an instance of a class that implements the <see cref="IEmailClientConfiguration"/> interface.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel a running task.</param>
        /// <returns>An initialized instance of a class that implements the <see cref="IEmailClientConfiguration" /> interface.</returns>
        public abstract Task<IEmailClientConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
    }
}