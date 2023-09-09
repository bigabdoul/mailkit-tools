namespace MailkitTools
{
    /// <summary>
    /// Contract required for an email client configuration.
    /// </summary>
    public interface IEmailClientConfiguration
    {
        /// <summary>
        /// The host machine (server) name or IP address.
        /// </summary>
        string? Host { get; set; }

        /// <summary>
        /// The server port to connect to.
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// Use secure connection?
        /// </summary>
        bool UseSsl { get; set; }

        /// <summary>
        /// The user name to connect with.
        /// </summary>
        string? UserName { get; set; }

        /// <summary>
        /// The user password to use for connection.
        /// </summary>
        string? Password { get; set; }

        /// <summary>
        /// Does the server require authentication?
        /// </summary>
        bool RequiresAuth { get; set; }

        /// <summary>
        /// Determines whether to remove the OAuth2 authentication mechanism.
        /// </summary>
        bool RemoveOAuth2 { get; set; }
    }
}
