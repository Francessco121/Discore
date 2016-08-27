using System;

namespace Discore
{
    /// <summary>
    /// A <see cref="DiscordGuild"/> integration.
    /// </summary>
    public class DiscordIntegration : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of this integration.
        /// </summary>
        public string Id { get; private set; }
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

        DiscordApiCache cache;

        /// <summary>
        /// Creates a new <see cref="DiscordIntegration"/> instance.
        /// </summary>
        /// <param name="client">The associated <see cref="IDiscordClient"/>.</param>
        /// <param name="guild">The assocaited <see cref="DiscordGuild"/>.</param>
        public DiscordIntegration(IDiscordClient client, DiscordGuild guild)
        {
            cache = client.Cache;
            Guild = guild;
        }

        /// <summary>
        /// Updates this integration with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this integration with.</param>
        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            Name = data.GetString("name") ?? Name;
            Type = data.GetString("type") ?? Type;
            IsEnabled = data.GetBoolean("enabled") ?? IsEnabled;
            IsSyncing = data.GetBoolean("syncing") ?? IsSyncing;
            ExpireBehavior = data.GetInteger("expire_behavior") ?? ExpireBehavior;
            ExpireGracePeriod = data.GetInteger("expire_grace_period") ?? ExpireGracePeriod;
            SyncedAt = data.GetDateTime("synced_at") ?? SyncedAt;

            string roleId = data.GetString("role_id");
            if (roleId != null)
            {
                DiscordRole role;
                if (cache.TryGet(Guild, roleId, out role))
                    Role = role;
            }

            DiscordApiData userData = data.Get("user");
            if (userData != null)
            {
                string userId = userData.GetString("id");
                User = cache.AddOrUpdate(userId, userData, () => { return new DiscordUser(); });
            }

            DiscordApiData accountData = data.Get("account");
            if (accountData != null)
            {
                if (Account == null)
                    Account = new DiscordIntegrationAccount();

                Account.Update(accountData);
            }
        }
    }
}
