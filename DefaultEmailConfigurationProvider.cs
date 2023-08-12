using System.Threading;
using System.Threading.Tasks;

namespace MailkitTools
{
    /// <summary>
    /// Represents the default email configuration provider.
    /// </summary>
    public class DefaultEmailConfigurationProvider : EmailConfigurationProviderBase
    {
        private readonly IEmailClientConfiguration _clientConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEmailConfigurationProvider"/> class.
        /// </summary>
        /// <param name="clientConfig">An email client configuration.</param>
        public DefaultEmailConfigurationProvider(IEmailClientConfiguration clientConfig)
        {
            _clientConfig = clientConfig;
        }

        /// <inheritdoc/>
        public override Task<IEmailClientConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
        {
            // normally, you would retrieve the settings from a (file or database) store;
            return Task.FromResult(_clientConfig);
        }
    }
}