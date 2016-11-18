using System;

namespace Discore.Http
{
    /// <summary>
    /// A guild integration.
    /// </summary>
    public class DiscordIntegration : DiscordIdObject
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
        public bool IsSyncing { get; }
        /// <summary>
        /// Gets the id of the associated role with this integration.
        /// </summary>
        public Snowflake RoleId { get; }
        /// <summary>
        /// Gets the expire behavior of this integration.
        /// </summary>
        public int ExpireBehavior { get; }
        /// <summary>
        /// Gets the expire grace period of this integration.
        /// </summary>
        public int ExpireGracePeriod { get; }
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
        public DateTime SyncedAt { get; }
        /// <summary>
        /// Gets the id of the associated guild with this integration.
        /// </summary>
        public Snowflake GuildId { get; }

        public DiscordIntegration(DiscordApiData data, Snowflake guildId)
            : base(data)
        {
            GuildId = guildId;

            Name = data.GetString("name");
            Type = data.GetString("type");
            IsEnabled = data.GetBoolean("enabled").Value;
            IsSyncing = data.GetBoolean("syncing").Value;
            ExpireBehavior = data.GetInteger("expire_behavior").Value;
            ExpireGracePeriod = data.GetInteger("expire_grace_period").Value;
            SyncedAt = data.GetDateTime("synced_at").Value;
            RoleId = data.GetSnowflake("role_id").Value;

            DiscordApiData userData = data.Get("user");
            User = new DiscordUser(userData);

            DiscordApiData accountData = data.Get("account");
            Account = new DiscordIntegrationAccount(accountData);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
