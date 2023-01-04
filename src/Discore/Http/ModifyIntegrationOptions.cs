using System.Text.Json;

namespace Discore.Http
{
    /// <summary>
    /// A set of parameters for modifying a guild integration.
    /// </summary>
    public class ModifyIntegrationOptions
    {
        /// <summary>
        /// Gets or sets the behavior to follow when the integration subscription lapses.
        /// </summary>
        public IntegrationExpireBehavior ExpireBehavior { get; set; }
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
        public ModifyIntegrationOptions SetExpireBehavior(IntegrationExpireBehavior expireBehavior)
        {
            ExpireBehavior = expireBehavior;
            return this;
        }

        /// <summary>
        /// Sets the period (in seconds) where the integration will ignore lapsed subscriptions.
        /// </summary>
        public ModifyIntegrationOptions SetExpireGracePeriod(int expireGracePeriod)
        {
            ExpireGracePeriod = expireGracePeriod;
            return this;
        }

        /// <summary>
        /// Sets whether emoticons should be synced for this integration (twitch only currently).
        /// </summary>
        public ModifyIntegrationOptions SetEnableEmoticons(bool enableEmoticons)
        {
            EnableEmoticons = enableEmoticons;
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteNumber("expire_behavior", (int)ExpireBehavior);
            writer.WriteNumber("expire_grace_period", ExpireGracePeriod);
            writer.WriteBoolean("enable_emoticons", EnableEmoticons);

            writer.WriteEndObject();
        }
    }
}
