using Discore.Http;
using System;
using System.Threading.Tasks;

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
        /// Gets the id of the associated role with this integration.
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
        /// Gets the id of the associated guild with this integration.
        /// </summary>
        public Snowflake? GuildId { get; }

        DiscordHttpGuildEndpoint guildsHttp;

        internal DiscordIntegration(IDiscordApplication app, DiscordApiData data, Snowflake guildId)
            : this(app, data)
        {
            GuildId = guildId;
        }

        internal DiscordIntegration(IDiscordApplication app, DiscordApiData data)
            : base(data)
        {
            guildsHttp = app.HttpApi.Guilds;

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
                User = new DiscordUser(userData);

            DiscordApiData accountData = data.Get("account");
            if (accountData != null)
                Account = new DiscordIntegrationAccount(accountData);
        }

        /// <summary>
        /// Changes the attributes of this integration, if this is a guild integration.
        /// <para>You can check if this is a guild integration, if <see cref="GuildId"/> is not null.</para>
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this is not a guild integration.</exception>
        public async Task<bool> Modify(ModifyIntegrationParameters parameters)
        {
            if (!GuildId.HasValue)
                throw new InvalidOperationException("This integration does not represent a guild integration");

            return await guildsHttp.ModifyIntegration(GuildId.Value, Id, parameters);
        }

        /// <summary>
        /// Deletes this integration, if this is a guild integration.
        /// <para>You can check if this is a guild integration, if <see cref="GuildId"/> is not null.</para>
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this is not a guild integration.</exception>
        public async Task<bool> Delete()
        {
            if (!GuildId.HasValue)
                throw new InvalidOperationException("This integration does not represent a guild integration");

            return await guildsHttp.DeleteIntegration(GuildId.Value, Id);
        }

        /// <summary>
        /// Synchronizes this integration, if this is a guild integration.
        /// <para>You can check if this is a guild integration, if <see cref="GuildId"/> is not null.</para>
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this is not a guild integration.</exception>
        public async Task<bool> Sync()
        {
            if (!GuildId.HasValue)
                throw new InvalidOperationException("This integration does not represent a guild integration");

            return await guildsHttp.SyncIntegration(GuildId.Value, Id);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
