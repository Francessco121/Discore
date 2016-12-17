using Discore.Http;
using Discore.Voice;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore
{
    public sealed class DiscordGuild : DiscordIdObject
    {
        /// <summary>
        /// Gets the name of this guild.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the icon hash of this guild.
        /// </summary>
        public string Icon { get; }

        /// <summary>
        /// Gets the splash image hash of this guild.
        /// </summary>
        public string Splash { get; }

        /// <summary>
        /// Gets the id of the user who owns this guild.
        /// </summary>
        public Snowflake OwnerId { get; }

        /// <summary>
        /// Gets the id of the voice region this guild is using.
        /// </summary>
        public string RegionId { get; }

        /// <summary>
        /// Gets the id of the afk channel in this guild (if set).
        /// </summary>
        public Snowflake? AfkChannelId { get; }
        /// <summary>
        /// Gets the afk timeout in seconds of this guild (if set).
        /// </summary>
        public int AfkTimeout { get; }

        /// <summary>
        /// Gets whether this guild is embeddable as a widget.
        /// </summary>
        public bool IsEmbedEnabled { get; }
        /// <summary>
        /// Gets the id of the embedded channel, if this guild is embeddable.
        /// </summary>
        public Snowflake? EmbedChannelId { get; }

        /// <summary>
        /// Gets the level of verification required by this guild.
        /// </summary>
        public int VerificationLevel { get; }

        /// <summary>
        /// Gets the default message notification level for users joining this guild.
        /// </summary>
        public int DefaultMessageNotifications { get; }

        /// <summary>
        /// Gets a list of guild features.
        /// </summary>
        public IReadOnlyCollection<string> Features { get; }

        /// <summary>
        /// Gets the level of multi-factor authentication for this guild.
        /// </summary>
        public int MFALevel { get; }

        /// <summary>
        /// Gets the number of members in this guild.
        /// </summary>
        public int MemberCount { get; }

        /// <summary>
        /// Gets the date-time that the current authenticated user joined this guild (if information is available).
        /// </summary>
        /// <remarks>Available if this guild was retrieved through the gateway.</remarks>
        public DateTime? JoinedAt { get; }

        /// <summary>
        /// Gets whether this guild is considered large (if information is available).
        /// </summary>
        /// <remarks>Available if this guild was retrieved through the gateway.</remarks>
        public bool? IsLarge { get;  }

        /// <summary>
        /// Gets whether this guild is unavailable (if information is available).
        /// </summary>
        public bool IsUnavailable { get; private set; }

        /// <summary>
        /// Gets the id of the @everyone role, which contains the default permissions for everyone in this guild.
        /// </summary>
        public Snowflake AtEveryoneRoleId { get; }

        /// <summary>
        /// Gets a table of all roles in this guild.
        /// </summary>
        public IReadOnlyDictionary<Snowflake, DiscordRole> Roles
        {
            get { return guildCache != null ? guildCache.Roles : roles; }
        }

        /// <summary>
        /// Gets a table of all custom emojis in this guild.
        /// </summary>
        public IReadOnlyDictionary<Snowflake, DiscordEmoji> Emojis
        {
            get { return guildCache != null ? guildCache.Emojis : emojis; }
        }

        IDiscordApplication app;
        DiscordHttpGuildEndpoint guildHttp;
        DiscordHttpWebhookEndpoint webhookHttp;

        DiscoreGuildCache guildCache;
        IReadOnlyDictionary<Snowflake, DiscordRole> roles;
        IReadOnlyDictionary<Snowflake, DiscordEmoji> emojis;

        internal DiscordGuild(IDiscordApplication app, DiscoreGuildCache guildCache, DiscordApiData data)
            : this(app, data, true)
        {
            this.guildCache = guildCache;
        }

        internal DiscordGuild(IDiscordApplication app, DiscordApiData data)
            : this(app, data, false)
        { }

        private DiscordGuild(IDiscordApplication app, DiscordApiData data, bool isWebSocket)
            : base(data)
        {
            this.app = app;

            guildHttp = app.HttpApi.Guilds;
            webhookHttp = app.HttpApi.Webhooks;

            IsUnavailable = data.GetBoolean("unavailable") ?? false;
            if (IsUnavailable)
                return;

            // Always available
            Name                        = data.GetString("name");
            Icon                        = data.GetString("icon");
            Splash                      = data.GetString("splash");
            RegionId                    = data.GetString("region");
            AfkTimeout                  = data.GetInteger("afk_timeout").Value;
            IsEmbedEnabled              = data.GetBoolean("embed_enabled") ?? false;
            VerificationLevel           = data.GetInteger("verification_level").Value;
            MFALevel                    = data.GetInteger("mfa_level").Value;
            DefaultMessageNotifications = data.GetInteger("default_message_notifications") ?? 0;
            MemberCount                 = data.GetInteger("member_count").Value;
            OwnerId                     = data.GetSnowflake("owner_id").Value;
            AfkChannelId                = data.GetSnowflake("afk_channel_id");
            EmbedChannelId              = data.GetSnowflake("embed_channel_id");

            // Only available in GUILD_CREATE
            JoinedAt                    = data.GetDateTime("joined_at");
            IsLarge                     = data.GetBoolean("large");

            // Get features
            IList<DiscordApiData> featuresData = data.GetArray("features");
            string[] features = new string[featuresData.Count];

            for (int i = 0; i < features.Length; i++)
                features[i] = featuresData[i].ToString();

            Features = features;

            // Only deserialize if not created from the websocket interface,
            // this information is already available in the websocket cache.
            if (!isWebSocket)
            {
                // Get roles
                IList<DiscordApiData> rolesData = data.GetArray("roles");
                Dictionary<Snowflake, DiscordRole> roles = new Dictionary<Snowflake, DiscordRole>();

                for (int i = 0; i < rolesData.Count; i++)
                {
                    DiscordRole role = new DiscordRole(app, Id, rolesData[i]);
                    roles.Add(role.Id, role);
                }

                this.roles = roles;

                // Get emojis
                IList<DiscordApiData> emojisArray = data.GetArray("emojis");
                Dictionary<Snowflake, DiscordEmoji> emojis = new Dictionary<Snowflake, DiscordEmoji>();

                for (int i = 0; i < emojisArray.Count; i++)
                {
                    DiscordEmoji emoji = new DiscordEmoji(emojisArray[i]);
                    emojis.Add(emoji.Id, emoji);
                }

                this.emojis = emojis;
            }
        }

        internal DiscordGuild UpdateUnavailable(bool unavailable)
        {
            DiscordGuild guild = (DiscordGuild)MemberwiseClone();
            guild.IsUnavailable = unavailable;

            return guild;
        }

        /// <summary>
        /// Gets a list of all webhooks in this guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordWebhook>> GetWebhooks()
        {
            return await webhookHttp.GetGuildWebhooks(Id);
        }

        /// <summary>
        /// Changes the settings of this guild.
        /// </summary>
        public async Task<DiscordGuild> Modify(ModifyGuildParameters parameters)
        {
            return await guildHttp.Modify(Id, parameters);
        }

        /// <summary>
        /// Deletes this guild permanently.
        /// Authenticated user must be the owner.
        /// </summary>
        public async Task<DiscordGuild> Delete()
        {
            return await guildHttp.Delete(Id);
        }

        /// <summary>
        /// Gets a list of all channels in this guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordGuildChannel>> GetChannels()
        {
            return await guildHttp.GetChannels(Id);
        }

        /// <summary>
        /// Creates a text or voice channel for this guild.
        /// </summary>
        public async Task<DiscordGuildChannel> CreateChannel(CreateGuildChannelParameters parameters)
        {
            return await guildHttp.CreateChannel(Id, parameters);
        }

        /// <summary>
        /// Changes the sorting positions of the channels in this guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordGuildChannel>> ModifyChannelPositions(IEnumerable<PositionParameters> positions)
        {
            return await guildHttp.ModifyChannelPositions(Id, positions);
        }

        /// <summary>
        /// Gets a member of this guild by their user ID.
        /// </summary>
        public async Task<DiscordGuildMember> GetMember(Snowflake userId)
        {
            return await guildHttp.GetMember(Id, userId);
        }

        /// <summary>
        /// Gets a list of members in this guild.
        /// This method is paged, and cannot always return every member at once.
        /// </summary>
        /// <param name="limit">Max number of members to return (1-1000).</param>
        /// <param name="after">The highest user id in the previous page.</param>
        public async Task<IReadOnlyList<DiscordGuildMember>> GetMembers(int? limit = null, Snowflake? after = null)
        {
            return await guildHttp.ListMembers(Id, limit, after);
        }

        /// <summary>
        /// Gets a list of all users banned from this guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordUser>> GetBans()
        {
            return await guildHttp.GetBans(Id);
        }

        /// <summary>
        /// Bans the specified user from this guild.
        /// </summary>
        /// <param name="deleteMessageDays">Number of days to delete messages for (0-7).</param>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> CreateBan(Snowflake userId, int? deleteMessageDays = null)
        {
            return await guildHttp.CreateBan(Id, userId, deleteMessageDays);
        }

        /// <summary>
        /// Unbans the specified user from this guild.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> RemoveBan(Snowflake userId)
        {
            return await guildHttp.RemoveBan(Id, userId);
        }

        /// <summary>
        /// Gets a list of all roles in this guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordRole>> GetRoles()
        {
            return await guildHttp.GetRoles(Id);
        }

        /// <summary>
        /// Creates a new role with default settings for this guild.
        /// </summary>
        public async Task<DiscordRole> CreateRole()
        {
            return await guildHttp.CreateRole(Id);
        }

        /// <summary>
        /// Changes the sorting positions of the roles in this guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordRole>> ModifyRolePositions(IEnumerable<PositionParameters> positions)
        {
            return await guildHttp.ModifyRolePositions(Id, positions);
        }

        /// <summary>
        /// Returns the number of members that would be kicked from a prune operation.
        /// </summary>
        /// <param name="days">The number of days to count prune for (1 or more).</param>
        public async Task<int> GetPruneCount(int days)
        {
            return await guildHttp.GetPruneCount(Id, days);
        }

        /// <summary>
        /// Begins a member prune operation, 
        /// kicking every member that has been offline for the specified number of days.
        /// </summary>
        /// <param name="days">The number of days to prune (1 or more).</param>
        public async Task<int> BeginPrune(int days)
        {
            return await guildHttp.BeginPrune(Id, days);
        }

        /// <summary>
        /// Gets a list of all voice regions available to this guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordVoiceRegion>> GetVoiceRegions()
        {
            return await guildHttp.GetVoiceRegions(Id);
        }

        /// <summary>
        /// Gets a list of invites to guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordInviteMetadata>> GetInvites()
        {
            return await guildHttp.GetInvites(Id);
        }

        /// <summary>
        /// Gets a list of integrations for this guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordIntegration>> GetIntegrations()
        {
            return await guildHttp.GetIntegrations(Id);
        }

        /// <summary>
        /// Attaches an integration from the current authenticated user to this guild.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> CreateIntegration(Snowflake integrationId, string type)
        {
            return await guildHttp.CreateIntegration(Id, integrationId, type);
        }

        /// <summary>
        /// Returns the embed for this guild.
        /// </summary>
        public async Task<DiscordGuildEmbed> GetEmbed()
        {
            return await guildHttp.GetEmbed(Id);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
