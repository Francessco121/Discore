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
        public int VerificationLevel { get; private set; }
        public int DefaultMessageNotifications { get; private set; }
        public int MFALevel { get; private set; }

        public IReadOnlyList<string> Features { get; private set; }

        public ShardCacheDictionary<DiscordEmoji> Emojis { get; }
        public ShardCacheDictionary<DiscordRole> Roles { get; }

        public MutableGuild(Snowflake id, DiscordHttpApi http)
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
            VerificationLevel = data.GetInteger("verification_level").Value;
            MFALevel = data.GetInteger("mfa_level").Value;
            DefaultMessageNotifications = data.GetInteger("default_message_notifications") ?? 0;
            OwnerId = data.GetSnowflake("owner_id").Value;
            AfkChannelId = data.GetSnowflake("afk_channel_id");
            EmbedChannelId = data.GetSnowflake("embed_channel_id");

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
                DiscordRole role = new DiscordRole(Http, Id, rolesArray[i]);
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
            return new DiscordGuild(Http, this);
        }
    }
}
