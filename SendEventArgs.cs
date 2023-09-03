using System;
using System.Collections.Generic;
using MimeKit;

namespace MailkitTools
{
    /// <summary>
    /// Encapsulates data for events that do reporting during a send email operation.
    /// </summary>
    public class SendEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendEventArgs"/> class using the specified parameter.
        /// </summary>
        /// <param name="message">The message that was sent successfully.</param>
        public SendEventArgs(MimeMessage message)
        {
            Messages = new MimeMessage[] { message };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEventArgs"/> class using the specified parameter.
        /// </summary>
        /// <param name="messages">The messages that were sent successfully.</param>
        public SendEventArgs(IEnumerable<MimeMessage> messages)
        {
            Messages = messages;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEventArgs"/> class using the specified parameters.
        /// </summary>
        /// <param name="messages">The messages that couldn't be sent.</param>
        /// <param name="error">The error that occured.</param>
        public SendEventArgs(IEnumerable<MimeMessage> messages, Exception error)
        {
            Messages = messages;
            Error = error;
        }

        /// <summary>
        /// Gets the error that occured.
        /// </summary>
        public Exception? Error { get; }

        /// <summary>
        /// Gets the messages that were or weren't sent.
        /// </summary>
        public IEnumerable<MimeMessage> Messages { get; }
    }
}
