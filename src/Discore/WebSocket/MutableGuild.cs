using Discore.Http;
using System.Collections.Generic;

namespace Discore.WebSocket
{
    class MutableGuild : MutableEntity<DiscordGuild>
    {
        public Snowflake Id { get; }

        public string Name { get; private set; }
        public string Icon { get; private set; }
        public string Splash { get; private set; }
        public Snowflake OwnerId { get; private set; }
        public string RegionId { get; private set; }
        public Snowflake? AfkChannelId { get; private set; }
        public int AfkTimeout { get; private set; }
        public bool IsEmbedEnabled { get; private set; }
        public Snowflake? EmbedChannelId { get; private set; }
        public GuildVerificationLevel VerificationLevel { get; private set; }
        public GuildNotificationOption DefaultMessageNotifications { get; private set; }
        public GuildExplicitContentFilterLevel ExplicitContentFilter { get; private set; }
        public GuildMfaLevel MfaLevel { get; private set; }
        public Snowflake? ApplicationId { get; private set; }
        public bool IsWidgetEnabled { get; private set; }
        public Snowflake? WidgetChannelId { get; private set; }
        public Snowflake? SystemChannelId { get; private set; }
        public int? MaxPresences { get; private set; }
        public int? MaxMembers { get; private set; }
        public string VanityUrlCode { get; private set; }
        public string Description { get; private set; }
        public string Banner { get; private set; }
        public GuildPremiumTier PremiumTier { get; private set; }
        public int PremiumSubscriptionCount { get; private set; }
        public string PreferredLocale { get; private set; }

        public IReadOnlyList<string> Features { get; private set; }

        public ShardCacheDictionary<DiscordEmoji> Emojis { get; }
        public ShardCacheDictionary<DiscordRole> Roles { get; }

        public MutableGuild(Snowflake id, DiscordHttpClient http)
            : base(http)
        {
            Id = id;

            Features = new List<string>();

            Emojis = new ShardCacheDictionary<DiscordEmoji>();
            Roles = new ShardCacheDictionary<DiscordRole>();
        }

        public void Update(DiscordApiData data)
        {
            Name = data.GetString("name");
            Icon = data.GetString("icon");
            Splash = data.GetString("splash");
            RegionId = data.GetString("region");
            AfkTimeout = data.GetInteger("afk_timeout").Value;
            IsEmbedEnabled = data.GetBoolean("embed_enabled") ?? false;
            OwnerId = data.GetSnowflake("owner_id").Value;
            AfkChannelId = data.GetSnowflake("afk_channel_id");
            EmbedChannelId = data.GetSnowflake("embed_channel_id");
            ApplicationId = data.GetSnowflake("application_id");
            IsWidgetEnabled = data.GetBoolean("widget_enabled") ?? false;
            WidgetChannelId = data.GetSnowflake("widget_channel_id");
            SystemChannelId = data.GetSnowflake("system_channel_id");
            MaxPresences = data.GetInteger("max_presences");
            MaxMembers = data.GetInteger("max_members");
            VanityUrlCode = data.GetString("vanity_url_code");
            Description = data.GetString("description");
            PremiumTier = (GuildPremiumTier)(data.GetInteger("premium_tier") ?? 0);
            PremiumSubscriptionCount = data.GetInteger("premium_subscription_count") ?? 0;
            PreferredLocale = data.GetString("preferred_locale");
            Banner = data.GetString("banner");

            ExplicitContentFilter = (GuildExplicitContentFilterLevel)data.GetInteger("explicit_content_filter").Value;
            VerificationLevel = (GuildVerificationLevel)data.GetInteger("verification_level").Value;
            DefaultMessageNotifications = (GuildNotificationOption)(data.GetInteger("default_message_notifications") ?? 0);
            MfaLevel = (GuildMfaLevel)data.GetInteger("mfa_level").Value;

            // Deserialize features
            IList<DiscordApiData> featuresArray = data.GetArray("features");
            List<string> features = new List<string>(featuresArray.Count);

            for (int i = 0; i < features.Count; i++)
                features[i] = featuresArray[i].ToString();

            Features = features;

            // Deserialize roles
            Roles.Clear();
            IList<DiscordApiData> rolesArray = data.GetArray("roles");
            for (int i = 0; i < rolesArray.Count; i++)
            {
                DiscordRole role = new DiscordRole(Id, rolesArray[i]);
                Roles[role.Id] = role;
            }

            // Deserialize emojis
            Emojis.Clear();
            IList<DiscordApiData> emojisArray = data.GetArray("emojis");
            for (int i = 0; i < emojisArray.Count; i++)
            {
                DiscordEmoji emoji = new DiscordEmoji(emojisArray[i]);
                Emojis[emoji.Id] = emoji;
            }

            Dirty();
        }

        protected override DiscordGuild BuildImmutableEntity()
        {
            return new DiscordGuild(
                id: Id,
                name: Name,
                icon: Icon != null ? DiscordCdnUrl.ForGuildIcon(Id, Icon) : null,
                splash: Splash != null ? DiscordCdnUrl.ForGuildSplash(Id, Splash) : null,
                ownerId: OwnerId,
                regionId: RegionId,
                afkChannelId: AfkChannelId,
                afkTimeout: AfkTimeout,
                isEmbedEnabled: IsEmbedEnabled,
                embedChannelId: EmbedChannelId,
                verificationLevel: VerificationLevel,
                defaultMessageNotifications: DefaultMessageNotifications,
                explicitContentFilter: ExplicitContentFilter,
                features: new List<string>(Features),
                mfaLevel: MfaLevel,
                applicationId: ApplicationId,
                isWidgetEnabled: IsWidgetEnabled,
                widgetChannelId: WidgetChannelId,
                systemChannelId: SystemChannelId,
                maxPresences: MaxPresences,
                maxMembers: MaxMembers,
                vanityUrlCode: VanityUrlCode,
                description: Description,
                banner: Banner != null ? DiscordCdnUrl.ForGuildBanner(Id, Banner) : null,
                premiumTier: PremiumTier,
                premiumSubscriptionCount: PremiumSubscriptionCount,
                preferredLocale: PreferredLocale,
                roles: Roles.CreateReadonlyCopy(),
                emojis: Emojis.CreateReadonlyCopy());
        }
    }
}
