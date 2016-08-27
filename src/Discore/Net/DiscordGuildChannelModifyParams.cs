namespace Discore.Net
{
    /// <summary>
    /// Changes to be made to an existing <see cref="DiscordGuildChannel"/>.
    /// </summary>
    public class DiscordGuildChannelModifyParams
    {
        /// <summary>
        /// Gets or sets the new name of the channel.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the new position of the channel.
        /// </summary>
        public int Position { get; set; }
        /// <summary>
        /// Gets or sets the new topic of the text channel.
        /// </summary>
        public string Topic { get; set; }
        /// <summary>
        /// Gets or sets the new bitrate of the voice channel.
        /// </summary>
        public int Bitrate { get; set; }
        /// <summary>
        /// Gets or sets the new user limit of the voice channel.
        /// </summary>
        public int UserLimit { get; set; }
        /// <summary>
        /// Gets or sets the type of channel.
        /// </summary>
        public DiscordGuildChannelType Type { get; set; }

        /// <summary>
        /// Creates a new <see cref="DiscordGuildChannelModifyParams"/> instance.
        /// </summary>
        /// <param name="type">The type of <see cref="DiscordGuildChannel"/></param>
        public DiscordGuildChannelModifyParams(DiscordGuildChannelType type)
        {
            Type = type;
        }

        /// <summary>
        /// Creates a new <see cref="DiscordGuildChannelModifyParams"/> instance.
        /// </summary>
        /// <param name="existingChannel">An existing <see cref="DiscordGuildChannel"/> to copy settings from.</param>
        public DiscordGuildChannelModifyParams(DiscordGuildChannel existingChannel)
        {
            Name = existingChannel.Name;
            Position = existingChannel.Position;
            Topic = existingChannel.Topic;
            Bitrate = existingChannel.Bitrate;
            UserLimit = existingChannel.UserLimit;
            Type = existingChannel.GuildChannelType;
        }

        /// <summary>
        /// Creates a new <see cref="DiscordGuildChannelModifyParams"/> instance for a text channel.
        /// </summary>
        /// <param name="name">The new name of the text channel.</param>
        /// <param name="position">The new position of the text channel</param>
        /// <param name="topic">The new topic of the text channel.</param>
        public DiscordGuildChannelModifyParams(string name, int position, string topic)
        {
            Name = name;
            Position = position;
            Topic = topic;
            Type = DiscordGuildChannelType.Text;
        }

        /// <summary>
        /// Creates a new <see cref="DiscordGuildChannelModifyParams"/> instance for a voice channel.
        /// </summary>
        /// <param name="name">The new name of the voice channel.</param>
        /// <param name="position">The new position of the voice channel.</param>
        /// <param name="bitrate">The new bitrate of the voice channel.</param>
        /// <param name="userLimit">The new user limit of the voice channel.</param>
        public DiscordGuildChannelModifyParams(string name, int position, int bitrate, int userLimit)
        {
            Name = name;
            Position = position;
            Bitrate = bitrate;
            UserLimit = userLimit;
            Type = DiscordGuildChannelType.Voice;
        }
    }
}
