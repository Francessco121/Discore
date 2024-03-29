using System.Text.Json;

namespace Discore
{
    /// <summary>
    /// A <see cref="DiscordDMChannel"/> or a <see cref="DiscordGuildChannel"/>.
    /// </summary>
    public class DiscordChannel : DiscordIdEntity
    {
        /// <summary>
        /// Gets the type of this channel.
        /// </summary>
        public DiscordChannelType ChannelType { get; }

        /// <summary>
        /// Gets whether this channel is a guild channel.
        /// </summary>
        public bool IsGuildChannel => 
               ChannelType == DiscordChannelType.GuildText
            || ChannelType == DiscordChannelType.GuildVoice
            || ChannelType == DiscordChannelType.GuildCategory
            || ChannelType == DiscordChannelType.GuildNews
            || ChannelType == DiscordChannelType.GuildStore;

        internal DiscordChannel(JsonElement json, DiscordChannelType type)
            : base(json)
        {
            ChannelType = type;
        }

        public override string ToString()
        {
            return $"{ChannelType} Channel: {Id}";
        }
    }
}
