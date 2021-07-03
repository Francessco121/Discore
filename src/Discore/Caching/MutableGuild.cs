using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Discore.Caching
{
    class MutableGuild : MutableEntity<DiscordGuild>
    {
        public Snowflake Id { get; }

        public string? Name { get; private set; }
        public DiscordCdnUrl? Icon { get; private set; }
        public DiscordCdnUrl? Splash { get; private set; }
        public Snowflake OwnerId { get; private set; }
        public string? RegionId { get; private set; }
        public Snowflake? AfkChannelId { get; private set; }
        public int AfkTimeout { get; private set; }
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
        public DiscordCdnUrl? Banner { get; private set; }
        public GuildPremiumTier PremiumTier { get; private set; }
        public int? PremiumSubscriptionCount { get; private set; }
        public string? PreferredLocale { get; private set; }

        public IReadOnlyList<string> Features { get; private set; }

        public CacheDictionary<DiscordEmoji> Emojis { get; }
        public CacheDictionary<DiscordRole> Roles { get; }

        public MutableGuild(Snowflake id)
        {
            Id = id;

            Features = new List<string>();

            Emojis = new CacheDictionary<DiscordEmoji>();
            Roles = new CacheDictionary<DiscordRole>();
        }

        public void Update(DiscordGuild guild)
        {
            Name = guild.Name;
            RegionId = guild.RegionId;
            AfkTimeout = guild.AfkTimeout;
            AfkChannelId = guild.AfkChannelId;
            OwnerId = guild.OwnerId;
            ApplicationId = guild.ApplicationId;
            IsWidgetEnabled = guild.IsWidgetEnabled;
            WidgetChannelId = guild.WidgetChannelId;
            SystemChannelId = guild.SystemChannelId;
            MaxPresences = guild.MaxPresences;
            MaxMembers = guild.MaxMembers;
            VanityUrlCode = guild.VanityUrlCode;
            Description = guild.Description;
            PremiumTier = guild.PremiumTier;
            PremiumSubscriptionCount = guild.PremiumSubscriptionCount;
            PreferredLocale = guild.PreferredLocale;
            ExplicitContentFilter = guild.ExplicitContentFilter;
            VerificationLevel = guild.VerificationLevel;
            DefaultMessageNotifications = guild.DefaultMessageNotifications;
            MfaLevel = guild.MfaLevel;
            Icon = guild.Icon;
            Splash = guild.Splash;
            Banner = guild.Banner;

            // Get features
            Features = guild.Features.ToArray();

            // Get roles
            Roles.Clear();
            Roles.AddRange(guild.Roles);

            // Get emojis
            Emojis.Clear();
            Emojis.AddRange(guild.Emojis);

            // Mark as dirty
            Dirty();
        }

        protected override DiscordGuild BuildImmutableEntity()
        {
            return new DiscordGuild(
                id: Id,
                name: Name!,
                icon: Icon,
                splash: Splash,
                ownerId: OwnerId,
                regionId: RegionId!,
                afkChannelId: AfkChannelId,
                afkTimeout: AfkTimeout,
                verificationLevel: VerificationLevel,
                defaultMessageNotifications: DefaultMessageNotifications,
                explicitContentFilter: ExplicitContentFilter,
                features: Features.ToArray(),
                mfaLevel: MfaLevel,
                applicationId: ApplicationId,
                isWidgetEnabled: IsWidgetEnabled,
                widgetChannelId: WidgetChannelId,
                systemChannelId: SystemChannelId,
                maxPresences: MaxPresences,
                maxMembers: MaxMembers,
                vanityUrlCode: VanityUrlCode,
                description: Description,
                banner: Banner,
                premiumTier: PremiumTier,
                premiumSubscriptionCount: PremiumSubscriptionCount,
                preferredLocale: PreferredLocale!,
                roles: Roles.CreateReadonlyCopy(),
                emojis: Emojis.CreateReadonlyCopy());
        }
    }
}
