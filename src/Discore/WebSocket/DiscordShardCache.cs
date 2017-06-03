using ConcurrentCollections;
using Discore.Voice;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Discore.WebSocket
{
    public class DiscordShardCache
    {
        internal ShardCacheDictionary<MutableUser> Users { get; }
        internal ShardCacheDictionary<MutableDMChannel> DMChannels { get; }

        internal ShardCacheDictionary<MutableGuild> Guilds { get; }
        internal ShardCacheDictionary<DiscordGuildMetadata> GuildMetadata { get; }

        internal ShardCacheDictionary<DiscordGuildChannel> GuildChannels { get; }

        internal ShardNestedCacheDictionary<MutableGuildMember> GuildMembers { get; }
        internal ShardNestedCacheDictionary<DiscordUserPresence> GuildPresences { get; }
        internal ShardNestedCacheDictionary<DiscordVoiceState> GuildVoiceStates { get; }

        internal ConcurrentDictionary<Snowflake, ConcurrentHashSet<Snowflake>> GuildChannelIds { get; }

        ConcurrentHashSet<Snowflake> guildIds;
        ConcurrentHashSet<Snowflake> unavailableGuildIds;

        internal DiscordShardCache()
        {
            guildIds = new ConcurrentHashSet<Snowflake>();
            unavailableGuildIds = new ConcurrentHashSet<Snowflake>();

            GuildChannelIds = new ConcurrentDictionary<Snowflake, ConcurrentHashSet<Snowflake>>();

            Users = new ShardCacheDictionary<MutableUser>();
            DMChannels = new ShardCacheDictionary<MutableDMChannel>();

            Guilds = new ShardCacheDictionary<MutableGuild>();
            GuildMetadata = new ShardCacheDictionary<DiscordGuildMetadata>();

            GuildChannels = new ShardCacheDictionary<DiscordGuildChannel>();

            GuildMembers = new ShardNestedCacheDictionary<MutableGuildMember>();
            GuildPresences = new ShardNestedCacheDictionary<DiscordUserPresence>();
            GuildVoiceStates = new ShardNestedCacheDictionary<DiscordVoiceState>();
        }

        internal void AddGuildId(Snowflake guildId)
        {
            guildIds.Add(guildId);
        }

        internal void RemoveGuildId(Snowflake guildId)
        {
            guildIds.TryRemove(guildId);
            unavailableGuildIds.TryRemove(guildId);
        }

        internal void SetGuildAvailability(Snowflake guildId, bool isAvailable)
        {
            if (isAvailable)
                unavailableGuildIds.TryRemove(guildId);
            else
                unavailableGuildIds.Add(guildId);
        }

        internal void AddGuildChannel(DiscordGuildChannel guildChannel)
        {
            GuildChannels[guildChannel.Id] = guildChannel;

            ConcurrentHashSet<Snowflake> guildChannelsIdSet;
            if (!GuildChannelIds.TryGetValue(guildChannel.GuildId, out guildChannelsIdSet))
            {
                guildChannelsIdSet = new ConcurrentHashSet<Snowflake>();
                GuildChannelIds[guildChannel.GuildId] = guildChannelsIdSet;
            }

            guildChannelsIdSet.Add(guildChannel.Id);
        }

        internal void RemoveGuildChannel(Snowflake guildId, Snowflake guildChannelId)
        {
            GuildChannels.TryRemove(guildChannelId, out _);

            if (GuildChannelIds.TryGetValue(guildId, out ConcurrentHashSet<Snowflake> guildChannelsIdSet))
                guildChannelsIdSet.TryRemove(guildChannelId);
        }

        internal void ClearGuildChannels(Snowflake guildId)
        {
            GuildChannelIds.TryRemove(guildId, out _);
        }

        public bool IsGuildAvailable(Snowflake guildId)
        {
            return !unavailableGuildIds.Contains(guildId);
        }

        public IEnumerable<Snowflake> GetAllGuildIds()
        {
            List<Snowflake> ids = new List<Snowflake>(guildIds.Count);
            foreach (Snowflake id in guildIds)
                ids.Add(id);

            return ids;
        }

        public IEnumerable<Snowflake> GetUnavailableGuildIds()
        {
            List<Snowflake> ids = new List<Snowflake>(unavailableGuildIds.Count);
            foreach (Snowflake id in unavailableGuildIds)
                ids.Add(id);

            return ids;
        }

        public DiscordGuildMetadata GetGuildMetadata(Snowflake guildId)
        {
            return GuildMetadata[guildId];
        }

        public DiscordGuild GetGuild(Snowflake guildId)
        {
            return Guilds[guildId]?.ImmutableEntity;
        }

        public DiscordUser GetUser(Snowflake userId)
        {
            return Users[userId]?.ImmutableEntity;
        }

        public DiscordChannel GetChannel(Snowflake channelId)
        {
            DiscordGuildChannel guildChannel = GuildChannels[channelId];
            if (guildChannel != null)
                return guildChannel;
            else
                return DMChannels[channelId]?.ImmutableEntity;
        }

        public DiscordDMChannel GetDMChannel(Snowflake dmChannelId)
        {
            return DMChannels[dmChannelId]?.ImmutableEntity;
        }

        public IEnumerable<DiscordGuildChannel> GetGuildChannels(Snowflake guildId)
        {
            if (GuildChannelIds.TryGetValue(guildId, out ConcurrentHashSet<Snowflake> guildChannelsIdSet))
            {
                List<DiscordGuildChannel> guildChannels = new List<DiscordGuildChannel>();
                foreach (Snowflake guildChannelId in guildChannelsIdSet)
                {
                    DiscordChannel channel = GuildChannels[guildChannelId];

                    // Channel should always be a guild channel, but in the very unlikely event
                    // that the ID does get mismatched, ensure that we are not inserting null
                    // into this list.
                    if (channel is DiscordGuildChannel guildChannel)
                        guildChannels.Add(guildChannel);
                }

                return guildChannels;
            }
            else
                return new DiscordGuildChannel[0];
        }

        public DiscordGuildTextChannel GetGuildTextChannel(Snowflake guildTextChannelId)
        {
            return GuildChannels[guildTextChannelId] as DiscordGuildTextChannel;
        }

        public DiscordGuildVoiceChannel GetGuildVoiceChannel(Snowflake guildVoiceChannelId)
        {
            return GuildChannels[guildVoiceChannelId] as DiscordGuildVoiceChannel;
        }

        public DiscordGuildMember GetGuildMember(Snowflake guildId, Snowflake userId)
        {
            return GuildMembers[guildId, userId]?.ImmutableEntity;
        }

        public IEnumerable<DiscordGuildMember> GetGuildMembers(Snowflake guildId)
        {
            return GuildMembers.GetValues(guildId).Select(x => x.ImmutableEntity);
        }

        public DiscordUserPresence GetUserPresence(Snowflake guildId, Snowflake userId)
        {
            return GuildPresences[guildId, userId];
        }

        public IEnumerable<DiscordUserPresence> GetUserPresences(Snowflake guildId)
        {
            return GuildPresences.GetValues(guildId);
        }

        public DiscordVoiceState GetVoiceState(Snowflake guildId, Snowflake userId)
        {
            return GuildVoiceStates[guildId, userId];
        }

        public IEnumerable<DiscordVoiceState> GetVoiceStates(Snowflake guildId)
        {
            return GuildVoiceStates.GetValues(guildId);
        }

        public void Clear()
        {
            guildIds.Clear();
            unavailableGuildIds.Clear();

            GuildChannelIds.Clear();

            Users.Clear();
            DMChannels.Clear();

            Guilds.Clear();
            GuildMetadata.Clear();

            GuildChannels.Clear();

            GuildMembers.Clear();
            GuildPresences.Clear();
            GuildVoiceStates.Clear();
        }
    }
}
