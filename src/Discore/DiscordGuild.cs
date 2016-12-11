using System;
using System.Collections.Generic;

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

        DiscoreGuildCache guildCache;
        IReadOnlyDictionary<Snowflake, DiscordRole> roles;
        IReadOnlyDictionary<Snowflake, DiscordEmoji> emojis;

        internal DiscordGuild(DiscoreGuildCache guildCache, DiscordApiData data)
            : this(data, true)
        {
            this.guildCache = guildCache;
        }

        internal DiscordGuild(DiscordApiData data)
            : this(data, false)
        { }

        private DiscordGuild(DiscordApiData data, bool isWebSocket)
            : base(data)
        {
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
            DefaultMessageNotifications = data.GetInteger("default_messages_notifications") ?? 0;
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
                    DiscordRole role = new DiscordRole(Id, rolesData[i]);
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

        public override string ToString()
        {
            return Name;
        }
    }
}
