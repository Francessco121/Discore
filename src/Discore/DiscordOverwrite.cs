using System;
using System.Text.Json;

#nullable enable

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

        public DiscordOverwrite(
            Snowflake id,
            Snowflake channelId, 
            DiscordOverwriteType type, 
            DiscordPermission allow, 
            DiscordPermission deny)
            : base(id)
        {
            ChannelId = channelId;
            Type = type;
            Allow = allow;
            Deny = deny;
        }

        internal DiscordOverwrite(JsonElement json, Snowflake channelId)
            : base(json)
        {
            ChannelId = channelId;
            Allow = (DiscordPermission)json.GetProperty("allow").GetUInt64();
            Deny = (DiscordPermission)json.GetProperty("deny").GetUInt64();

            DiscordOverwriteType type;
            if (Enum.TryParse(json.GetProperty("type").GetString()!, out type))
                Type = type;
        }

        public override string ToString()
        {
            return $"{Type} Overwrite: {Id}";
        }
    }
}

#nullable restore
