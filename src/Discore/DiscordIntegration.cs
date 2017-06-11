using Discore.Http;
using System;
using System.Threading.Tasks;

namespace Discore
{
    /// <summary>
    /// A guild integration.
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

        DiscordHttpClient http;

        internal DiscordIntegration(DiscordHttpClient http, DiscordApiData data, Snowflake guildId)
            : this(http, data)
        {
            GuildId = guildId;
        }

        internal DiscordIntegration(DiscordHttpClient http, DiscordApiData data)
            : base(data)
        {
            this.http = http;

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

        /// <summary>
        /// Changes the attributes of this integration, if this is a guild integration.
        /// <para>You can check if this is a guild integration, if <see cref="GuildId"/> is not null.</para>
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        /// <exception cref="InvalidOperationException">Thrown if this is not a guild integration.</exception>
        public Task Modify(ModifyIntegrationParameters parameters)
        {
            if (!GuildId.HasValue)
                throw new InvalidOperationException("This integration does not represent a guild integration");

            return http.ModifyGuildIntegration(GuildId.Value, Id, parameters);
        }

        /// <summary>
        /// Deletes this integration, if this is a guild integration.
        /// <para>You can check if this is a guild integration, if <see cref="GuildId"/> is not null.</para>
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        /// <exception cref="InvalidOperationException">Thrown if this is not a guild integration.</exception>
        public Task Delete()
        {
            if (!GuildId.HasValue)
                throw new InvalidOperationException("This integration does not represent a guild integration");

            return http.DeleteGuildIntegration(GuildId.Value, Id);
        }

        /// <summary>
        /// Synchronizes this integration, if this is a guild integration.
        /// <para>You can check if this is a guild integration, if <see cref="GuildId"/> is not null.</para>
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        /// <exception cref="InvalidOperationException">Thrown if this is not a guild integration.</exception>
        public Task Sync()
        {
            if (!GuildId.HasValue)
                throw new InvalidOperationException("This integration does not represent a guild integration");

            return http.SyncGuildIntegration(GuildId.Value, Id);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
