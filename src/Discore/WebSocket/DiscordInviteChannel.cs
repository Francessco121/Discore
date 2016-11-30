namespace Discore.WebSocket
{
    public class DiscordInviteChannel : DiscordObject
    {
        /// <summary>
        /// Gets the channel this invite is for.
        /// </summary>
        public DiscordGuildChannel Channel { get; private set; }

        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type of channel.
        /// </summary>
        public DiscordGuildChannelType Type { get; private set; }

        Shard shard;

        internal DiscordInviteChannel(Shard shard)
        {
            this.shard = shard;
        }

        internal override void Update(DiscordApiData data)
        {
            Snowflake id = data.GetSnowflake("id").Value;
            Channel = shard.Channels.Get(id) as DiscordGuildChannel;

            Name = data.GetString("name");

            string type = data.GetString("type");
            if (type == "text")
                Type = DiscordGuildChannelType.Text;
            else if (type == "voice")
                Type = DiscordGuildChannelType.Voice;
        }
    }
}
