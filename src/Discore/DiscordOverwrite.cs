using System;

namespace Discore
{
    /// <summary>
    /// A permission overwrite for a <see cref="DiscordRole"/> or <see cref="DiscordGuildMember"/>.
    /// </summary>
    public sealed class DiscordOverwrite : DiscordIdEntity
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

        internal DiscordOverwrite(Snowflake channelId, DiscordApiData data)
            : base(data)
        {
            ChannelId = channelId;

            string typeStr = data.GetString("type");
            DiscordOverwriteType type;
            if (Enum.TryParse(typeStr, true, out type))
                Type = type;

            long allow = data.GetInt64("allow").Value;
            Allow = (DiscordPermission)allow;

            long deny = data.GetInt64("deny").Value;
            Deny = (DiscordPermission)deny;
        }

        public override string ToString()
        {
            return $"{Type} Overwrite: {Id}";
        }
    }
}
