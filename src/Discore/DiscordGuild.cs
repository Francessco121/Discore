﻿using Discore.Http;
using Discore.Voice;
using Discore.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discore
{
    /// <summary>
    /// Represents a collection of users and channels. Also referred to as a "server".
    /// </summary>
    public sealed class DiscordGuild : DiscordIdEntity
    {
        /// <summary>
        /// Gets the name of this guild.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the icon of this guild or null if the guild has no icon set.
        /// </summary>
        public DiscordCdnUrl Icon { get; }

        /// <summary>
        /// Gets the splash image of this guild or null if the guild has no splash.
        /// </summary>
        public DiscordCdnUrl Splash { get; }

        /// <summary>
        /// Gets the ID of the user who owns this guild.
        /// </summary>
        public Snowflake OwnerId { get; }

        /// <summary>
        /// Gets the ID of the voice region this guild is using.
        /// </summary>
        public string RegionId { get; }

        /// <summary>
        /// Gets the ID of the afk channel in this guild (if set).
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
        /// Gets the ID of the embedded channel, if this guild is embeddable.
        /// </summary>
        public Snowflake? EmbedChannelId { get; }

        /// <summary>
        /// Gets the level of verification required by this guild.
        /// </summary>
        public GuildVerificationLevel VerificationLevel { get; }

        /// <summary>
        /// Gets the default message notification level for users joining this guild.
        /// </summary>
        public GuildNotificationOption DefaultMessageNotifications { get; }

        /// <summary>
        /// Gets the level of explicit content filtering used by this server.
        /// </summary>
        public GuildExplicitContentFilterLevel ExplicitContentFilter { get; }

        /// <summary>
        /// Gets a list of guild features.
        /// </summary>
        public IReadOnlyList<string> Features { get; }

        /// <summary>
        /// Gets the level of multi-factor authentication for this guild.
        /// </summary>
        public GuildMfaLevel MfaLevel { get; }

        /// <summary>
        /// Gets the application ID of the bot who created this guild.
        /// Returns null if this guild was not created by a bot.
        /// </summary>
        public Snowflake? ApplicationId { get; }

        /// <summary>
        /// Gets whether this guild has the widget enabled.
        /// </summary>
        public bool IsWidgetEnabled { get; }

        /// <summary>
        /// Gets the ID of the channel used by the guild's widget.
        /// </summary>
        public Snowflake? WidgetChannelId { get; }

        /// <summary>
        /// Gets the ID of the text channel which system messages are sent to.
        /// </summary>
        public Snowflake? SystemChannelId { get; }

        /// <summary>
        /// The maximum number of presences for the guild.
        /// </summary>
        public int? MaxPresences { get; }

        /// <summary>
        /// The maximum number of members for the guild.
        /// </summary>
        public int? MaxMembers { get; }

        /// <summary>
        /// The vanity URL code for the guild or null if the guild does not have a vanity URL.
        /// </summary>
        public string VanityUrlCode { get; }

        /// <summary>
        /// The description of the guild or null if the guild does not have one.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the guild's banner or null if the guild does not have one.
        /// </summary>
        public DiscordCdnUrl Banner { get; }

        /// <summary>
        /// Gets the Nitro boosting (premium) tier of the guild.
        /// </summary>
        public GuildPremiumTier PremiumTier { get; }

        /// <summary>
        /// Gets the total number of users currently boosting the guild.
        /// </summary>
        public int PremiumSubscriptionCount { get; }

        /// <summary>
        /// Gets the preferred locale of the guild.
        /// </summary>
        public string PreferredLocale { get; }

        /// <summary>
        /// Gets a dictionary of all roles in this guild.
        /// </summary>
        public IReadOnlyDictionary<Snowflake, DiscordRole> Roles { get; }

        /// <summary>
        /// Gets a dictionary of all custom emojis in this guild.
        /// </summary>
        public IReadOnlyDictionary<Snowflake, DiscordEmoji> Emojis { get; }

        readonly DiscordHttpClient http;

        internal DiscordGuild(DiscordHttpClient http, MutableGuild guild)
        {
            this.http = http;

            Id = guild.Id;

            Name = guild.Name;
            RegionId = guild.RegionId;
            AfkTimeout = guild.AfkTimeout;
            IsEmbedEnabled = guild.IsEmbedEnabled;
            VerificationLevel = guild.VerificationLevel;
            MfaLevel = guild.MfaLevel;
            ApplicationId = guild.ApplicationId;
            IsWidgetEnabled = guild.IsWidgetEnabled;
            WidgetChannelId = guild.WidgetChannelId;
            SystemChannelId = guild.SystemChannelId;
            DefaultMessageNotifications = guild.DefaultMessageNotifications;
            ExplicitContentFilter = guild.ExplicitContentFilter;
            OwnerId = guild.OwnerId;
            AfkChannelId = guild.AfkChannelId;
            EmbedChannelId = guild.EmbedChannelId;
            MaxPresences = guild.MaxPresences;
            MaxMembers = guild.MaxMembers;
            VanityUrlCode = guild.VanityUrlCode;
            Description = guild.Description;
            PremiumTier = guild.PremiumTier;
            PremiumSubscriptionCount = guild.PremiumSubscriptionCount;
            PreferredLocale = guild.PreferredLocale;

            if (guild.Icon != null)
                Icon = DiscordCdnUrl.ForGuildIcon(guild.Id, guild.Icon);

            if (guild.Splash != null)
                Splash = DiscordCdnUrl.ForGuildSplash(guild.Id, guild.Splash);

            if (guild.Banner != null)
                Splash = DiscordCdnUrl.ForGuildBanner(guild.Id, guild.Banner);

            Features = new List<string>(guild.Features);

            Roles = guild.Roles.CreateReadonlyCopy();
            Emojis = guild.Emojis.CreateReadonlyCopy();
        }

        internal DiscordGuild(DiscordHttpClient http, DiscordApiData data)
            : base(data)
        {
            this.http = http;

            Name                        = data.GetString("name");
            RegionId                    = data.GetString("region");
            AfkTimeout                  = data.GetInteger("afk_timeout").Value;
            IsEmbedEnabled              = data.GetBoolean("embed_enabled") ?? false;
            OwnerId                     = data.GetSnowflake("owner_id").Value;
            AfkChannelId                = data.GetSnowflake("afk_channel_id");
            EmbedChannelId              = data.GetSnowflake("embed_channel_id");
            ApplicationId               = data.GetSnowflake("application_id");
            IsWidgetEnabled             = data.GetBoolean("widget_enabled") ?? false;
            WidgetChannelId             = data.GetSnowflake("widget_channel_id");
            SystemChannelId             = data.GetSnowflake("system_channel_id");
            MaxPresences                = data.GetInteger("max_presences");
            MaxMembers                  = data.GetInteger("max_members");
            VanityUrlCode               = data.GetString("vanity_url_code");
            Description                 = data.GetString("description");
            PremiumTier                 = (GuildPremiumTier)(data.GetInteger("premium_tier") ?? 0);
            PremiumSubscriptionCount    = data.GetInteger("premium_subscription_count") ?? 0;
            PreferredLocale             = data.GetString("preferred_locale");

            ExplicitContentFilter = (GuildExplicitContentFilterLevel)data.GetInteger("explicit_content_filter").Value;
            VerificationLevel = (GuildVerificationLevel)data.GetInteger("verification_level").Value;
            DefaultMessageNotifications = (GuildNotificationOption)(data.GetInteger("default_message_notifications") ?? 0);
            MfaLevel = (GuildMfaLevel)data.GetInteger("mfa_level").Value;

            // Get image hashes
            string iconHash = data.GetString("icon");
            if (iconHash != null)
                Icon = DiscordCdnUrl.ForGuildIcon(Id, iconHash);

            string splashHash = data.GetString("splash");
            if (splashHash != null)
                Splash = DiscordCdnUrl.ForGuildSplash(Id, splashHash);

            string bannerHash = data.GetString("banner");
            if (bannerHash != null)
                Banner = DiscordCdnUrl.ForGuildBanner(Id, bannerHash);

            // Get features
            IList<DiscordApiData> featuresData = data.GetArray("features");
            string[] features = new string[featuresData.Count];

            for (int i = 0; i < features.Length; i++)
                features[i] = featuresData[i].ToString();

            Features = features;

            // Get roles
            IList<DiscordApiData> rolesData = data.GetArray("roles");
            Dictionary<Snowflake, DiscordRole> roles = new Dictionary<Snowflake, DiscordRole>();

            for (int i = 0; i < rolesData.Count; i++)
            {
                DiscordRole role = new DiscordRole(http, Id, rolesData[i]);
                roles.Add(role.Id, role);
            }

            Roles = roles;

            // Get emojis
            IList<DiscordApiData> emojisArray = data.GetArray("emojis");
            Dictionary<Snowflake, DiscordEmoji> emojis = new Dictionary<Snowflake, DiscordEmoji>();

            for (int i = 0; i < emojisArray.Count; i++)
            {
                DiscordEmoji emoji = new DiscordEmoji(emojisArray[i]);
                emojis.Add(emoji.Id, emoji);
            }

            Emojis = emojis;
        }

        /// <summary>
        /// Returns whether this guild has the given <paramref name="feature"/>.
        /// </summary>
        /// <seealso cref="DiscordGuildFeature"/>
        public bool HasFeature(string feature)
        {
            return Features.Contains(feature);
        }

        /// <summary>
        /// Gets a list of all webhooks in this guild.
        /// <para>Requires <see cref="DiscordPermission.ManageWebhooks"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordWebhook>> GetWebhooks()
        {
            return http.GetGuildWebhooks(Id);
        }

        /// <summary>
        /// Changes the settings of this guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuild> Modify(ModifyGuildOptions options)
        {
            return http.ModifyGuild(Id, options);
        }

        /// <summary>
        /// Deletes this guild permanently.
        /// <para>Note: current bot must be the owner.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task Delete()
        {
            return http.DeleteGuild(Id);
        }

        /// <summary>
        /// Gets a list of all channels in this guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordGuildChannel>> GetChannels()
        {
            return http.GetGuildChannels(Id);
        }

        /// <summary>
        /// Creates a text or voice channel for this guild.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildChannel> CreateChannel(CreateGuildChannelOptions options)
        {
            return http.CreateGuildChannel(Id, options);
        }

        /// <summary>
        /// Changes the positions of channels in the specified guild. The list of
        /// positions does not need to include every channel, it just needs the 
        /// channels that are being moved.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task ModifyChannelPositions(IEnumerable<PositionOptions> positions)
        {
            return http.ModifyGuildChannelPositions(Id, positions);
        }

        /// <summary>
        /// Gets a member of this guild by their user ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildMember> GetMember(Snowflake userId)
        {
            return http.GetGuildMember(Id, userId);
        }

        /// <summary>
        /// Gets a list of members in this guild.
        /// This method is paged, and cannot always return every member at once.
        /// </summary>
        /// <param name="limit">Max number of members to return (1-1000).</param>
        /// <param name="after">The highest user ID in the previous page.</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordGuildMember>> ListGuildMembers(int? limit = null, Snowflake? after = null)
        {
            return http.ListGuildMembers(Id, limit, after);
        }

        /// <summary>
        /// Gets a list of all users banned from this guild.
        /// <para>Requires <see cref="DiscordPermission.BanMembers"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordGuildBan>> GetBans()
        {
            return http.GetGuildBans(Id);
        }

        /// <summary>
        /// Bans the specified user from this guild.
        /// <para>Requires <see cref="DiscordPermission.BanMembers"/>.</para>
        /// </summary>
        /// <param name="deleteMessageDays">Number of days to delete messages for (0-7) or null to delete none.</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task CreateBan(Snowflake userId, int? deleteMessageDays = null)
        {
            return http.CreateGuildBan(Id, userId, deleteMessageDays);
        }

        /// <summary>
        /// Unbans the specified user from this guild.
        /// <para>Requires <see cref="DiscordPermission.BanMembers"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task RemoveBan(Snowflake userId)
        {
            return http.RemoveGuildBan(Id, userId);
        }

        /// <summary>
        /// Gets a list of all roles in this guild.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordRole>> GetRoles()
        {
            return http.GetGuildRoles(Id);
        }

        /// <summary>
        /// Creates a new role for this guild.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <param name="options">A set of optional options to use when creating the role.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordRole> CreateRole(CreateRoleOptions options)
        {
            return http.CreateGuildRole(Id, options);
        }

        /// <summary>
        /// Changes the sorting positions of the roles in this guild.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordRole>> ModifyRolePositions(IEnumerable<PositionOptions> positions)
        {
            return http.ModifyGuildRolePositions(Id, positions);
        }

        /// <summary>
        /// Returns the number of members that would be kicked from a prune operation.
        /// <para>Requires <see cref="DiscordPermission.KickMembers"/>.</para>
        /// </summary>
        /// <param name="days">The number of days to count prune for (1-30).</param>
        /// <param name="includeRoles">
        /// By default, prune will not remove users with roles. You can optionally include specific 
        /// roles in your prune by providing the <paramref name="includeRoles"/> parameter. Any inactive 
        /// user that has a subset of the provided role(s) will be counted in the prune and users with 
        /// additional roles will not.
        /// </param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<int> GetPruneCount(int? days = null, IEnumerable<Snowflake> includeRoles = null)
        {
            return http.GetGuildPruneCount(Id, days, includeRoles);
        }

        /// <summary>
        /// Begins a member prune operation, 
        /// kicking every member that has been offline for the specified number of days.
        /// <para>Requires <see cref="DiscordPermission.KickMembers"/>.</para>
        /// </summary>
        /// <param name="days">The number of days to prune (1-30).</param>
        /// <param name="includeRoles">
        /// By default, prune will not remove users with roles. You can optionally include specific 
        /// roles in your prune by providing the <paramref name="includeRoles"/> parameter. Any inactive 
        /// user that has a subset of the provided role(s) will be counted in the prune and users with 
        /// additional roles will not.
        /// </param>
        /// <param name="computePruneCount">
        /// For large guilds it's recommended to set this to false. When false, this method will always return 0.
        /// </param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<int> BeginPrune(int? days = null, IEnumerable<Snowflake> includeRoles = null, bool? computePruneCount = null)
        {
            return http.BeginGuildPrune(Id, days, includeRoles, computePruneCount);
        }

        /// <summary>
        /// Gets a list of all voice regions available to this guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordVoiceRegion>> GetVoiceRegions()
        {
            return http.GetGuildVoiceRegions(Id);
        }

        /// <summary>
        /// Gets a list of invites to guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordInviteMetadata>> GetInvites()
        {
            return http.GetGuildInvites(Id);
        }

        /// <summary>
        /// Gets a list of integrations for this guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordIntegration>> GetIntegrations()
        {
            return http.GetGuildIntegrations(Id);
        }

        /// <summary>
        /// Attaches an integration from the current bot to this guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task CreateIntegration(Snowflake integrationId, string type)
        {
            return http.CreateGuildIntegration(Id, integrationId, type);
        }

        /// <summary>
        /// Returns the embed for this guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildEmbed> GetEmbed()
        {
            return http.GetGuildEmbed(Id);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
