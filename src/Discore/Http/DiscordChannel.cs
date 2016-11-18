namespace Discore.Http
{
    /// <summary>
    /// A <see cref="DiscordDMChannel"/> or a <see cref="DiscordGuildChannel"/>.
    /// </summary>
    public abstract class DiscordChannel : DiscordIdObject
    {
        /// <summary>
        /// Gets the type of this channel.
        /// </summary>
        public DiscordChannelType ChannelType { get; }

        public DiscordChannel(DiscordApiData data, DiscordChannelType type)
            : base(data)
        {
            ChannelType = type;
        }
    }
}
