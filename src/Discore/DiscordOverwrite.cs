using System.Text.Json;

namespace Discore
{
    /// <summary>
    /// A permission overwrite for a <see cref="DiscordRole"/> or <see cref="DiscordGuildMember"/>.
    /// </summary>
    public class DiscordOverwrite : DiscordIdEntity
    {
        public Snowflake ChannelId { get; }

        /// <summary>
        /// The type of this overwrite.
        /// </summary>
        public DiscordOverwriteType Type { get; }
        /// <summary>
        /// The specifically allowed permissions specified by this overwrite.
        /// </summary>
        public DiscordPermission Allow { get; }
        /// <summary>
        /// The specifically denied permissions specified by this overwrite.
        /// </summary>
        public DiscordPermission Deny { get; }

        internal DiscordOverwrite(JsonElement json, Snowflake channelId)
            : base(json)
        {
            ChannelId = channelId;
            Type = (DiscordOverwriteType)json.GetProperty("type").GetInt32();
            Allow = (DiscordPermission)json.GetProperty("allow").GetStringUInt64();
            Deny = (DiscordPermission)json.GetProperty("deny").GetStringUInt64();
        }

        public override string ToString()
        {
            return $"{Type} Overwrite: {Id}";
        }
    }
}
