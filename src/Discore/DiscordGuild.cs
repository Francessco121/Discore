using System.Collections.Generic;
using System.Linq;

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

        public DiscordGuild(
            Snowflake id,
            string name,
            DiscordCdnUrl icon,
            DiscordCdnUrl splash,
            Snowflake ownerId,
            string regionId,
            Snowflake? afkChannelId,
            int afkTimeout,
            bool isEmbedEnabled,
            Snowflake? embedChannelId,
            GuildVerificationLevel verificationLevel,
            GuildNotificationOption defaultMessageNotifications,
            GuildExplicitContentFilterLevel explicitContentFilter,
            IReadOnlyList<string> features,
            GuildMfaLevel mfaLevel,
            Snowflake? applicationId,
            bool isWidgetEnabled,
            Snowflake? widgetChannelId,
            Snowflake? systemChannelId,
            int? maxPresences,
            int? maxMembers,
            string vanityUrlCode,
            string description,
            DiscordCdnUrl banner,
            GuildPremiumTier premiumTier,
            int premiumSubscriptionCount,
            string preferredLocale,
            IReadOnlyDictionary<Snowflake, DiscordRole> roles,
            IReadOnlyDictionary<Snowflake, DiscordEmoji> emojis)
            : base(id)
        {
            Name = name;
            Icon = icon;
            Splash = splash;
            OwnerId = ownerId;
            RegionId = regionId;
            AfkChannelId = afkChannelId;
            AfkTimeout = afkTimeout;
            IsEmbedEnabled = isEmbedEnabled;
            EmbedChannelId = embedChannelId;
            VerificationLevel = verificationLevel;
            DefaultMessageNotifications = defaultMessageNotifications;
            ExplicitContentFilter = explicitContentFilter;
            Features = features;
            MfaLevel = mfaLevel;
            ApplicationId = applicationId;
            IsWidgetEnabled = isWidgetEnabled;
            WidgetChannelId = widgetChannelId;
            SystemChannelId = systemChannelId;
            MaxPresences = maxPresences;
            MaxMembers = maxMembers;
            VanityUrlCode = vanityUrlCode;
            Description = description;
            Banner = banner;
            PremiumTier = premiumTier;
            PremiumSubscriptionCount = premiumSubscriptionCount;
            PreferredLocale = preferredLocale;
            Roles = roles;
            Emojis = emojis;
        }

        internal DiscordGuild(DiscordApiData data)
            : base(data)
        {
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
                DiscordRole role = new DiscordRole(Id, rolesData[i]);
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

        public override string ToString()
        {
            return Name;
        }
    }
}
