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

            string type = data.GetString("type");
            if (type == "text")
                Type = DiscordGuildChannelType.Text;
            else if (type == "voice")
                Type = DiscordGuildChannelType.Voice;
        }
    }
}
