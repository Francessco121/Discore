using System.Text.Json;

namespace Discore
{
    public class DiscordWebhook : DiscordIdEntity
    {
        /// <summary> 
        /// Gets the ID of the guild this webhook belongs to.
        /// </summary> 
        public Snowflake? GuildId { get; }
        /// <summary> 
        /// Gets the ID of the channel this webhook is active for.
        /// </summary> 
        public Snowflake ChannelId { get; }
        /// <summary> 
        /// Gets the user that created this webhook.
        /// <para/>
        /// Will be null if this webhook was retrieved via its token.
        /// </summary> 
        public DiscordUser? User { get; }
        /// <summary> 
        /// Gets the default name of this webhook or null if no name is set.
        /// </summary> 
        public string? Name { get; }
        /// <summary> 
        /// Gets the default avatar of this webhook or null if no avatar is set.
        /// </summary> 
        public DiscordCdnUrl? Avatar { get; }
        /// <summary> 
        /// Gets the token of this webhook. 
        /// <para>This is only populated if the current bot created the webhook, otherwise it's empty/null.</para> 
        /// <para>It's used for executing, updating, and deleting this webhook without the need of authorization.</para> 
        /// </summary> 
        public string? Token { get; }
        /// <summary>
        /// Gets whether this webhook instance contains the webhook token.
        /// </summary>
        /// <seealso cref="Token"/>
        public bool HasToken => !string.IsNullOrWhiteSpace(Token);

        // TODO: add type, application_id

        internal DiscordWebhook(JsonElement json)
            : base(json)
        {
            GuildId = json.GetPropertyOrNull("guild_id")?.GetSnowflake();
            ChannelId = json.GetProperty("channel_id").GetSnowflake();
            Name = json.GetPropertyOrNull("name")?.GetString();
            Token = json.GetPropertyOrNull("token")?.GetString();

            JsonElement? userJson = json.GetPropertyOrNull("user");
            User = userJson == null ? null : new DiscordUser(userJson.Value, isWebhookUser: false);

            string? avatarHash = json.GetPropertyOrNull("avatar")?.GetString();
            Avatar = avatarHash == null ? null : DiscordCdnUrl.ForUserAvatar(Id, avatarHash);
        }
    }
}
