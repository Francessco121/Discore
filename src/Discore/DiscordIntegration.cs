using System;

namespace Discore
{
    /// <summary>
    /// A guild integration.
    /// </summary>
    public sealed class DiscordIntegration : DiscordIdObject
    {
        /// <summary>
        /// Gets the name of this integration.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the type of this integration.
        /// </summary>
        public string Type { get; private set; }
        /// <summary>
        /// Gets whether or not this integration is enabled.
        /// </summary>
        public bool IsEnabled { get; private set; }
        /// <summary>
        /// Gets whether or not this integration is syncing.
        /// </summary>
        public bool IsSyncing { get; private set; }
        /// <summary>
        /// Gets the associated <see cref="DiscordRole"/> with this integration.
        /// </summary>
        public DiscordRole Role { get; private set; }
        /// <summary>
        /// Gets the expire behavior of this integration.
        /// </summary>
        public int ExpireBehavior { get; private set; }
        /// <summary>
        /// Gets the expire grace period of this integration.
        /// </summary>
        public int ExpireGracePeriod { get; private set; }
        /// <summary>
        /// Gets the associated <see cref="DiscordUser"/> with this integration.
        /// </summary>
        public DiscordUser User { get; private set; }
        /// <summary>
        /// Gets the account of this integration.
        /// </summary>
        public DiscordIntegrationAccount Account { get; private set; }
        /// <summary>
        /// Gets the last time this integration was synced.
        /// </summary>
        public DateTime SyncedAt { get; private set; }
        /// <summary>
        /// Gets the associated <see cref="DiscordGuild"/> with this integration.
        /// </summary>
        public DiscordGuild Guild { get; private set; }

        Shard shard;

        internal DiscordIntegration(Shard shard, DiscordGuild guild)
        {
            this.shard = shard;
            Guild = guild;
        }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            Name = data.GetString("name") ?? Name;
            Type = data.GetString("type") ?? Type;
            IsEnabled = data.GetBoolean("enabled") ?? IsEnabled;
            IsSyncing = data.GetBoolean("syncing") ?? IsSyncing;
            ExpireBehavior = data.GetInteger("expire_behavior") ?? ExpireBehavior;
            ExpireGracePeriod = data.GetInteger("expire_grace_period") ?? ExpireGracePeriod;
            SyncedAt = data.GetDateTime("synced_at") ?? SyncedAt;

            Snowflake? roleId = data.GetSnowflake("role_id");
            if (roleId != null)
                Role = Guild.Roles.Get(roleId.Value);

            DiscordApiData userData = data.Get("user");
            if (userData != null)
            {
                Snowflake userId = userData.GetSnowflake("id").Value;
                User = shard.Users.Edit(userId, () => new DiscordUser(), user => user.Update(userData));
            }

            DiscordApiData accountData = data.Get("account");
            if (accountData != null)
            {
                if (Account == null)
                    Account = new DiscordIntegrationAccount();

                Account.Update(accountData);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
