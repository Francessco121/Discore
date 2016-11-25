using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Discore.Http
{
    public class DiscordGuild : DiscordIdObject
    {
        public string Name { get; }

        /// <summary>
        /// Gets the icon hash of this guild.
        /// </summary>
        public string Icon { get; }

        /// <summary>
        /// Gets the splash image hash of this guild.
        /// </summary>
        public string Splash { get; }

        public string Region { get; }

        public Snowflake OwnerId { get; }

        /// <summary>
        /// Gets the afk channel id in this guild (if set).
        /// </summary>
        public Snowflake? AfkChannelId { get; }
        /// <summary>
        /// Gets the afk timeout in seconds of this guild.
        /// </summary>
        public int AfkTimeout { get; }

        /// <summary>
        /// Gets whether this guild is embeddable as a widget.
        /// </summary>
        public bool IsEmbedEnabled { get; }
        /// <summary>
        /// Gets the embedded channel id, if this guild is embeddable.
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
        /// Gets a list of all roles in this guild.
        /// </summary>
        public IReadOnlyCollection<DiscordRole> Roles { get; }

        /// <summary>
        /// Gets a list of all emojis in this guild.
        /// </summary>
        public IReadOnlyCollection<DiscordEmoji> Emojis { get; }

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

        public DiscordGuild(DiscordApiData data)
        {
            Name                        = data.GetString("name");
            Icon                        = data.GetString("icon");
            Splash                      = data.GetString("splash");
            Region                      = data.GetString("region");
            AfkTimeout                  = data.GetInteger("afk_timeout").Value;
            IsEmbedEnabled              = data.GetBoolean("embed_enabled").Value;
            VerificationLevel           = data.GetInteger("verification_level").Value;
            MFALevel                    = data.GetInteger("mfa_level").Value;
            DefaultMessageNotifications = data.GetInteger("default_messages_notifications").Value;
            MemberCount                 = data.GetInteger("member_count").Value;
            OwnerId                     = data.GetSnowflake("owner_id").Value;
            AfkChannelId                = data.GetSnowflake("afk_channel_id");
            EmbedChannelId              = data.GetSnowflake("embed_channel_id");

            // Update features
            IList<DiscordApiData> featuresData = data.GetArray("features");
            string[] features = new string[featuresData.Count];

            for (int i = 0; i < features.Length; i++)
                features[i] = featuresData[i].ToString();

            Features = new ReadOnlyCollection<string>(features);

            // Update roles
            IList<DiscordApiData> rolesData = data.GetArray("roles");
            DiscordRole[] roles = new DiscordRole[rolesData.Count];

            for (int i = 0; i < rolesData.Count; i++)
                roles[i] = new DiscordRole(rolesData[i]);

            Roles = new ReadOnlyCollection<DiscordRole>(roles);

            // Update emojis
            IList<DiscordApiData> emojisData = data.GetArray("emojis");
            DiscordEmoji[] emojis = new DiscordEmoji[emojisData.Count];

            for (int i = 0; i < emojisData.Count; i++)
                emojis[i] = new DiscordEmoji(emojisData[i]);

            Emojis = new ReadOnlyCollection<DiscordEmoji>(emojis);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
