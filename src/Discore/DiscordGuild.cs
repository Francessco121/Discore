using System.Collections.Generic;

namespace Discore
{
    /// <summary>
    /// Guilds in Discord represent a collection of users and channels into an isolated "server".
    /// </summary>
    public class DiscordGuild : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of this guild.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets the name of this guild.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the icon hash of this guild.
        /// </summary>
        public string Icon { get; private set; }
        /// <summary>
        /// Gets the splash hash of this guild.
        /// </summary>
        public string Splash { get; private set; }
        /// <summary>
        /// Gets the owner of this guild.
        /// </summary>
        public DiscordGuildMember Owner { get; private set; }
        /// <summary>
        /// Gets the region id of this guild.
        /// </summary>
        // TODO: Grab actual region
        public string Region { get; private set; }
        /// <summary>
        /// Gets the id of the afk channel in this guild.
        /// </summary>
        // TODO: Grab actual channel
        public string AfkChannelId { get; private set; }
        /// <summary>
        /// Gets the afk timeout in seconds of this guild.
        /// </summary>
        public int AfkTimeout { get; private set; }
        /// <summary>
        /// Gets whether or not this guild is embeddable as a widget.
        /// </summary>
        public bool EmbedEnabled { get; private set; }
        /// <summary>
        /// Gets the id of the embedded channel.
        /// </summary>
        public string EmbedChannelId { get; private set; }
        /// <summary>
        /// Gets the level of verification required by this guild.
        /// </summary>
        public int VerificationLevel { get; private set; }
        /// <summary>
        /// Gets a list of guild features.
        /// </summary>
        public string[] Features { get; private set; }

        /// <summary>
        /// Gets the <see cref="DiscordRole"/> containing default permissions for
        /// everyone in this guild.
        /// </summary>
        public DiscordRole AtEveryoneRole
        {
            get
            {
                DiscordRole role;
                if (cache.TryGet(this, atEveryoneRoleId, out role))
                    return role;

                return null;
            }
        }

        /// <summary>
        /// Gets a list of all channels in this guild.
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, DiscordGuildChannel>> Channels
        {
            get { return cache.GetList<DiscordGuildChannel, DiscordGuild>(this); }
        }
        /// <summary>
        /// Gets a list of all emojis in this guild.
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, DiscordEmoji>> Emojis
        {
            get { return cache.GetList<DiscordEmoji, DiscordGuild>(this); }
        }
        /// <summary>
        /// Gets a list of all roles in this guild.
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, DiscordRole>> Roles
        {
            get { return cache.GetList<DiscordRole, DiscordGuild>(this); }
        }
        /// <summary>
        /// Gets a list of all members in this guild.
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, DiscordGuildMember>> Members
        {
            get { return cache.GetList<DiscordGuildMember, DiscordGuild>(this); }
        }

        string atEveryoneRoleId;

        IDiscordClient client;
        DiscordApiCache cache;

        /// <summary>
        /// Creates a new <see cref="DiscordGuild"/> instance.
        /// </summary>
        /// <param name="client">The associated <see cref="IDiscordClient"/>.</param>
        public DiscordGuild(IDiscordClient client)
        {
            this.client = client;
            cache = client.Cache;
        }

        /// <summary>
        /// Attempts to get a channel in this guild.
        /// </summary>
        /// <param name="id">The id of the channel.</param>
        /// <param name="channel">The found <see cref="DiscordGuildChannel"/>.</param>
        /// <returns>Returns whether or not the <see cref="DiscordGuildChannel"/> was found.</returns>
        public bool TryGetChannel(string id, out DiscordGuildChannel channel)
        {
            return cache.TryGet(this, id, out channel);
        }

        /// <summary>
        /// Attempts to get an emoji in this guild.
        /// </summary>
        /// <param name="id">The id of the emoji.</param>
        /// <param name="emoji">The found <see cref="DiscordEmoji"/>.</param>
        /// <returns>Returns whether or not the <see cref="DiscordEmoji"/> was found.</returns>
        public bool TryGetEmoji(string id, out DiscordEmoji emoji)
        {
            return cache.TryGet(this, id, out emoji);
        }

        /// <summary>
        /// Attempts to get a role in this guild.
        /// </summary>
        /// <param name="id">The id of the role.</param>
        /// <param name="role">The found <see cref="DiscordRole"/>.</param>
        /// <returns>Returns whether or not the <see cref="DiscordRole"/> was found.</returns>
        public bool TryGetRole(string id, out DiscordRole role)
        {
            return cache.TryGet(this, id, out role);
        }

        /// <summary>
        /// Attempts to get a guild member in this guild.
        /// </summary>
        /// <param name="id">The id of the member.</param>
        /// <param name="member">The found <see cref="DiscordGuildMember"/>.</param>
        /// <returns>Returns whether or not the <see cref="DiscordGuildMember"/> was found.</returns>
        public bool TryGetMember(string id, out DiscordGuildMember member)
        {
            return cache.TryGet(this, id, out member);
        }

        /// <summary>
        /// Updates this guild with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this guild with.</param>
        public void Update(DiscordApiData data)
        {
            Id                = data.GetString("id") ?? Id;
            Name              = data.GetString("name") ?? Name;
            Icon              = data.GetString("icon") ?? Icon;
            Splash            = data.GetString("splash") ?? Splash;
            Region            = data.GetString("region") ?? Region;
            AfkChannelId      = data.GetString("afk_chanenl_id") ?? AfkChannelId;
            AfkTimeout        = data.GetInteger("afk_timeout") ?? AfkTimeout;
            EmbedEnabled      = data.GetBoolean("embed_enabled") ?? EmbedEnabled;
            EmbedChannelId    = data.GetString("embed_channel_id") ?? EmbedChannelId;
            VerificationLevel = data.GetInteger("verification_level") ?? VerificationLevel;

            // Update features
            IReadOnlyList<DiscordApiData> featuresData = data.GetArray("features");
            if (featuresData != null)
            {
                Features = new string[featuresData.Count];
                for (int i = 0; i < Features.Length; i++)
                    Features[i] = featuresData[i].ToString();
            }

            // Update roles
            IReadOnlyList<DiscordApiData> rolesData = data.GetArray("roles");
            if (rolesData != null)
            {
                foreach (DiscordApiData roleData in rolesData)
                {
                    string roleId = roleData.GetString("id");
                    DiscordRole role = cache.AddOrUpdate(this, roleId, roleData, 
                        () => { return new DiscordRole(); });

                    if (role.Name.ToLower() == "@everyone")
                        atEveryoneRoleId = role.Id;
                }
            }

            // Update emojis
            IReadOnlyList<DiscordApiData> emojisData = data.GetArray("emojis");
            if (emojisData != null)
            {
                foreach (DiscordApiData emojiData in emojisData)
                {
                    string eId = emojiData.GetString("id");
                    cache.AddOrUpdate(this, eId, emojiData, () => { return new DiscordEmoji(client, this); });
                }
            }

            // Update channels
            IReadOnlyList<DiscordApiData> channelsData = data.GetArray("channels");
            if (channelsData != null)
            {
                foreach (DiscordApiData channelData in channelsData)
                {
                    string channelId = channelData.GetString("id");
                    DiscordGuildChannel channel = cache.AddOrUpdate(this, channelId, channelData, () => { return new DiscordGuildChannel(client, this); });
                    cache.SetAlias<DiscordChannel>(channel);
                }
            }

            string ownerId = data.GetString("owner_id");

            // Update members
            IReadOnlyList<DiscordApiData> membersData = data.GetArray("members");
            if (membersData != null)
            {
                foreach (DiscordApiData memberData in membersData)
                {
                    string memberId = memberData.LocateString("user.id");
                    DiscordGuildMember member = cache.AddOrUpdate(this, memberId, memberData, 
                        () => { return new DiscordGuildMember(client, this); });

                    if (ownerId != null && ownerId == memberId)
                        Owner = member;
                }
            }

            // Update voice states
            IReadOnlyList<DiscordApiData> voiceStatesData = data.GetArray("voice_states");
            if (voiceStatesData != null)
            {
                foreach (DiscordApiData voiceStateData in voiceStatesData)
                {
                    DiscordGuildMember member;
                    string memberId = voiceStateData.GetString("user_id");
                    if (cache.TryGet(this, memberId, out member))
                        member.VoiceState.Update(voiceStateData);
                    else
                        DiscordLogger.Default.LogWarning($"[GUILD.UPDATE:{Name}] Failed to locate member with id {memberId}");
                }
            }
        }

        /// <summary>
        /// Returns the name of this guild.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
