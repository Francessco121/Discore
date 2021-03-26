namespace Discore
{
    public sealed class DiscordWebhook : DiscordIdEntity
    {
        /// <summary> 
        /// Gets the ID of the guild this webhook belongs to.
        /// </summary> 
        public Snowflake GuildId { get; }
        /// <summary> 
        /// Gets the ID of the channel this webhook is active for.
        /// </summary> 
        public Snowflake ChannelId { get; }
        /// <summary> 
        /// Gets the user that created this webhook.
        /// </summary> 
        public DiscordUser User { get; }
        /// <summary> 
        /// Gets the public name of this webhook.
        /// </summary> 
        public string Name { get; }
        /// <summary> 
        /// Gets the avatar of this webhook (or null if the webhook user has no avatar set).
        /// </summary> 
        public DiscordCdnUrl Avatar { get; }
        /// <summary> 
        /// Gets the token of this webhook. 
        /// <para>This is only populated if the current bot created the webhook, otherwise it's empty/null.</para> 
        /// <para>It's used for executing, updating, and deleting this webhook without the need of authorization.</para> 
        /// </summary> 
        public string Token { get; }
        /// <summary>
        /// Gets whether this webhook instance contains the webhook token.
        /// </summary>
        public bool HasToken => !string.IsNullOrWhiteSpace(Token);

        internal DiscordWebhook(DiscordApiData data)
            : base(data)
        {
            GuildId = data.GetSnowflake("guild_id").Value;
            ChannelId = data.GetSnowflake("channel_id").Value;

            DiscordApiData userData = data.Get("user");
            if (!userData.IsNull)
                User = new DiscordUser(false, userData);

            Name = data.GetString("name");
            Token = data.GetString("token");

            string avatarHash = data.GetString("avatar");
            if (avatarHash != null)
                Avatar = DiscordCdnUrl.ForUserAvatar(Id, avatarHash);
        }
    }
}
