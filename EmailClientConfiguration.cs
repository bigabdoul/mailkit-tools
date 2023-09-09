namespace MailkitTools
{
    /// <summary>
    /// Implements the data contract required for an email client configuration.
    /// </summary>
    public class EmailClientConfiguration : IEmailClientConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailClientConfiguration"/> class.
        /// </summary>
        public EmailClientConfiguration()
        {
        }

        /// <summary>
        /// The host machine (server) name or IP address.
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// The server port to connect to.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Use secure connection?
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// The user name to connect with.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// The user password to use for connection.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Does the server require authentication?
        /// </summary>
        public bool RequiresAuth { get; set; }

        /// <inheritdoc/>
        public bool RemoveOAuth2 { get; set; }
    }
}
