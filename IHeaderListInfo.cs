using MimeKit;

namespace MailkitTools
{
    internal interface IHeaderListInfo
    {
        string Cc { get; }
        string ContentLanguage { get; }
        string ContentType { get; }
        string Date { get; }
        string From { get; }
        HeaderList Headers { get; set; }
        int MessageCount { get; set; }
        string MessageId { get; }
        int MessageIndex { get; set; }
        string MimeVersion { get; }
        string ReturnPath { get; }
        string Subject { get; }
        string To { get; }
        string XMailer { get; }
    }
}