using ConcurrentCollections;
using Discore.Voice;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Discore.WebSocket
{
    /// <summary>
    /// A set of cached entity data for a Discord shard connection.
    /// </summary>
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

        /// <summary>
        /// Returns whether the specified guild is available or false if the guild is not known to this cache.
        /// </summary>
        public bool IsGuildAvailable(Snowflake guildId)
        {
            return !unavailableGuildIds.Contains(guildId);
        }

        /// <summary>
        /// Returns a list of the IDs of all guilds currently in this cache.
        /// </summary>
        public IReadOnlyList<Snowflake> GetAllGuildIds()
        {
            List<Snowflake> ids = new List<Snowflake>(guildIds.Count);
            foreach (Snowflake id in guildIds)
                ids.Add(id);

            return ids;
        }

        /// <summary>
        /// Returns a list of the IDs of all unavailable guilds currently in this cache.
        /// </summary>
        public IReadOnlyList<Snowflake> GetUnavailableGuildIds()
        {
            List<Snowflake> ids = new List<Snowflake>(unavailableGuildIds.Count);
            foreach (Snowflake id in unavailableGuildIds)
                ids.Add(id);

            return ids;
        }

        /// <summary>
        /// Returns the shard-specific metdata for the given guild or null if the guild is not currently cached.
        /// </summary>
        public DiscordGuildMetadata GetGuildMetadata(Snowflake guildId)
        {
            return GuildMetadata[guildId];
        }

        /// <summary>
        /// Returns the specified guild or null if it is not currently cached.
        /// </summary>
        public DiscordGuild GetGuild(Snowflake guildId)
        {
            return Guilds[guildId]?.ImmutableEntity;
        }

        /// <summary>
        /// Returns the specified user or null if they are not currently cached.
        /// </summary>
        public DiscordUser GetUser(Snowflake userId)
        {
            return Users[userId]?.ImmutableEntity;
        }

        /// <summary>
        /// Returns the specified channel or null if it is not currently cached.
        /// </summary>
        public DiscordChannel GetChannel(Snowflake channelId)
        {
            DiscordGuildChannel guildChannel = GuildChannels[channelId];
            if (guildChannel != null)
                return guildChannel;
            else
                return DMChannels[channelId]?.ImmutableEntity;
        }

        /// <summary>
        /// Returns the specified DM channel or, null if it is not currently cached or is not a DM channel.
        /// </summary>
        public DiscordDMChannel GetDMChannel(Snowflake dmChannelId)
        {
            return DMChannels[dmChannelId]?.ImmutableEntity;
        }

        /// <summary>
        /// Returns a list of all channels in the given guild or null if the guild is not currently cached.
        /// </summary>
        public IReadOnlyList<DiscordGuildChannel> GetGuildChannels(Snowflake guildId)
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

        /// <summary>
        /// Returns the specified guild text channel or, null if it is not currently cached or is not a guild text channel.
        /// </summary>
        public DiscordGuildTextChannel GetGuildTextChannel(Snowflake guildTextChannelId)
        {
            return GuildChannels[guildTextChannelId] as DiscordGuildTextChannel;
        }

        /// <summary>
        /// Returns the specified guild voice channel or, null if it is not currently cached or is not a guild voice channel.
        /// </summary>
        public DiscordGuildVoiceChannel GetGuildVoiceChannel(Snowflake guildVoiceChannelId)
        {
            return GuildChannels[guildVoiceChannelId] as DiscordGuildVoiceChannel;
        }

        /// <summary>
        /// Returns the specified guild category channel or, 
        /// null if it is not currently cached or is not a guild category channel.
        /// </summary>
        public DiscordGuildCategoryChannel GetGuildCategoryChannel(Snowflake guildCategoryChannelId)
        {
            return GuildChannels[guildCategoryChannelId] as DiscordGuildCategoryChannel;
        }

        /// <summary>
        /// Returns the specified guild news channel or, 
        /// null if it is not currently cached or is not a guild news channel.
        /// </summary>
        public DiscordGuildNewsChannel GetGuildNewsChannel(Snowflake guildNewsChannelId)
        {
            return GuildChannels[guildNewsChannelId] as DiscordGuildNewsChannel;
        }

        /// <summary>
        /// Returns the specified guild store channel or, 
        /// null if it is not currently cached or is not a guild store channel.
        /// </summary>
        public DiscordGuildStoreChannel GetGuildStoreChannel(Snowflake guildStoreChannelId)
        {
            return GuildChannels[guildStoreChannelId] as DiscordGuildStoreChannel;
        }

        /// <summary>
        /// Returns the specified guild member or,
        /// null if the member is not currently cached or the guild is not currently cached.
        /// </summary>
        public DiscordGuildMember GetGuildMember(Snowflake guildId, Snowflake userId)
        {
            return GuildMembers[guildId, userId]?.ImmutableEntity;
        }

        /// <summary>
        /// Returns a list of all currently cached members for the given guild, or null if the guild is not currently cached.
        /// </summary>
        public IReadOnlyList<DiscordGuildMember> GetGuildMembers(Snowflake guildId)
        {
            return GuildMembers.GetValues(guildId)?.Select(x => x.ImmutableEntity).ToList();
        }

        /// <summary>
        /// Returns the presence for the specified user or, 
        /// null if the presence is not currently cached or the guild is not currently cached.
        /// </summary>
        public DiscordUserPresence GetUserPresence(Snowflake guildId, Snowflake userId)
        {
            return GuildPresences[guildId, userId];
        }

        /// <summary>
        /// Returns a list of all currently cached user presences for the given guild, 
        /// or null if the guild is not currently cached.
        /// </summary>
        public IReadOnlyList<DiscordUserPresence> GetUserPresences(Snowflake guildId)
        {
            return GuildPresences.GetValues(guildId)?.ToList();
        }

        /// <summary>
        /// Returns the voice state for the specified user or, 
        /// null if the voice state is not currently cached or the guild is not currently cached.
        /// </summary>
        public DiscordVoiceState GetVoiceState(Snowflake guildId, Snowflake userId)
        {
            return GuildVoiceStates[guildId, userId];
        }

        /// <summary>
        /// Returns a list of all currently cached voice states for the given guild, 
        /// or null if the guild is not currently cached.
        /// </summary>
        public IReadOnlyList<DiscordVoiceState> GetVoiceStates(Snowflake guildId)
        {
            return GuildVoiceStates.GetValues(guildId)?.ToList();
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
