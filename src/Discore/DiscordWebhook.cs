namespace Discore
{
    public sealed class DiscordWebhook : DiscordIdObject
    {
        /// <summary> 
        /// The Id of Guild this Webhook belongs to 
        /// </summary> 
        public Snowflake Guild { get; }
        /// <summary> 
        /// The Id of the Channel this Webhook was created on 
        /// </summary> 
        public Snowflake Channel { get; }
        /// <summary> 
        /// The User that created this Webhook 
        /// </summary> 
        public DiscordUser User { get; }
        /// <summary> 
        /// The public name of this Webhook 
        /// </summary> 
        public string Name { get; }
        /// <summary> 
        /// The Avatar of this Webhook 
        /// </summary> 
        public DiscordAvatarData Avatar { get; }
        /// <summary> 
        /// The token of this Webhook<para/> 
        /// This is only populated if the we created the webhook, otherwise its empty/null<para/> 
        /// Its used for Executing, Updating, and Deleting said webhook without the need of authorization<para/> 
        /// We as a bot user can't modify or execute a webhook that we don't own 
        /// </summary> 
        public string Token { get; }
        public bool HasToken { get { return !string.IsNullOrWhiteSpace(Token); } }

        internal DiscordWebhook(DiscordApiData data)
            :base(data)
        {
            Guild = data.GetSnowflake("guild_id").Value;
            Channel = data.GetSnowflake("channel_id").Value;

            DiscordApiData userData = data.Get("user");
            if (!userData.IsNull)
                User = new DiscordUser(userData);

            Name = data.GetString("name");
            Avatar = new DiscordAvatarData(data.GetString("avatar"));
            Token = data.GetString("token");
        }
    }
}
