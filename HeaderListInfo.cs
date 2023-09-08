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
        /// The message's unique identifier.
        /// </summary>
        public uint UniqueId { get; set; }

        /// <summary>
        /// The message headers.
        /// </summary>
        public HeaderList? Headers { get; set; }

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.MessageId"/> header field.
        /// </summary>
        public string MessageId => Headers?[HeaderId.MessageId]?.TrimStart('<').TrimEnd('>') ?? string.Empty;

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.Date"/> header field.
        /// </summary>
        public string Date => Headers?[HeaderId.Date] ?? string.Empty;

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.From"/> header field.
        /// </summary>
        public string From => Headers?[HeaderId.From] ?? string.Empty;

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.To"/> header field.
        /// </summary>
        public string To => Headers?[HeaderId.To] ?? string.Empty;

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.Cc"/> header field.
        /// </summary>
        public string Cc => Headers?[HeaderId.Cc] ?? string.Empty;

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.Subject"/> header field.
        /// </summary>
        public string Subject => Headers?[HeaderId.Subject] ?? string.Empty;

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.ReturnPath"/> header field.
        /// </summary>
        public string ReturnPath => Headers?[HeaderId.ReturnPath] ?? string.Empty;

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.ContentType"/> header field.
        /// </summary>
        public string ContentType => Headers?[HeaderId.ContentType] ?? string.Empty;

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.ContentLanguage"/> header field.
        /// </summary>
        public string ContentLanguage => Headers?[HeaderId.ContentLanguage] ?? string.Empty;

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.MimeVersion"/> header field.
        /// </summary>
        public string MimeVersion => Headers?[HeaderId.MimeVersion] ?? string.Empty;

        /// <summary>
        /// Gets the value of the <see cref="HeaderId.XMailer"/> header field.
        /// </summary>
        public string XMailer => Headers?[HeaderId.XMailer] ?? string.Empty;

        /// <summary>
        /// Attempts to parse and return the <see cref="Date"/> 
        /// property value as a more user-friendly display.
        /// </summary>
        /// <param name="toLocalTime">true to convert the parsed date to the local time; otherwise, false.</param>
        /// <returns></returns>
        public string GetFriendlyDate(bool toLocalTime = false)
        {
            var d = NormalizedDate;
            if (toLocalTime)
                d = d.ToLocalTime();
            return d.Year != DateTime.Today.Year ? d.ToString("ddd, dd/MM/yyyy") : d.ToString("ddd, dd/MM");
        }

        /// <summary>
        /// Removes "useless" characters - such as (UTC), (CEST) - from the end of the <see cref="Date"/> string.
        /// </summary>
        public DateTimeOffset NormalizedDate
        {
            get
            {
                if (!_normalizedDate.HasValue)
                {
                    var s = Regex.Replace(Date, @"[()\sA-Z]+$", string.Empty, RegexOptions.Compiled);
                    if (DateTimeOffset.TryParse(s, out var result))
                        _normalizedDate = result;
                    else
                        _normalizedDate = DateTimeOffset.MinValue;
                }
                return _normalizedDate.Value;
            }
        }
        private DateTimeOffset? _normalizedDate;

        /// <summary>
        /// Extracts and returns the email address contained in the <see cref="From"/> property value.
        /// </summary>
        public string FromAddress
        {
            get
            {
                if (_fromAddress != null) return _fromAddress;
                (_toName, _toAddress) = ParseNameEmail(From);
                return _fromAddress ?? string.Empty;
            }
        }
        private string? _fromAddress;

        /// <summary>
        /// Extracts and returns the name contained in the <see cref="From"/> property value.
        /// </summary>
        public string FromName
        {
            get
            {
                if (_fromName != null) return _fromName;
                (_fromName, _fromAddress) = ParseNameEmail(From);
                return _fromName ?? string.Empty;
            }
        }
        private string? _fromName;

        /// <summary>
        /// Extracts and returns the email address contained in the <see cref="To"/> property value.
        /// </summary>
        public string ToAddress
        {
            get
            {
                if (_toAddress != null) return _toAddress;
                (_toName, _toAddress) = ParseNameEmail(To);
                return _toAddress ?? string.Empty;
            }
        }
        private string? _toAddress;

        /// <summary>
        /// Extracts and returns the name contained in the <see cref="To"/> property value.
        /// </summary>
        public string ToName
        {
            get
            {
                if (_toName != null) return _toName;
                (_toName, _toAddress) = ParseNameEmail(To);
                return _toName ?? string.Empty;
            }
        }
        private string? _toName;

        /// <summary>
        /// Returns the <see cref="FromName"/>, or <see cref="FromAddress"/> 
        /// property value (if <see cref="FromName"/> null or white space).
        /// </summary>
        public string FromNameOrAddress => string.IsNullOrWhiteSpace(FromName) ? FromAddress : FromName;

        private static (string? name, string? email) ParseNameEmail(string text)
        {
            // John Doe <john.doe@example.com>
            // <jane.fondue@example.com>
            // "Maurice Jackson" <mauricejackson@example.com>
            // user977@example.com
            var match = Regex.Match(text, @"(?<name>[^<]+)?<(?<email>[^>]+)|(?<email>.+)", RegexOptions.Compiled);
            if (match.Success)
            {
                var name = match.Groups["name"].Value.Trim(new char[] { ' ', '"' });
                var addr = match.Groups["email"].Value.Trim();

                return (name, addr);
            }

            return (null, null);
        }
    }
}