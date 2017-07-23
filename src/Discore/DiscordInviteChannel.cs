namespace Discore
{
    public sealed class DiscordInviteChannel
    {
        /// <summary>
        /// Gets the ID of the channel this invite is for.
        /// </summary>
        public Snowflake ChannelId { get; }

        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of channel.
        /// </summary>
        public DiscordGuildChannelType Type { get; }

        internal DiscordInviteChannel(DiscordApiData data)
        {
            ChannelId = data.GetSnowflake("id").Value;

            Name = data.GetString("name");

            // TODO: Support all channel types

            InternalChannelType type = (InternalChannelType)data.GetInteger("type");
            if (type == InternalChannelType.GuildText)
                Type = DiscordGuildChannelType.Text;
            else if (type == InternalChannelType.GuildVoice)
                Type = DiscordGuildChannelType.Voice;
        }
    }
}
