using System.Text.Json;

namespace Discore
{
    public class DiscordGuildVoiceChannel : DiscordGuildChannel, ITextChannel
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
        /// Gets whether this voice channel is NSFW (not-safe-for-work).
        /// </summary>
        public bool Nsfw { get; }

        /// <summary>
        /// Gets the ID of the parent category channel or null if the channel is not in a category.
        /// </summary>
        public Snowflake? ParentId { get; }

        /// <summary>
        /// Gets the ID of the last message sent in this voice channel.
        /// <para/>
        /// This ID is only up-to-date for when this voice channel was first retrieved from the Discord API.
        /// It's very likely that this value is outdated.
        /// <para/>
        /// Use <see cref="Http.DiscordHttpClient.GetChannel{T}(Snowflake)"/> to get an up-to-date ID.
        /// </summary>
        public Snowflake? LastMessageId { get; }

        internal DiscordGuildVoiceChannel(JsonElement json, Snowflake? guildId = null)
            : base(json, DiscordChannelType.GuildVoice, guildId)
        {
            Bitrate = json.GetProperty("bitrate").GetInt32();
            UserLimit = json.GetProperty("user_limit").GetInt32();
            Nsfw = json.GetPropertyOrNull("nsfw")?.GetBoolean() ?? false;
            ParentId = json.GetPropertyOrNull("parent_id")?.GetSnowflakeOrNull();
            LastMessageId = json.GetPropertyOrNull("last_message_id")?.GetSnowflakeOrNull();
        }
    }
}
