namespace Discore.Http
{
    /// <summary>
    /// A set of parameters for modifying a guild integration.
    /// </summary>
    public class ModifyIntegrationParameters
    {
        /// <summary>
        /// The behavior when an integration subscription lapses.
        /// </summary>
        public int ExpireBehavior { get; set; }
        /// <summary>
        /// The period (in seconds) where the integration will ignore lapsed subscriptions.
        /// </summary>
        public int ExpireGracePeriod { get; set; }
        /// <summary>
        /// Whether emoticons should be synced for this integration (twitch only currently).
        /// </summary>
        public bool EnableEmoticons { get; set; }

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
