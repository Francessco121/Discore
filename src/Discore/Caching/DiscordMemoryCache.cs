using ConcurrentCollections;
using Discore.Voice;
using Discore.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Discore.Caching
{
    /// <summary>
    /// A memory cache of Discord entities for a single <see cref="Shard"/>.
    /// </summary>
    public class DiscordMemoryCache : IDisposable
    {
        readonly CacheDictionary<MutableUser> users;

        readonly CacheDictionary<MutableGuild> guilds;
        readonly CacheDictionary<DiscordGuildMetadata> guildMetadata;

        readonly CacheDictionary<DiscordGuildChannel> guildChannels;

        readonly NestedCacheDictionary<MutableGuildMember> guildMembers;
        readonly NestedCacheDictionary<DiscordUserPresence> guildPresences;
        readonly NestedCacheDictionary<DiscordVoiceState> guildVoiceStates;

        readonly ConcurrentDictionary<Snowflake, ConcurrentHashSet<Snowflake>> guildChannelIds;

        readonly ConcurrentHashSet<Snowflake> guildIds;
        readonly ConcurrentHashSet<Snowflake> unavailableGuildIds;

        readonly ConcurrentDictionary<Snowflake, ConcurrentHashSet<Snowflake>> voiceChannelUsers;

        readonly Shard shard;
        readonly DiscoreLogger logger;

        /// <summary>
        /// Creates a new Discord entity memory cache.
        /// </summary>
        /// <param name="shard">The shard to cache entities from.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="shard"/> is null.</exception>
        public DiscordMemoryCache(Shard shard)
        {
            this.shard = shard ?? throw new ArgumentNullException(nameof(shard));

            logger = new DiscoreLogger($"DiscordMemoryCache#{shard.Id}");

            // Set up stores
            guildIds = new ConcurrentHashSet<Snowflake>();
            unavailableGuildIds = new ConcurrentHashSet<Snowflake>();

            guildChannelIds = new ConcurrentDictionary<Snowflake, ConcurrentHashSet<Snowflake>>();

            users = new CacheDictionary<MutableUser>();

            guilds = new CacheDictionary<MutableGuild>();
            guildMetadata = new CacheDictionary<DiscordGuildMetadata>();

            guildChannels = new CacheDictionary<DiscordGuildChannel>();

            guildMembers = new NestedCacheDictionary<MutableGuildMember>();
            guildPresences = new NestedCacheDictionary<DiscordUserPresence>();
            guildVoiceStates = new NestedCacheDictionary<DiscordVoiceState>();

            voiceChannelUsers = new ConcurrentDictionary<Snowflake, ConcurrentHashSet<Snowflake>>();

            // Set up shard events
            shard.OnReconnected += Shard_OnReconnected;
            shard.OnDisconnected += Shard_OnDisconnected;

            IDiscordGateway gateway = shard.Gateway;
            gateway.OnReady += Gateway_OnReady;
            gateway.OnGuildCreate += Gateway_OnGuildCreated;
            gateway.OnGuildUpdate += Gateway_OnGuildUpdated;
            gateway.OnGuildDelete += Gateway_OnGuildDeleted;
            gateway.OnGuildBanAdd += Gateway_OnGuildBanAdded;
            gateway.OnGuildBanRemove += Gateway_OnGuildBanRemoved;
            gateway.OnGuildEmojisUpdate += Gateway_OnGuildEmojisUpdated;
            gateway.OnGuildMemberAdd += Gateway_OnGuildMemberAdded;
            gateway.OnGuildMemberRemove += Gateway_OnGuildMemberRemoved;
            gateway.OnGuildMemberUpdate += Gateway_OnGuildMemberUpdated;
            gateway.OnGuildMembersChunk += Gateway_OnGuildMembersChunk;
            gateway.OnGuildRoleCreate += Gateway_OnGuildRoleCreated;
            gateway.OnGuildRoleUpdate += Gateway_OnGuildRoleUpdated;
            gateway.OnGuildRoleDelete += Gateway_OnGuildRoleDeleted;
            gateway.OnChannelCreate += Gateway_OnChannelCreated;
            gateway.OnChannelUpdate += Gateway_OnChannelUpdated;
            gateway.OnChannelDelete += Gateway_OnChannelDeleted;
            gateway.OnMessageCreate += Gateway_OnMessageCreated;
            gateway.OnMessageUpdate += Gateway_OnMessageUpdated;
            gateway.OnPresenceUpdate += Gateway_OnPresenceUpdated;
            gateway.OnUserUpdate += Gateway_OnUserUpdated;
            gateway.OnVoiceStateUpdate += Gateway_OnVoiceStateUpdated;
        }

        /// <summary>
        /// Clears the cache and unsubscribes from shard and Gateway events.
        /// </summary>
        public void Dispose()
        {
            // Unsubscribe from shard events
            shard.OnReconnected -= Shard_OnReconnected;
            shard.OnDisconnected -= Shard_OnDisconnected;

            // Unsubscribe from Gateway events
            IDiscordGateway gateway = shard.Gateway;
            gateway.OnReady -= Gateway_OnReady;
            gateway.OnGuildCreate -= Gateway_OnGuildCreated;
            gateway.OnGuildUpdate -= Gateway_OnGuildUpdated;
            gateway.OnGuildDelete -= Gateway_OnGuildDeleted;
            gateway.OnGuildBanAdd -= Gateway_OnGuildBanAdded;
            gateway.OnGuildBanRemove -= Gateway_OnGuildBanRemoved;
            gateway.OnGuildEmojisUpdate -= Gateway_OnGuildEmojisUpdated;
            gateway.OnGuildMemberAdd -= Gateway_OnGuildMemberAdded;
            gateway.OnGuildMemberRemove -= Gateway_OnGuildMemberRemoved;
            gateway.OnGuildMemberUpdate -= Gateway_OnGuildMemberUpdated;
            gateway.OnGuildMembersChunk -= Gateway_OnGuildMembersChunk;
            gateway.OnGuildRoleCreate -= Gateway_OnGuildRoleCreated;
            gateway.OnGuildRoleUpdate -= Gateway_OnGuildRoleUpdated;
            gateway.OnGuildRoleDelete -= Gateway_OnGuildRoleDeleted;
            gateway.OnChannelCreate -= Gateway_OnChannelCreated;
            gateway.OnChannelUpdate -= Gateway_OnChannelUpdated;
            gateway.OnChannelDelete -= Gateway_OnChannelDeleted;
            gateway.OnMessageCreate -= Gateway_OnMessageCreated;
            gateway.OnMessageUpdate -= Gateway_OnMessageUpdated;
            gateway.OnPresenceUpdate -= Gateway_OnPresenceUpdated;
            gateway.OnUserUpdate -= Gateway_OnUserUpdated;
            gateway.OnVoiceStateUpdate -= Gateway_OnVoiceStateUpdated;

            // Clear cache
            Clear();
        }

        private void Shard_OnReconnected(object sender, ShardReconnectedEventArgs e)
        {
            if (e.IsNewSession)
            {
                logger.LogVerbose("New shard session, clearing cache...");

                // Shard started a new session, clear cache since the gateway isn't going to repeat
                // any missing events and the cache is no longer guaranteed to be valid
                Clear();
            }
        }

        private void Shard_OnDisconnected(object sender, ShardEventArgs e)
        {
            logger.LogVerbose("Shard disconnected, clearing cache...");

            // Shard disconnected, clear cache since its no longer guaranteed to be valid
            Clear();
        }

        private void Gateway_OnReady(object sender, ReadyEventArgs e)
        {
            logger.LogVerbose("Gateway ready, caching initial data...");

            // Cache user
            CacheUser(e.User);

            // Cache unavailable guilds
            foreach (Snowflake id in e.GuildIds)
            {
                guildIds.Add(id);
                unavailableGuildIds.Add(id);
            }
        }

        private void Gateway_OnGuildCreated(object sender, GuildCreateEventArgs e)
        {
            // Cache guild
            CacheGuild(e.Guild);
            guildIds.Add(e.Guild.Id);

            // Cache metadata
            guildMetadata[e.Guild.Id] = e.GuildMetadata;

            // Cache guild members
            guildMembers.Clear(e.Guild.Id);

            foreach (DiscordGuildMember member in e.Members)
            {
                CacheGuildMember(member, e.Guild.Id);
            }

            // Cache guild channels
            if (guildChannelIds.TryRemove(e.Guild.Id, out ConcurrentHashSet<Snowflake>? channelIds))
            {
                foreach (Snowflake oldChannelId in channelIds)
                    guildChannels.TryRemove(oldChannelId, out _);
            }

            foreach (DiscordGuildChannel channel in e.Channels)
            {
                CacheGuildChannel(channel);
            }

            // Cache voice states
            guildVoiceStates.Clear(e.Guild.Id);

            foreach (DiscordVoiceState state in e.VoiceStates)
            {
                CacheVoiceState(state);
            }

            // Cache presences
            guildPresences.Clear(e.Guild.Id);

            foreach (DiscordUserPresence presence in e.Presences)
            {
                if (users.TryGetValue(presence.User.Id, out MutableUser? mutableUser))
                {
                    mutableUser.PartialUpdate(presence.User);
                }

                guildPresences[e.Guild.Id, presence.User.Id] = presence;
            }

            // Mark guild as available
            unavailableGuildIds.TryRemove(e.Guild.Id);
        }

        private void Gateway_OnGuildUpdated(object sender, GuildUpdateEventArgs e)
        {
            // Cache guild
            CacheGuild(e.Guild);
        }

        private void Gateway_OnGuildDeleted(object sender, GuildDeleteEventArgs e)
        {
            if (e.Unavailable)
            {
                // Mark the guild as no longer available
                unavailableGuildIds.Add(e.GuildId);
            }
            else
            {
                // Fully remove guild from cache
                guildIds.TryRemove(e.GuildId);
            }

            // Clear all cache data related to the guild
            guildMetadata.TryRemove(e.GuildId, out _);
            guildMembers.RemoveParent(e.GuildId);
            guildPresences.RemoveParent(e.GuildId);
            guildVoiceStates.RemoveParent(e.GuildId);

            if (guildChannelIds.TryRemove(e.GuildId, out ConcurrentHashSet<Snowflake>? channelIds))
            {
                foreach (Snowflake channelId in channelIds)
                {
                    guildChannels.TryRemove(channelId, out _);
                    voiceChannelUsers.TryRemove(channelId, out _);
                }
            }

            // Remove guild from cache
            unavailableGuildIds.TryRemove(e.GuildId);

            if (guilds.TryRemove(e.GuildId, out MutableGuild? mutableGuild))
            {
                // Ensure all references are cleared
                mutableGuild.ClearReferences();
            }
        }

        private void Gateway_OnGuildBanAdded(object sender, GuildBanAddEventArgs e)
        {
            // Cache user
            CacheUser(e.User);
        }

        private void Gateway_OnGuildBanRemoved(object sender, GuildBanRemoveEventArgs e)
        {
            // Cache user
            CacheUser(e.User);
        }

        private void Gateway_OnGuildEmojisUpdated(object sender, GuildEmojisUpdateEventArgs e)
        {
            // Cache new emojis
            if (guilds.TryGetValue(e.GuildId, out MutableGuild? mutableGuild))
            {
                // Clear existing emojis
                mutableGuild.Emojis.Clear();

                // Set new emojis
                foreach (DiscordEmoji emoji in e.Emojis)
                    mutableGuild.Emojis[emoji.Id] = emoji;

                // Dirty the guild
                mutableGuild.Dirty();
            }
        }

        private void Gateway_OnGuildMemberAdded(object sender, GuildMemberAddEventArgs e)
        {
            // Cache member
            CacheGuildMember(e.Member, e.GuildId);
        }

        private void Gateway_OnGuildMemberRemoved(object sender, GuildMemberRemoveEventArgs e)
        {
            // Cache user
            CacheUser(e.User);

            if (guildMembers.TryRemove(e.GuildId, e.User.Id, out MutableGuildMember? mutableMember))
            {
                // Ensure all references are removed
                mutableMember.ClearReferences();
            }
        }

        private void Gateway_OnGuildMemberUpdated(object sender, GuildMemberUpdateEventArgs e)
        {
            // Cache user
            CacheUser(e.PartialMember.User);

            // Get mutable member
            if (guildMembers.TryGetValue(e.GuildId, e.PartialMember.Id, out MutableGuildMember? mutableMember))
            {
                // Update member
                mutableMember.PartialUpdate(e.PartialMember);
            }

            // It is technically valid for the member to not exist here, especially if the guild is considered large.
        }

        private void Gateway_OnGuildMembersChunk(object sender, GuildMemberChunkEventArgs e)
        {
            // Cache guild members
            foreach (DiscordGuildMember member in e.Members)
            {
                CacheGuildMember(member, e.GuildId);
            }
        }

        private void Gateway_OnGuildRoleCreated(object sender, GuildRoleCreateEventArgs e)
        {
            // Update guild roles
            if (guilds.TryGetValue(e.GuildId, out MutableGuild? guild))
            {
                guild.Roles[e.Role.Id] = e.Role;
                guild.Dirty();
            }
        }

        private void Gateway_OnGuildRoleUpdated(object sender, GuildRoleUpdateEventArgs e)
        {
            // Update guild roles
            if (guilds.TryGetValue(e.GuildId, out MutableGuild? guild))
            {
                guild.Roles[e.Role.Id] = e.Role;
                guild.Dirty();
            }
        }

        private void Gateway_OnGuildRoleDeleted(object sender, GuildRoleDeleteEventArgs e)
        {
            // Update guild roles
            if (guilds.TryGetValue(e.GuildId, out MutableGuild? guild))
            {
                guild.Roles.TryRemove(e.RoleId, out _);
                guild.Dirty();
            }
        }

        private void Gateway_OnChannelCreated(object sender, ChannelCreateEventArgs e)
        {
            if (e.Channel is DiscordGuildChannel guildChannel)
            {
                // Cache guild channel
                guildChannels[e.Channel.Id] = guildChannel;
            }
        }

        private void Gateway_OnChannelUpdated(object sender, ChannelUpdateEventArgs e)
        {
            if (e.Channel is DiscordGuildChannel guildChannel)
            {
                // Cache guild channel
                guildChannels[e.Channel.Id] = guildChannel;
            }
        }

        private void Gateway_OnChannelDeleted(object sender, ChannelDeleteEventArgs e)
        {
            if (e.Channel is DiscordGuildChannel guildChannel)
            {
                // Remove guild channel
                if (guildChannelIds.TryGetValue(guildChannel.GuildId, out ConcurrentHashSet<Snowflake> channelIds))
                    channelIds.TryRemove(guildChannel.Id);

                guildChannels.TryRemove(guildChannel.Id, out _);

                // Remove associated voice users
                if (guildChannel.ChannelType == DiscordChannelType.GuildVoice)
                    voiceChannelUsers.TryRemove(guildChannel.Id, out _);
            }
        }

        private void Gateway_OnMessageCreated(object sender, MessageCreateEventArgs e)
        {
            // Cache author user
            CacheUser(e.Message.Author);

            // Cache mentioned users
            foreach (DiscordUser user in e.Message.Mentions)
            {
                CacheUser(user);
            }
        }

        private void Gateway_OnMessageUpdated(object sender, MessageUpdateEventArgs e)
        {
            // Cache author user
            if (e.PartialMessage.Author != null)
            {
                CacheUser(e.PartialMessage.Author);
            }

            // Cache mentioned users
            if (e.PartialMessage.Mentions != null)
            {
                foreach (DiscordUser user in e.PartialMessage.Mentions)
                {
                    CacheUser(user);
                }
            }
        }

        private void Gateway_OnPresenceUpdated(object sender, PresenceUpdateEventArgs e)
        {
            // Cache user
            if (users.TryGetValue(e.Presence.User.Id, out MutableUser? mutableUser))
            {
                mutableUser.PartialUpdate(e.Presence.User);
            }

            // Cache presence
            guildPresences[e.GuildId, e.Presence.User.Id] = e.Presence;
        }

        private void Gateway_OnUserUpdated(object sender, UserUpdateEventArgs e)
        {
            // Cache user
            CacheUser(e.User);
        }

        private void Gateway_OnVoiceStateUpdated(object sender, VoiceStateUpdateEventArgs e)
        {
            // Cache voice state
            CacheVoiceState(e.VoiceState);
        }

        void CacheVoiceState(DiscordVoiceState newState)
        {
            if (newState.GuildId == null)
                return;

            // Save previous state
            DiscordVoiceState? previousState;
            guildVoiceStates.TryGetValue(newState.GuildId.Value, newState.UserId, out previousState);

            // Update cache with new state
            guildVoiceStates[newState.GuildId.Value, newState.UserId] = newState;

            // If previously in a voice channel that differs from the new channel (or no longer in a channel),
            // then remove this user from the voice channel user list.
            if (previousState != null && previousState.ChannelId.HasValue && previousState.ChannelId != newState.ChannelId)
            {
                if (voiceChannelUsers.TryGetValue(previousState.ChannelId.Value, out ConcurrentHashSet<Snowflake>? userList))
                    userList.TryRemove(newState.UserId);
            }

            // If user is now in a voice channel, add them to the user list.
            if (newState.ChannelId.HasValue)
            {
                Snowflake voiceChannelId = newState.ChannelId.Value;

                ConcurrentHashSet<Snowflake>? userList;
                if (!voiceChannelUsers.TryGetValue(voiceChannelId, out userList))
                {
                    userList = new ConcurrentHashSet<Snowflake>();
                    voiceChannelUsers[voiceChannelId] = userList;
                }

                userList.Add(newState.UserId);
            }
        }

        void CacheGuildChannel(DiscordGuildChannel guildChannel)
        {
            guildChannels[guildChannel.Id] = guildChannel;

            ConcurrentHashSet<Snowflake>? guildChannelsIdSet;
            if (!guildChannelIds.TryGetValue(guildChannel.GuildId, out guildChannelsIdSet))
            {
                guildChannelsIdSet = new ConcurrentHashSet<Snowflake>();
                guildChannelIds[guildChannel.GuildId] = guildChannelsIdSet;
            }

            guildChannelsIdSet.Add(guildChannel.Id);
        }

        MutableGuildMember CacheGuildMember(DiscordGuildMember member, Snowflake guildId)
        {
            // Cache user first
            MutableUser user = CacheUser(member.User);

            // Get member
            MutableGuildMember? mutableMember;
            if (!guildMembers.TryGetValue(guildId, user.Id, out mutableMember))
            {
                // Create member
                mutableMember = new MutableGuildMember(user, guildId);
                guildMembers[guildId, user.Id] = mutableMember;
            }

            // Update
            mutableMember.Update(member);

            return mutableMember;
        }

        MutableGuild CacheGuild(DiscordGuild guild)
        {
            // Get guild
            MutableGuild? mutableGuild;
            if (!guilds.TryGetValue(guild.Id, out mutableGuild))
            {
                // Create guild
                mutableGuild = new MutableGuild(guild.Id);
                guilds[guild.Id] = mutableGuild;
            }

            // Update guild
            mutableGuild.Update(guild);

            return mutableGuild;
        }

        MutableUser CacheUser(DiscordUser user)
        {
            // Get user
            MutableUser? mutableUser;
            if (!users.TryGetValue(user.Id, out mutableUser))
            {
                // Create user
                mutableUser = new MutableUser(user.Id, user.IsWebhookUser);
                users[user.Id] = mutableUser;
            }

            // Update user
            mutableUser.Update(user);

            return mutableUser;
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
            var ids = new List<Snowflake>(guildIds.Count);
            foreach (Snowflake id in guildIds)
                ids.Add(id);

            return ids;
        }

        /// <summary>
        /// Returns a list of the IDs of all unavailable guilds currently in this cache.
        /// </summary>
        public IReadOnlyList<Snowflake> GetUnavailableGuildIds()
        {
            var ids = new List<Snowflake>(unavailableGuildIds.Count);
            foreach (Snowflake id in unavailableGuildIds)
                ids.Add(id);

            return ids;
        }

        /// <summary>
        /// Returns the shard-specific metdata for the given guild or null if the guild is not currently cached.
        /// </summary>
        public DiscordGuildMetadata? GetGuildMetadata(Snowflake guildId)
        {
            return guildMetadata[guildId];
        }

        /// <summary>
        /// Returns the specified guild or null if it is not currently cached.
        /// </summary>
        public DiscordGuild? GetGuild(Snowflake guildId)
        {
            return guilds[guildId]?.ImmutableEntity;
        }

        /// <summary>
        /// Returns the specified user or null if they are not currently cached.
        /// </summary>
        public DiscordUser? GetUser(Snowflake userId)
        {
            return users[userId]?.ImmutableEntity;
        }

        /// <summary>
        /// Returns a list of all channels in the given guild or null if the guild is not currently cached.
        /// </summary>
        public IReadOnlyList<DiscordGuildChannel>? GetGuildChannels(Snowflake guildId)
        {
            if (guildChannelIds.TryGetValue(guildId, out ConcurrentHashSet<Snowflake>? guildChannelsIdSet))
            {
                var guildChannels = new List<DiscordGuildChannel>();
                foreach (Snowflake guildChannelId in guildChannelsIdSet)
                {
                    DiscordChannel? channel = this.guildChannels[guildChannelId];

                    // Channel should always be a guild channel, but in the very unlikely event
                    // that the ID does get mismatched, ensure that we are not inserting null
                    // into this list.
                    if (channel is DiscordGuildChannel guildChannel)
                        guildChannels.Add(guildChannel);
                }

                return guildChannels;
            }
            else
                return null;
        }

        /// <summary>
        /// Returns the specified guild channel or null if it is not currently cached.
        /// </summary>
        public DiscordGuildChannel? GetGuildChannel(Snowflake guildChannelId)
        {
            return guildChannels[guildChannelId];
        }

        /// <summary>
        /// Returns the specified guild text channel or, null if it is not currently cached or is not a guild text channel.
        /// </summary>
        public DiscordGuildTextChannel? GetGuildTextChannel(Snowflake guildTextChannelId)
        {
            return guildChannels[guildTextChannelId] as DiscordGuildTextChannel;
        }

        /// <summary>
        /// Returns the specified guild voice channel or, null if it is not currently cached or is not a guild voice channel.
        /// </summary>
        public DiscordGuildVoiceChannel? GetGuildVoiceChannel(Snowflake guildVoiceChannelId)
        {
            return guildChannels[guildVoiceChannelId] as DiscordGuildVoiceChannel;
        }

        /// <summary>
        /// Returns the specified guild category channel or, 
        /// null if it is not currently cached or is not a guild category channel.
        /// </summary>
        public DiscordGuildCategoryChannel? GetGuildCategoryChannel(Snowflake guildCategoryChannelId)
        {
            return guildChannels[guildCategoryChannelId] as DiscordGuildCategoryChannel;
        }

        /// <summary>
        /// Returns the specified guild news channel or, 
        /// null if it is not currently cached or is not a guild news channel.
        /// </summary>
        public DiscordGuildNewsChannel? GetGuildNewsChannel(Snowflake guildNewsChannelId)
        {
            return guildChannels[guildNewsChannelId] as DiscordGuildNewsChannel;
        }

        /// <summary>
        /// Returns the specified guild store channel or, 
        /// null if it is not currently cached or is not a guild store channel.
        /// </summary>
        public DiscordGuildStoreChannel? GetGuildStoreChannel(Snowflake guildStoreChannelId)
        {
            return guildChannels[guildStoreChannelId] as DiscordGuildStoreChannel;
        }

        /// <summary>
        /// Returns the specified guild member or,
        /// null if the member is not currently cached or the guild is not currently cached.
        /// </summary>
        public DiscordGuildMember? GetGuildMember(Snowflake guildId, Snowflake userId)
        {
            return guildMembers[guildId, userId]?.ImmutableEntity;
        }

        /// <summary>
        /// Returns a list of all currently cached members for the given guild, or null if the guild is not currently cached.
        /// </summary>
        public IReadOnlyList<DiscordGuildMember>? GetGuildMembers(Snowflake guildId)
        {
            return guildMembers.GetValues(guildId)?.Select(x => x.ImmutableEntity).ToList();
        }

        /// <summary>
        /// Returns the presence for the specified user or, 
        /// null if the presence is not currently cached or the guild is not currently cached.
        /// </summary>
        public DiscordUserPresence? GetUserPresence(Snowflake guildId, Snowflake userId)
        {
            return guildPresences[guildId, userId];
        }

        /// <summary>
        /// Returns a list of all currently cached user presences for the given guild, 
        /// or null if the guild is not currently cached.
        /// </summary>
        public IReadOnlyList<DiscordUserPresence>? GetUserPresences(Snowflake guildId)
        {
            return guildPresences.GetValues(guildId)?.ToList();
        }

        /// <summary>
        /// Returns the voice state for the specified user or, 
        /// null if the voice state is not currently cached or the guild is not currently cached.
        /// </summary>
        public DiscordVoiceState? GetVoiceState(Snowflake guildId, Snowflake userId)
        {
            return guildVoiceStates[guildId, userId];
        }

        /// <summary>
        /// Returns a list of all currently cached voice states for the given guild, 
        /// or null if the guild is not currently cached.
        /// </summary>
        public IReadOnlyList<DiscordVoiceState>? GetVoiceStates(Snowflake guildId)
        {
            return guildVoiceStates.GetValues(guildId)?.ToList();
        }

        /// <summary>
        /// Gets a list of the IDs of every user currently in the specified voice channel.
        /// <para>Note: Will return an empty list if the voice channel is not cached.</para>
        /// </summary>
        public IReadOnlyList<Snowflake> GetUsersInVoiceChannel(Snowflake voiceChannelId)
        {
            if (voiceChannelUsers.TryGetValue(voiceChannelId, out ConcurrentHashSet<Snowflake>? userIds))
            {
                var ids = new List<Snowflake>(userIds.Count);
                foreach (Snowflake id in userIds)
                    ids.Add(id);

                return ids;
            }
            else
                return Array.Empty<Snowflake>();
        }

        /// <summary>
        /// Clears the entire cache.
        /// </summary>
        public void Clear()
        {
            guildIds.Clear();
            unavailableGuildIds.Clear();

            guildChannelIds.Clear();

            users.Clear();

            guilds.Clear();
            guildMetadata.Clear();

            guildChannels.Clear();

            guildMembers.Clear();
            guildPresences.Clear();
            guildVoiceStates.Clear();

            voiceChannelUsers.Clear();
        }
    }
}
