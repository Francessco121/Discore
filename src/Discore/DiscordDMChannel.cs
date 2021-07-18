using System.Text.Json;

namespace Discore
{
    /// <summary>
    /// Direct message channels represent a one-to-one conversation between two users, outside of the scope of guilds.
    /// </summary>
    public class DiscordDMChannel : DiscordChannel, ITextChannel
    {
        /// <summary>
        /// Gets the user on the other end of this channel.
        /// </summary>
        public DiscordUser Recipient { get; }
        /// <summary>
        /// Gets the ID of the last message sent in this text channel.
        /// <para/>
        /// This ID is only up-to-date for when this text channel was first retrieved from the Discord API.
        /// It's very likely that this value is outdated.
        /// <para/>
        /// Use <see cref="Http.DiscordHttpClient.GetChannel{T}(Snowflake)"/> to get an up-to-date ID.
        /// </summary>
        public Snowflake? LastMessageId { get; }

        internal DiscordDMChannel(JsonElement json)
            : base(json, DiscordChannelType.DirectMessage)
        {
            LastMessageId = json.GetPropertyOrNull("last_message_id")?.GetSnowflakeOrNull();

            // Normal DM should only ever have exactly one recipient
            JsonElement recipientJson = json.GetProperty("recipients");
            Recipient = new DiscordUser(recipientJson[0], isWebhookUser: false);
        }

        public override string ToString()
        {
            return $"DM Channel: {Recipient}";
        }
    }
}
