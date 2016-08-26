using System.Collections.Generic;

namespace Discore
{
    public class DiscordGuild : IDiscordObject, ICacheable
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Icon { get; private set; }
        public string Splash { get; private set; }
        public DiscordGuildMember Owner { get; private set; }
        public string Region { get; private set; }
        public string AfkChannelId { get; private set; }
        public int AfkTimeout { get; private set; }
        public bool EmbedEnabled { get; private set; }
        public string EmbedChannelId { get; private set; }
        public int VerificationLevel { get; private set; }
        public string[] Features { get; private set; }
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

        public IReadOnlyList<KeyValuePair<string, DiscordGuildChannel>> Channels
        {
            get { return cache.GetList<DiscordGuildChannel, DiscordGuild>(this); }
        }
        public IReadOnlyList<KeyValuePair<string, DiscordEmoji>> Emojis
        {
            get { return cache.GetList<DiscordEmoji, DiscordGuild>(this); }
        }
        public IReadOnlyList<KeyValuePair<string, DiscordRole>> Roles
        {
            get { return cache.GetList<DiscordRole, DiscordGuild>(this); }
        }
        public IReadOnlyList<KeyValuePair<string, DiscordGuildMember>> Members
        {
            get { return cache.GetList<DiscordGuildMember, DiscordGuild>(this); }
        }

        string atEveryoneRoleId;

        IDiscordClient client;
        DiscordApiCache cache;

        public DiscordGuild(IDiscordClient client)
        {
            this.client = client;
            cache = client.Cache;
        }

        public bool TryGetChannel(string id, out DiscordGuildChannel channel)
        {
            return cache.TryGet(this, id, out channel);
        }

        public bool TryGetEmoji(string id, out DiscordEmoji emoji)
        {
            return cache.TryGet(this, id, out emoji);
        }

        public bool TryGetRole(string id, out DiscordRole role)
        {
            return cache.TryGet(this, id, out role);
        }

        public bool TryGetMember(string id, out DiscordGuildMember member)
        {
            return cache.TryGet(this, id, out member);
        }

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

        public override string ToString()
        {
            return Name;
        }
    }
}
