namespace Discore.Http
{
    /// <summary>
    /// A set of parameters for modifying a guild integration.
    /// </summary>
    public class ModifyIntegrationParameters
    {
        /// <summary>
        /// Gets or sets the behavior to follow when the integration subscription lapses.
        /// </summary>
        public int ExpireBehavior { get; set; }
        /// <summary>
        /// Gets or sets the period (in seconds) where the integration will ignore lapsed subscriptions.
        /// </summary>
        public int ExpireGracePeriod { get; set; }
        /// <summary>
        /// Gets or sets whether emoticons should be synced for this integration (twitch only currently).
        /// </summary>
        public bool EnableEmoticons { get; set; }

        /// <summary>
        /// Sets the behavior for when the integration subscription lapses.
        /// </summary>
        public ModifyIntegrationParameters SetExpireBehavior(int expireBehavior)
        {
            ExpireBehavior = expireBehavior;
            return this;
        }

        /// <summary>
        /// Sets the period (in seconds) where the integration will ignore lapsed subscriptions.
        /// </summary>
        public ModifyIntegrationParameters SetExpireGracePeriod(int expireGracePeriod)
        {
            ExpireGracePeriod = expireGracePeriod;
            return this;
        }

        /// <summary>
        /// Sets whether emoticons should be synced for this integration (twitch only currently).
        /// </summary>
        public ModifyIntegrationParameters SetEnableEmoticons(bool enableEmoticons)
        {
            EnableEmoticons = enableEmoticons;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("expire_behavior", ExpireBehavior);
            data.Set("expire_grace_period", ExpireGracePeriod);
            data.Set("enable_emoticons", EnableEmoticons);

            return data;
        }
    }
}
