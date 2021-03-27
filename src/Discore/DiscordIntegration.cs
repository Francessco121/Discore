using System;

namespace Discore
{
    /// <summary>
    /// A user or guild integration.
    /// </summary>
    public sealed class DiscordIntegration : DiscordIdEntity
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
        public bool? IsEnabled { get; }
        /// <summary>
        /// Gets whether or not this integration is syncing.
        /// </summary>
        public bool? IsSyncing { get; }
        /// <summary>
        /// Gets the ID of the associated role with this integration.
        /// </summary>
        public Snowflake? RoleId { get; }
        /// <summary>
        /// Gets the expire behavior of this integration.
        /// </summary>
        public int? ExpireBehavior { get; }
        /// <summary>
        /// Gets the expire grace period of this integration.
        /// </summary>
        public int? ExpireGracePeriod { get; }
        /// <summary>
        /// Gets the associated <see cref="DiscordUser"/> with this integration.
        /// </summary>
        public DiscordUser User { get; }
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

        internal DiscordIntegration(DiscordApiData data, Snowflake guildId)
            : this(data)
        {
            GuildId = guildId;
        }

        internal DiscordIntegration(DiscordApiData data)
            : base(data)
        {
            Name = data.GetString("name");
            Type = data.GetString("type");
            IsEnabled = data.GetBoolean("enabled");
            IsSyncing = data.GetBoolean("syncing");
            ExpireBehavior = data.GetInteger("expire_behavior");
            ExpireGracePeriod = data.GetInteger("expire_grace_period");
            SyncedAt = data.GetDateTime("synced_at");
            RoleId = data.GetSnowflake("role_id");

            DiscordApiData userData = data.Get("user");
            if (userData != null)
                User = new DiscordUser(false, userData);

            DiscordApiData accountData = data.Get("account");
            if (accountData != null)
                Account = new DiscordIntegrationAccount(accountData);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
