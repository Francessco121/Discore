using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Discore.WebSocket
{
    public sealed class DiscordGuild : DiscordIdObject
    {
        public string Name { get; private set; }

        /// <summary>
        /// Gets the icon hash of this guild.
        /// </summary>
        public string Icon { get; private set; }

        /// <summary>
        /// Gets the splash image hash of this guild.
        /// </summary>
        public string Splash { get; private set; }

        // TODO: get actual region
        public string Region { get; private set; }

        public DiscordGuildMember Owner { get; private set; }

        /// <summary>
        /// Gets the first undeletable text channel of this guild.
        /// </summary>
        public DiscordGuildTextChannel InitialTextChannel { get; private set; }

        /// <summary>
        /// Gets the afk channel in this guild (if set).
        /// </summary>
        public DiscordGuildVoiceChannel AfkChannel { get; private set; }
        /// <summary>
        /// Gets the afk timeout in seconds of this guild.
        /// </summary>
        public int AfkTimeout { get; private set; }

        /// <summary>
        /// Gets whether this guild is embeddable as a widget.
        /// </summary>
        public bool IsEmbedEnabled { get; private set; }
        /// <summary>
        /// Gets the embedded channel, if this guild is embeddable.
        /// </summary>
        public DiscordGuildChannel EmbedChannel { get; private set; }

        /// <summary>
        /// Gets the level of verification required by this guild.
        /// </summary>
        public int VerificationLevel { get; private set; }

        /// <summary>
        /// Gets the default message notification level for users joining this guild.
        /// </summary>
        public int DefaultMessageNotifications { get; private set; }

        /// <summary>
        /// Gets a list of guild features.
        /// </summary>
        public IReadOnlyCollection<string> Features { get; private set; }

        /// <summary>
        /// Gets the level of multi-factor authentication for this guild.
        /// </summary>
        public int MFALevel { get; private set; }

        /// <summary>
        /// Gets the date-time that the current authenticated user joined this guild.
        /// </summary>
        public DateTime JoinedAt { get; private set; }

        /// <summary>
        /// Gets whether this guild is considered large.
        /// </summary>
        public bool IsLarge { get; private set; }

        /// <summary>
        /// Gets whether this guild is unavailable.
        /// </summary>
        public bool IsUnavailable { get; internal set; }

        /// <summary>
        /// Gets the @everyone role, which contains the default permissions for everyone in this guild.
        /// </summary>
        public DiscordRole AtEveryoneRole { get; private set; }

        /// <summary>
        /// Gets a table of all members in this guild.
        /// </summary>
        public DiscordApiCacheTable<DiscordGuildMember> Members { get; }

        /// <summary>
        /// Gets a table of all roles in this guild.
        /// </summary>
        public DiscordApiCacheTable<DiscordRole> Roles { get; }

        /// <summary>
        /// Gets a table of all custom emojis in this guild.
        /// </summary>
        public DiscordApiCacheTable<DiscordEmoji> Emojis { get; }

        /// <summary>
        /// Gets a table of all text and voice channels in this guild.
        /// </summary>
        public DiscordApiCacheTable<DiscordGuildChannel> Channels { get; }

        /// <summary>
        /// Gets a table of all text channels in this guild.
        /// </summary>
        public DiscordApiCacheTable<DiscordGuildTextChannel> TextChannels { get; }

        /// <summary>
        /// Gets a table of all voice channels in this guild.
        /// </summary>
        public DiscordApiCacheTable<DiscordGuildVoiceChannel> VoiceChannels { get; }

        Shard shard;

        internal DiscordGuild(Shard shard)
        {
            this.shard = shard;

            Members       = new DiscordApiCacheTable<DiscordGuildMember>();
            Roles         = new DiscordApiCacheTable<DiscordRole>();
            Emojis        = new DiscordApiCacheTable<DiscordEmoji>();
            Channels      = new DiscordApiCacheTable<DiscordGuildChannel>();
            TextChannels  = new DiscordApiCacheTable<DiscordGuildTextChannel>();
            VoiceChannels = new DiscordApiCacheTable<DiscordGuildVoiceChannel>();
        }

        internal override void Update(DiscordApiData data)
        {
            // Perform base update first, since we need the id for updating
            // other properties such as emojis.
            base.Update(data);

            Name                        = data.GetString("name") ?? Name;
            Icon                        = data.GetString("icon") ?? Icon;
            Splash                      = data.GetString("splash") ?? Splash;
            Region                      = data.GetString("region") ?? Region;
            AfkTimeout                  = data.GetInteger("afk_timeout") ?? AfkTimeout;
            IsEmbedEnabled              = data.GetBoolean("embed_enabled") ?? IsEmbedEnabled;
            VerificationLevel           = data.GetInteger("verification_level") ?? VerificationLevel;
            MFALevel                    = data.GetInteger("mfa_level") ?? MFALevel;
            DefaultMessageNotifications = data.GetInteger("default_messages_notifications") ?? DefaultMessageNotifications;
            JoinedAt                    = data.GetDateTime("joined_at") ?? JoinedAt;
            IsUnavailable               = data.GetBoolean("unavailable") ?? IsUnavailable;

            // Update features
            IList<DiscordApiData> featuresData = data.GetArray("features");
            if (featuresData != null)
            {
                string[] features = new string[featuresData.Count];
                for (int i = 0; i < features.Length; i++)
                    features[i] = featuresData[i].ToString();

                Features = new ReadOnlyCollection<string>(features);
            }

            // Update roles
            IList<DiscordApiData> rolesData = data.GetArray("roles");
            if (rolesData != null)
            {
                Roles.Clear();
                foreach (DiscordApiData roleData in rolesData)
                {
                    Snowflake roleId = roleData.GetSnowflake("id").Value;
                    DiscordRole role = Roles.Edit(roleId, () => new DiscordRole(),
                        r =>
                        {
                            r.Update(roleData);

                            if (r.Name.ToLower() == "@everyone")
                                AtEveryoneRole = r;

                        });

                    shard.Roles.Set(roleId, role);
                }
            }

            // Update emojis
            IList<DiscordApiData> emojisData = data.GetArray("emojis");
            if (emojisData != null)
            {
                Emojis.Clear();
                foreach (DiscordApiData emojiData in emojisData)
                {
                    Snowflake emojiId = emojiData.GetSnowflake("id").Value;

                    Emojis.Edit(emojiId, () => new DiscordEmoji(shard, this),
                        emoji => emoji.Update(emojiData));
                }
            }

            // Update channels
            IList<DiscordApiData> channelsData = data.GetArray("channels");
            if (channelsData != null)
            {
                Channels.Clear();
                TextChannels.Clear();
                VoiceChannels.Clear();

                foreach (DiscordApiData channelData in channelsData)
                {
                    Snowflake channelId = channelData.GetSnowflake("id").Value;
                    DiscordGuildChannel channel;

                    string type = channelData.GetString("type");
                    if (type == "voice")
                    {
                        channel = VoiceChannels.Edit(channelId, 
                            () => new DiscordGuildVoiceChannel(shard, this),
                            c => c.Update(channelData));
                    }
                    else
                    {
                        channel = TextChannels.Edit(channelId,
                            () => new DiscordGuildTextChannel(shard, this),
                            c => c.Update(channelData));

                        // Check if it is the initial channel
                        if (channelId == Id)
                            InitialTextChannel = (DiscordGuildTextChannel)channel;
                    }

                    // Set aliases
                    Channels.Set(channelId, channel);
                    shard.Channels.Set(channelId, channel);
                }
            }

            // Update members
            IList<DiscordApiData> membersData = data.GetArray("members");
            if (membersData != null)
            {
                Dictionary<Snowflake, DiscordApiData> presences = new Dictionary<Snowflake, DiscordApiData>();
                Dictionary<Snowflake, DiscordApiData> voiceStates = new Dictionary<Snowflake, DiscordApiData>();

                // Get presences
                IList<DiscordApiData> presencesData = data.GetArray("presences");
                if (presences != null)
                {
                    foreach (DiscordApiData presence in presencesData)
                    {
                        Snowflake memberId = presence.LocateSnowflake("user.id").Value;
                        presences.Add(memberId, presence);
                    }
                }

                // Get voice states
                IList<DiscordApiData> voiceStatesData = data.GetArray("voice_states");
                if (voiceStatesData != null)
                {
                    foreach (DiscordApiData voiceStateData in voiceStatesData)
                    {
                        Snowflake memberId = voiceStateData.GetSnowflake("user_id").Value;
                        voiceStates.Add(memberId, voiceStateData);
                    }
                }

                // Update each member with their presence and voice state
                foreach (DiscordApiData memberData in membersData)
                {
                    Snowflake memberId = memberData.LocateSnowflake("user.id").Value;

                    Members.Edit(memberId, () => new DiscordGuildMember(shard, this),
                        member =>
                        {
                            member.Update(memberData);

                            // Include the presence in the update, if it exists
                            DiscordApiData presence;
                            if (presences.TryGetValue(memberId, out presence))
                                member.Update(presence);

                            // Include the voice state in the update, if it exists
                            DiscordApiData voiceState;
                            if (voiceStates.TryGetValue(memberId, out voiceState))
                                member.VoiceState.Update(voiceState);
                        });
                }
            }

            // Get afk channel
            Snowflake? afkChannelId = data.GetSnowflake("afk_channel_id");
            if (afkChannelId != null)
                AfkChannel = VoiceChannels.Get(afkChannelId.Value);

            // Get owner
            Snowflake? ownerId = data.GetSnowflake("owner_id");
            if (ownerId != null)
                Owner = Members.Get(ownerId.Value);

            // Get embed channel
            Snowflake? embedChannelId = data.GetSnowflake("embed_channel_id");
            if (embedChannelId != null)
                EmbedChannel = Channels.Get(embedChannelId.Value);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
