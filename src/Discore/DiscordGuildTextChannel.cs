#nullable enable

using System.Collections.Generic;
using System.Text.Json;

namespace Discore
{
    public sealed class DiscordGuildTextChannel : DiscordGuildChannel, ITextChannel
    {
        /// <summary>
        /// Gets the topic of this channel.
        /// </summary>
        public string? Topic { get; }

        /// <summary>
        /// Gets whether this text channel is NSFW (not-safe-for-work).
        /// </summary>
        public bool Nsfw { get; }

        /// <summary>
        /// Gets the ID of the parent category channel or null if the channel is not in a category.
        /// </summary>
        public Snowflake? ParentId { get; }

        /// <summary>
        /// Gets the ID of the last message sent in this text channel.
        /// <para/>
        /// This ID is only up-to-date for when this text channel was first retrieved from the Discord API.
        /// It's very likely that this value is outdated.
        /// <para/>
        /// Use <see cref="Http.DiscordHttpClient.GetChannel{T}(Snowflake)"/> to get an up-to-date ID.
        /// </summary>
        public Snowflake? LastMessageId { get; }

        public DiscordGuildTextChannel(
            Snowflake id,
            string name,
            int position,
            IReadOnlyDictionary<Snowflake, DiscordOverwrite> permissionOverwrites,
            Snowflake guildId,
            string? topic,
            bool nsfw,
            Snowflake? parentId,
            Snowflake? lastMessageId)
            : base(id,
                  DiscordChannelType.GuildText,
                  name,
                  position,
                  permissionOverwrites,
                  guildId)
        {
            Topic = topic;
            Nsfw = nsfw;
            ParentId = parentId;
            LastMessageId = lastMessageId;
        }

        internal DiscordGuildTextChannel(JsonElement json, Snowflake? guildId = null)
            : base(json, DiscordChannelType.GuildText, guildId)
        {
            Topic = json.GetPropertyOrNull("topic")?.GetString();
            Nsfw = json.GetPropertyOrNull("nsfw")?.GetBoolean() ?? false;
            ParentId = json.GetPropertyOrNull("parent_id")?.GetSnowflakeOrNull();
            LastMessageId = json.GetPropertyOrNull("last_message_id")?.GetSnowflakeOrNull();
        }
    }
}

#nullable restore
