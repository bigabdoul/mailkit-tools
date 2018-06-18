using System.Threading;
using System.Threading.Tasks;

namespace MailkitTools.Services
{
    /// <summary>
    /// Defines the contract required for retrieving a stored <see cref="IEmailClientConfiguration"/>.
    /// </summary>
    public interface IEmailConfigurationProvider
    {
        /// <summary>
        /// Asynchronously retrieve an instance of a class that implements the <see cref="IEmailClientConfiguration"/> interface.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel a running task.</param>
        /// <returns></returns>
        Task<IEmailClientConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
