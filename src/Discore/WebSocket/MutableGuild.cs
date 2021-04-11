using System.Collections.Generic;
using System.Text.Json;

namespace Discore.WebSocket
{
    class MutableGuild : MutableEntity<DiscordGuild>
    {
        public Snowflake Id { get; }

        public string? Name { get; private set; }
        public string? Icon { get; private set; }
        public string? Splash { get; private set; }
        public Snowflake OwnerId { get; private set; }
        public string? RegionId { get; private set; }
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
        public string? VanityUrlCode { get; private set; }
        public string? Description { get; private set; }
        public string? Banner { get; private set; }
        public GuildPremiumTier PremiumTier { get; private set; }
        public int? PremiumSubscriptionCount { get; private set; }
        public string? PreferredLocale { get; private set; }

        public IReadOnlyList<string> Features { get; private set; }

        public ShardCacheDictionary<DiscordEmoji> Emojis { get; }
        public ShardCacheDictionary<DiscordRole> Roles { get; }

        public MutableGuild(Snowflake id)
        {
            Id = id;

            Features = new List<string>();

            Emojis = new ShardCacheDictionary<DiscordEmoji>();
            Roles = new ShardCacheDictionary<DiscordRole>();
        }

        public void Update(JsonElement json)
        {
            Name = json.GetProperty("name").GetString()!;
            RegionId = json.GetProperty("region").GetString()!;
            AfkTimeout = json.GetProperty("afk_timeout").GetInt32();
            AfkChannelId = json.GetProperty("afk_channel_id").GetSnowflakeOrNull();
            IsEmbedEnabled = json.GetPropertyOrNull("embed_enabled")?.GetBoolean() ?? false;
            EmbedChannelId = json.GetPropertyOrNull("embed_channel_id")?.GetSnowflakeOrNull();
            OwnerId = json.GetProperty("owner_id").GetSnowflake();
            ApplicationId = json.GetProperty("application_id").GetSnowflakeOrNull();
            IsWidgetEnabled = json.GetPropertyOrNull("widget_enabled")?.GetBoolean() ?? false;
            WidgetChannelId = json.GetPropertyOrNull("widget_channel_id")?.GetSnowflakeOrNull();
            SystemChannelId = json.GetProperty("system_channel_id").GetSnowflakeOrNull();
            MaxPresences = json.GetPropertyOrNull("max_presences")?.GetInt32OrNull();
            MaxMembers = json.GetPropertyOrNull("max_members")?.GetInt32();
            VanityUrlCode = json.GetProperty("vanity_url_code").GetString();
            Description = json.GetProperty("description").GetString();
            PremiumTier = (GuildPremiumTier)json.GetProperty("premium_tier").GetInt32();
            PremiumSubscriptionCount = json.GetPropertyOrNull("premium_subscription_count")?.GetInt32();
            PreferredLocale = json.GetProperty("preferred_locale").GetString()!;
            ExplicitContentFilter = (GuildExplicitContentFilterLevel)json.GetProperty("explicit_content_filter").GetInt32();
            VerificationLevel = (GuildVerificationLevel)json.GetProperty("verification_level").GetInt32();
            DefaultMessageNotifications = (GuildNotificationOption)json.GetProperty("default_message_notifications").GetInt32();
            MfaLevel = (GuildMfaLevel)json.GetProperty("mfa_level").GetInt32();

            // Get image hashes
            Icon = json.GetProperty("icon").GetString();
            Splash = json.GetProperty("splash").GetString();
            Banner = json.GetProperty("banner").GetString();

            // Get features
            JsonElement featuresJson = json.GetProperty("features");
            string[] features = new string[featuresJson.GetArrayLength()];

            for (int i = 0; i < features.Length; i++)
                features[i] = featuresJson[i].GetString()!;

            Features = features;

            // Get roles
            Roles.Clear();
            JsonElement rolesJson = json.GetProperty("roles");

            int numRoles = rolesJson.GetArrayLength();
            for (int i = 0; i < numRoles; i++)
            {
                var role = new DiscordRole(rolesJson[i], guildId: Id);
                Roles[role.Id] = role;
            }

            // Get emojis
            Emojis.Clear();
            JsonElement emojisJson = json.GetProperty("emojis");

            int numEmojis = emojisJson.GetArrayLength();
            for (int i = 0; i < numEmojis; i++)
            {
                var emoji = new DiscordEmoji(emojisJson[i]);
                Emojis[emoji.Id] = emoji;
            }

            Dirty();
        }

        protected override DiscordGuild BuildImmutableEntity()
        {
            return new DiscordGuild(
                id: Id,
                name: Name!,
                icon: Icon != null ? DiscordCdnUrl.ForGuildIcon(Id, Icon) : null,
                splash: Splash != null ? DiscordCdnUrl.ForGuildSplash(Id, Splash) : null,
                ownerId: OwnerId,
                regionId: RegionId!,
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
                preferredLocale: PreferredLocale!,
                roles: Roles.CreateReadonlyCopy(),
                emojis: Emojis.CreateReadonlyCopy());
        }
    }
}
