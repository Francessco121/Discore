using System.Text.Json;

namespace Discore
{
    public class DiscordInviteChannel
    {
        /// <summary>
        /// Gets the ID of the channel this invite is for.
        /// </summary>
        public Snowflake Id { get; }

        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of channel.
        /// </summary>
        public DiscordChannelType Type { get; }

        internal DiscordInviteChannel(JsonElement json)
        {
            Id = json.GetProperty("id").GetSnowflake();
            Name = json.GetProperty("name").GetString()!;
            Type = (DiscordChannelType)json.GetProperty("type").GetInt32();
        }
    }
}
