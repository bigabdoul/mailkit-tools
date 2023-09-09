using MailKit;
using System;

namespace MailkitTools
{
    /// <summary>
    /// Provides information about an <see cref="IMailFolder"/>.
    /// </summary>
    public class MailFolderInfo : IComparable<MailFolderInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MailFolderInfo"/> class.
        /// </summary>
        public MailFolderInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MailFolderInfo"/> class using the specified parameters.
        /// </summary>
        /// <param name="name">The name of the folder.</param>
        /// <param name="count">The total number of messages in the folder.</param>
        /// <param name="unread">The number of unread messages in the folder.</param>
        /// <param name="recent">The number of recently delivered messages in the folder.</param>
        public MailFolderInfo(string name, int count, int unread, int recent)
        {
            Name = name;
            Count = count;
            Unread = unread;
            Recent = recent;
        }

        /// <summary>
        /// Gets the folder name.
        /// </summary>
        public string Name { get; } = default!;

        /// <summary>
        /// Gets the total number of messages in the folder.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets the number of unread messages in the folder.
        /// </summary>
        public int Unread { get; }

        /// <summary>
        /// Gets the number of recently delivered messages in the folder.
        /// </summary>
        public int Recent { get; }

        /// <summary>
        /// Gets or sets the folder's display name.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the ordinal (display) number of the folder.
        /// </summary>
        public int Ordinal { get; set; }

        /// <inheritdoc/>
        public int CompareTo(MailFolderInfo? other)
        {
            if (other == null) return 1;
            return Ordinal.CompareTo(other.Ordinal);
        }

        /// <summary>
        /// Returns the name of the folder.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;
    }
}