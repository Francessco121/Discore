using System.Text.Json;

#nullable enable

namespace Discore
{
    /// <summary>
    /// A <see cref="DiscordDMChannel"/> or a <see cref="DiscordGuildChannel"/>.
    /// </summary>
    public abstract class DiscordChannel : DiscordIdEntity
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

        protected DiscordChannel(Snowflake id, DiscordChannelType type)
            : base(id)
        {
            ChannelType = type;
        }

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

#nullable restore
