using System;
using System.Collections.Generic;

namespace Discore
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
        public string[] Features { get; private set; }

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
                Features = new string[featuresData.Count];
                for (int i = 0; i < Features.Length; i++)
                    Features[i] = featuresData[i].ToString();
            }

            // Update roles
            IList<DiscordApiData> rolesData = data.GetArray("roles");
            if (rolesData != null)
            {
                Roles.Clear();
                foreach (DiscordApiData roleData in rolesData)
                {
                    string roleId = roleData.GetString("id");
                    Roles.Edit(roleId, () => new DiscordRole(),
                        role =>
                        {
                            role.Update(roleData);

                            if (role.Name.ToLower() == "@everyone")
                                AtEveryoneRole = role;

                        });
                }
            }

            // Update emojis
            IList<DiscordApiData> emojisData = data.GetArray("emojis");
            if (emojisData != null)
            {
                Emojis.Clear();
                foreach (DiscordApiData emojiData in emojisData)
                {
                    string emojiId = emojiData.GetString("id");

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
                    string channelId = channelData.GetString("id");
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
                Dictionary<string, DiscordApiData> presences = new Dictionary<string, DiscordApiData>();
                Dictionary<string, DiscordApiData> voiceStates = new Dictionary<string, DiscordApiData>();

                // Get presences
                IList<DiscordApiData> presencesData = data.GetArray("presences");
                if (presences != null)
                {
                    foreach (DiscordApiData presence in presencesData)
                    {
                        string memberId = presence.LocateString("user.id");
                        presences.Add(memberId, presence);
                    }
                }

                // Get voice states
                IList<DiscordApiData> voiceStatesData = data.GetArray("voice_states");
                if (voiceStatesData != null)
                {
                    foreach (DiscordApiData voiceStateData in voiceStatesData)
                    {
                        string memberId = voiceStateData.GetString("user_id");
                        voiceStates.Add(memberId, voiceStateData);
                    }
                }

                // Update each member with their presence and voice state
                foreach (DiscordApiData memberData in membersData)
                {
                    string memberId = memberData.LocateString("user.id");

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
                                member.VoiceState.Update(presence);
                        });
                }
            }

            // Get afk channel
            string afkChannelId = data.GetString("afk_channel_id");
            if (afkChannelId != null)
                AfkChannel = VoiceChannels.Get(afkChannelId);

            // Get owner
            string ownerId = data.GetString("owner_id");
            if (ownerId != null)
                Owner = Members.Get(ownerId);

            // Get embed channel
            string embedChannelId = data.GetString("embed_channel_id");
            if (embedChannelId != null)
                EmbedChannel = Channels.Get(embedChannelId);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
