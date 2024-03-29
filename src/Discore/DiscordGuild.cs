using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Discore
{
    /// <summary>
    /// Represents a collection of users and channels. Also referred to as a "server".
    /// </summary>
    public class DiscordGuild : DiscordIdEntity
    {
        /// <summary>
        /// Gets the name of this guild.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the icon of this guild or null if the guild has no icon set.
        /// </summary>
        public DiscordCdnUrl? Icon { get; }

        /// <summary>
        /// Gets the splash image of this guild or null if the guild has no splash.
        /// </summary>
        public DiscordCdnUrl? Splash { get; }

        /// <summary>
        /// Gets the ID of the user who owns this guild.
        /// </summary>
        public Snowflake OwnerId { get; }

        /// <summary>
        /// Gets the ID of the afk channel in this guild (if set).
        /// </summary>
        public Snowflake? AfkChannelId { get; }
        /// <summary>
        /// Gets the afk timeout in seconds of this guild (if set).
        /// </summary>
        public int AfkTimeout { get; }

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
        public string? VanityUrlCode { get; }

        /// <summary>
        /// The description of the guild or null if the guild does not have one.
        /// </summary>
        public string? Description { get; }
        
        /// <summary>
        /// Gets the guild's banner or null if the guild does not have one.
        /// </summary>
        public DiscordCdnUrl? Banner { get; }

        /// <summary>
        /// Gets the Nitro boosting (premium) tier of the guild.
        /// </summary>
        public GuildPremiumTier PremiumTier { get; }

        /// <summary>
        /// Gets the total number of users currently boosting the guild.
        /// </summary>
        public int? PremiumSubscriptionCount { get; }

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

        internal DiscordGuild(
            Snowflake id,
            string name,
            DiscordCdnUrl? icon,
            DiscordCdnUrl? splash,
            Snowflake ownerId,
            Snowflake? afkChannelId,
            int afkTimeout,
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
            string? vanityUrlCode,
            string? description,
            DiscordCdnUrl? banner,
            GuildPremiumTier premiumTier,
            int? premiumSubscriptionCount,
            string preferredLocale,
            IReadOnlyDictionary<Snowflake, DiscordRole> roles,
            IReadOnlyDictionary<Snowflake, DiscordEmoji> emojis)
            : base(id)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Icon = icon;
            Splash = splash;
            OwnerId = ownerId;
            AfkChannelId = afkChannelId;
            AfkTimeout = afkTimeout;
            VerificationLevel = verificationLevel;
            DefaultMessageNotifications = defaultMessageNotifications;
            ExplicitContentFilter = explicitContentFilter;
            Features = features ?? throw new ArgumentNullException(nameof(features));
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
            PreferredLocale = preferredLocale ?? throw new ArgumentNullException(nameof(preferredLocale));
            Roles = roles ?? throw new ArgumentNullException(nameof(roles));
            Emojis = emojis ?? throw new ArgumentNullException(nameof(emojis));
        }

        internal DiscordGuild(JsonElement json)
            : base(json)
        {
            Name = json.GetProperty("name").GetString()!;
            AfkTimeout = json.GetProperty("afk_timeout").GetInt32();
            AfkChannelId = json.GetPropertyOrNull("afk_channel_id")?.GetSnowflakeOrNull();
            OwnerId = json.GetProperty("owner_id").GetSnowflake();
            ApplicationId = json.GetPropertyOrNull("application_id")?.GetSnowflakeOrNull();
            IsWidgetEnabled = json.GetPropertyOrNull("widget_enabled")?.GetBoolean() ?? false;
            WidgetChannelId = json.GetPropertyOrNull("widget_channel_id")?.GetSnowflakeOrNull();
            SystemChannelId = json.GetPropertyOrNull("system_channel_id")?.GetSnowflakeOrNull();
            MaxPresences = json.GetPropertyOrNull("max_presences")?.GetInt32OrNull();
            MaxMembers = json.GetPropertyOrNull("max_members")?.GetInt32();
            VanityUrlCode = json.GetPropertyOrNull("vanity_url_code")?.GetString();
            Description = json.GetPropertyOrNull("description")?.GetString();
            PremiumTier = (GuildPremiumTier)json.GetProperty("premium_tier").GetInt32();
            PremiumSubscriptionCount = json.GetPropertyOrNull("premium_subscription_count")?.GetInt32();
            PreferredLocale = json.GetProperty("preferred_locale").GetString()!;
            ExplicitContentFilter = (GuildExplicitContentFilterLevel)json.GetProperty("explicit_content_filter").GetInt32();
            VerificationLevel = (GuildVerificationLevel)json.GetProperty("verification_level").GetInt32();
            DefaultMessageNotifications = (GuildNotificationOption)json.GetProperty("default_message_notifications").GetInt32();
            MfaLevel = (GuildMfaLevel)json.GetProperty("mfa_level").GetInt32();

            // Get image hashes
            string? iconHash = json.GetPropertyOrNull("icon")?.GetString();
            if (iconHash != null)
                Icon = DiscordCdnUrl.ForGuildIcon(Id, iconHash);

            string? splashHash = json.GetPropertyOrNull("splash")?.GetString();
            if (splashHash != null)
                Splash = DiscordCdnUrl.ForGuildSplash(Id, splashHash);

            string? bannerHash = json.GetPropertyOrNull("banner")?.GetString();
            if (bannerHash != null)
                Banner = DiscordCdnUrl.ForGuildBanner(Id, bannerHash);

            // Get features
            JsonElement featuresJson = json.GetProperty("features");
            string[] features = new string[featuresJson.GetArrayLength()];

            for (int i = 0; i < features.Length; i++)
                features[i] = featuresJson[i].GetString()!;

            Features = features;

            // Get roles
            JsonElement rolesJson = json.GetProperty("roles");
            var roles = new Dictionary<Snowflake, DiscordRole>();

            int numRoles = rolesJson.GetArrayLength();
            for (int i = 0; i < numRoles; i++)
            {
                var role = new DiscordRole(rolesJson[i], guildId: Id);
                roles.Add(role.Id, role);
            }

            Roles = roles;

            // Get emojis
            JsonElement emojisJson = json.GetProperty("emojis");
            var emojis = new Dictionary<Snowflake, DiscordEmoji>();

            int numEmojis = emojisJson.GetArrayLength();
            for (int i = 0; i < numEmojis; i++)
            {
                var emoji = new DiscordEmoji(emojisJson[i]);
                emojis.Add(emoji.Id, emoji);
            }

            Emojis = emojis;
        }

        /// <summary>
        /// Returns whether this guild has the given <paramref name="feature"/>.
        /// </summary>
        /// <seealso cref="DiscordGuildFeature"/>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="feature"/> is null.</exception>
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
