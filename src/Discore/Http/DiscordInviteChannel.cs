namespace Discore.Http
{
    public class DiscordInviteChannel
    {
        /// <summary>
        /// Gets the id of the channel this invite is for.
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

        public DiscordInviteChannel(DiscordApiData data)
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
