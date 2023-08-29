using MimeKit;
using System;
using System.Text.RegularExpressions;

namespace MailkitTools
{
    /// <summary>
    /// Represents an object that encapsulates a MIME message index and its corresponding headers.
    /// </summary>
    public class HeaderListInfo //: IHeaderListInfo
    {
        /// <summary>
        /// The index of the message.
        /// </summary>
        public int MessageIndex { get; set; }

        /// <summary>
        /// The total number of messages in the mail folder or the message spool.
        /// </summary>
        public int MessageCount { get; set; }

        /// <summary>
        /// The message headers.
        /// </summary>
        public HeaderList Headers { get; set; }

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.MessageId"/> header field.
        /// </summary>
        public string MessageId { get => Headers[HeaderId.MessageId]; }

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.Date"/> header field.
        /// </summary>
        public string Date { get => Headers[HeaderId.Date]; }

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.From"/> header field.
        /// </summary>
        public string From { get => Headers[HeaderId.From]; }

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.To"/> header field.
        /// </summary>
        public string To { get => Headers[HeaderId.To]; }

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.Cc"/> header field.
        /// </summary>
        public string Cc { get => Headers[HeaderId.Cc]; }

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.Subject"/> header field.
        /// </summary>
        public string Subject { get => Headers[HeaderId.Subject]; }

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.ReturnPath"/> header field.
        /// </summary>
        public string ReturnPath { get => Headers[HeaderId.ReturnPath]; }

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.ContentType"/> header field.
        /// </summary>
        public string ContentType { get => Headers[HeaderId.ContentType]; }

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.ContentLanguage"/> header field.
        /// </summary>
        public string ContentLanguage { get => Headers[HeaderId.ContentLanguage]; }

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.MimeVersion"/> header field.
        /// </summary>
        public string MimeVersion { get => Headers[HeaderId.MimeVersion]; }

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.XMailer"/> header field.
        /// </summary>
        public string XMailer { get => Headers[HeaderId.XMailer]; }

        /// <summary>
        /// Attempts to parse and return the <see cref="Date"/> 
        /// property value as a more user-friendly display.
        /// </summary>
        /// <param name="toLocalTime">true to convert the parsed date to the local time; otherwise, false.</param>
        /// <returns></returns>
        public string ToFriendlyDate(bool toLocalTime = false)
        {
            if (DateTimeOffset.TryParse(Date, out var d))
            {
                if (toLocalTime)
                    d = d.ToLocalTime();
                return d.Year != DateTime.Today.Year ? d.ToString("ddd, dd/MM/yyyy") : d.ToString("ddd, dd/MM");
            }
            return Date;
        }

        /// <summary>
        /// Extracts and returns the email address contained in the <see cref="From"/> property value.
        /// </summary>
        public string FromAddress
        {
            get
            {
                if (_fromAddress != null) return _fromAddress;
                ParseAddressName();
                return _fromAddress;
            }
        }
        private string _fromAddress;

        /// <summary>
        /// Extracts and returns the name contained in the <see cref="From"/> property value.
        /// </summary>
        public string FromName
        {
            get
            {
                if (_fromName != null) return _fromName;
                ParseAddressName();
                return _fromName;
            }
        }
        private string _fromName;

        /// <summary>
        /// Returns the <see cref="FromName"/>, or <see cref="FromAddress"/> 
        /// property value (if <see cref="FromName"/> null or white space).
        /// </summary>
        public string FromNameOrAddress => string.IsNullOrWhiteSpace(FromName) ? FromAddress : FromName;

        private void ParseAddressName()
        {
            // John Doe <john.doe@example.com>
            // <jane.fondue@example.com>
            // "Maurice Jackson" <mauricejackson@example.com>
            // user977@example.com
            var match = Regex.Match(From, @"(?<name>[^<]+)?<(?<email>[^>]+)|(?<email>.+)", RegexOptions.Compiled);
            if (match.Success)
            {
                _fromName = match.Groups["name"].Value.Trim(new char[] { ' ', '"' });
                _fromAddress = match.Groups["email"].Value.Trim();
            }
        }
    }
}