using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Discore
{
    public class DiscordGuildVoiceChannel : DiscordGuildChannel
    {
        /// <summary>
        /// Gets the audio bitrate used for this channel.
        /// </summary>
        public int Bitrate { get; }

        /// <summary>
        /// Gets the maximum number of users that can be connected to this channel simultaneously.
        /// </summary>
        public int UserLimit { get; }

        /// <summary>
        /// Gets the ID of the parent category channel or null if the channel is not in a category.
        /// </summary>
        public Snowflake? ParentId { get; }

        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="name"/> or <paramref name="permissionOverwrites"/> is null.
        /// </exception>
        public DiscordGuildVoiceChannel(
            Snowflake id,
            string name,
            int position,
            IReadOnlyDictionary<Snowflake, DiscordOverwrite> permissionOverwrites,
            Snowflake guildId,
            int bitrate,
            int userLimit,
            Snowflake? parentId)
            : base(id,
                  DiscordChannelType.GuildVoice,
                  name,
                  position,
                  permissionOverwrites,
                  guildId)
        {
            Bitrate = bitrate;
            UserLimit = userLimit;
            ParentId = parentId;
        }

        internal DiscordGuildVoiceChannel(JsonElement json, Snowflake? guildId = null)
            : base(json, DiscordChannelType.GuildVoice, guildId)
        {
            Bitrate = json.GetProperty("bitrate").GetInt32();
            UserLimit = json.GetProperty("user_limit").GetInt32();
            ParentId = json.GetPropertyOrNull("parent_id")?.GetSnowflakeOrNull();
        }
    }
}
