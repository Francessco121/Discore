namespace Discore
{
    /// <summary>
    /// DM Channels represent a one-to-one conversation between two users, outside of the scope of guilds.
    /// </summary>
    public class DiscordDMChannel : DiscordChannel
    {
        /// <summary>
        /// Gets whether or not this channel is private.
        /// </summary>
        public bool IsPrivate { get; private set; }
        /// <summary>
        /// Gets the <see cref="DiscordUser"/> on the other end of this <see cref="DiscordDMChannel"/>.
        /// </summary>
        public DiscordUser Recipient { get; private set; }
        /// <summary>
        /// Gets the id of the last message sent in this channel.
        /// </summary>
        public string LastMessageId { get; private set; }

        /// <summary>
        /// Creates a new <see cref="DiscordDMChannel"/> instance.
        /// </summary>
        /// <param name="client">The associated <see cref="IDiscordClient"/>.</param>
        public DiscordDMChannel(IDiscordClient client) 
            : base(client, DiscordChannelType.DirectMessage)
        { }

        /// <summary>
        /// Updates this channel with the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this channel with.</param>
        public override void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            IsPrivate = data.GetBoolean("is_private") ?? IsPrivate;
            LastMessageId = data.GetString("last_message_id") ?? LastMessageId;

            DiscordApiData recipientData = data.Get("recipient");
            if (recipientData != null)
            {
                string userId = recipientData.GetString("id");
                Cache.AddOrUpdate(userId, recipientData, () => { return new DiscordUser(); });
            }
        }

        #region Object Overrides
        /// <summary>
        /// Determines whether the specified <see cref="DiscordDMChannel"/> is equal 
        /// to the current channel.
        /// </summary>
        /// <param name="other">The other <see cref="DiscordDMChannel"/> to check.</param>
        public bool Equals(DiscordDMChannel other)
        {
            return Id == other.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current channel.
        /// </summary>
        /// <param name="obj">The other object to check.</param>
        public override bool Equals(object obj)
        {
            DiscordDMChannel other = obj as DiscordDMChannel;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Equals(other);
        }

        /// <summary>
        /// Returns the hash of this channel.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns the name of the recipient of this dm channel.
        /// </summary>
        public override string ToString()
        {
            return Recipient.Username;
        }

#pragma warning disable 1591
        public static bool operator ==(DiscordDMChannel a, DiscordDMChannel b)
        {
            return a.Id == b.Id;
        }

        public static bool operator !=(DiscordDMChannel a, DiscordDMChannel b)
        {
            return a.Id != b.Id;
        }
#pragma warning restore 1591
        #endregion
    }
}
