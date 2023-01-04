using System;
using System.Text.Json;

namespace Discore
{
    /// <summary>
    /// A user or guild integration.
    /// </summary>
    public class DiscordIntegration : DiscordIdEntity
    {
        /// <summary>
        /// Gets the name of this integration.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the type of this integration.
        /// </summary>
        public string Type { get; }
        /// <summary>
        /// Gets whether or not this integration is enabled.
        /// </summary>
        public bool IsEnabled { get; }
        /// <summary>
        /// Gets whether or not this integration is syncing.
        /// </summary>
        public bool? IsSyncing { get; }
        /// <summary>
        /// Gets the ID of the associated role with this integration.
        /// </summary>
        public Snowflake? RoleId { get; }
        /// <summary>
        /// Gets the behavior of expiring subscribers.
        /// </summary>
        public IntegrationExpireBehavior? ExpireBehavior { get; }
        /// <summary>
        /// Gets the expire grace period (in days) before expiring subscribers.
        /// </summary>
        public int? ExpireGracePeriod { get; }
        /// <summary>
        /// Gets the associated <see cref="DiscordUser"/> with this integration.
        /// </summary>
        public DiscordUser? User { get; }
        /// <summary>
        /// Gets the account of this integration.
        /// </summary>
        public DiscordIntegrationAccount Account { get; }
        /// <summary>
        /// Gets the last time this integration was synced.
        /// </summary>
        public DateTime? SyncedAt { get; }
        /// <summary>
        /// Gets the ID of the associated guild with this integration.
        /// </summary>
        public Snowflake? GuildId { get; }

        // TODO: add enable_emoticons, subscriber_count, revoked, application

        internal DiscordIntegration(JsonElement json, Snowflake? guildId = null)
            : base(json)
        {
            Name = json.GetProperty("name").GetString()!;
            Type = json.GetProperty("type").GetString()!;
            IsEnabled = json.GetProperty("enabled").GetBoolean();
            IsSyncing = json.GetPropertyOrNull("syncing")?.GetBoolean();
            RoleId = json.GetPropertyOrNull("role_id")?.GetSnowflake();
            ExpireBehavior = (IntegrationExpireBehavior?)json.GetPropertyOrNull("expire_behavior")?.GetInt32();
            ExpireGracePeriod = json.GetPropertyOrNull("expire_grace_period")?.GetInt32();
            Account = new DiscordIntegrationAccount(json.GetProperty("account"));
            SyncedAt = json.GetPropertyOrNull("synced_at")?.GetDateTime();
            GuildId = guildId;

            JsonElement? userJson = json.GetPropertyOrNull("user");
            User = userJson == null ? null : new DiscordUser(userJson.Value, isWebhookUser: false);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
