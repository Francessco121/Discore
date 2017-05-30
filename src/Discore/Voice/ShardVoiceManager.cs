using ConcurrentCollections;
using Discore.WebSocket;
using Discore.WebSocket.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Discore.Voice
{
    public class ShardVoiceManager
    {
        /// <summary>
        /// Gets a list of all voice connections for the current authenticated user.
        /// </summary>
        public ICollection<DiscordVoiceConnection> VoiceConnections => voiceConnections.Values;

        ConcurrentDictionary<Snowflake, DiscordVoiceConnection> voiceConnections;
        ConcurrentDictionary<Snowflake, ConcurrentHashSet<Snowflake>> voiceChannelUsers;

        Shard shard;
        Gateway gateway;
        DiscordShardCache cache;

        internal ShardVoiceManager(Shard shard, Gateway gateway)
        {
            this.shard = shard;
            this.gateway = gateway;

            cache = shard.Cache;

            voiceConnections = new ConcurrentDictionary<Snowflake, DiscordVoiceConnection>();
            voiceChannelUsers = new ConcurrentDictionary<Snowflake, ConcurrentHashSet<Snowflake>>();
        }

        /// <summary>
        /// Gets a list of the IDs of every user currently in the specified voice channel.
        /// <para>Note: Will return an empty list if the voice channel is not found.</para>
        /// </summary>
        public IReadOnlyList<Snowflake> GetUsersInVoiceChannel(Snowflake voiceChannelId)
        {
            if (voiceChannelUsers.TryGetValue(voiceChannelId, out ConcurrentHashSet<Snowflake> userIds))
            {
                List<Snowflake> ids = new List<Snowflake>(userIds.Count);
                foreach (Snowflake id in userIds)
                    ids.Add(id);

                return ids;
            }
            else
                return new Snowflake[0];
        }

        /// <summary>
        /// Attempts to retrieve a voice connection for the current user,
        /// by the guild the connection is in.
        /// </summary>
        public bool TryGetVoiceConnection(Snowflake guildId, out DiscordVoiceConnection connection)
        {
            return voiceConnections.TryGetValue(guildId, out connection);
        }

        /// <summary>
        /// Creates a voice connection to the specified voice channel.
        /// If a voice connection already exists for the given channel,
        /// a new connection is not created and the existing one is returned.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordPermissionException">
        /// Thrown if the current user does not have permission to connect to the voice channel.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the voice channel is full, and the current user does not have the administrator permission.
        /// </exception>
        public DiscordVoiceConnection CreateConnection(DiscordGuildVoiceChannel voiceChannel)
        {
            if (voiceChannel == null)
                throw new ArgumentNullException(nameof(voiceChannel));

            DiscordVoiceConnection connection;
            if (voiceConnections.TryGetValue(voiceChannel.GuildId, out connection))
            {
                // Return existing connection
                return connection;
            }

            // Get the guild
            if (cache.Guilds.TryGetValue(voiceChannel.GuildId, out MutableGuild mutableGuild))
            {
                // Get the authenticated user's guild member from cache
                if (cache.GuildMembers.TryGetValue(shard.User.Id, out memberCache))
                {
                    // Check if the user has permission to connect.
                    DiscordPermissionHelper.AssertPermission(DiscordPermission.Connect,
                        memberCache.Value, guildCache.Value, voiceChannel);

                    DiscoreVoiceChannelCache voiceChannelCache;
                    if (guildCache.VoiceChannels.TryGetValue(voiceChannel.Id, out voiceChannelCache))
                    {
                        bool isAdmin = DiscordPermissionHelper.HasPermission(DiscordPermission.Administrator,
                            memberCache.Value, guildCache.Value, voiceChannel);

                        if (isAdmin || voiceChannel.UserLimit == 0 || voiceChannelCache.ConnectedMembers.Count < voiceChannel.UserLimit)
                        {
                            // Create the new connection
                            connection = new DiscordVoiceConnection(shard, gateway, guildCache, memberCache, voiceChannel);
                            if (voiceConnections.TryAdd(voiceChannel.GuildId, connection))
                                return connection;
                            else
                                // Connection already exists, just return the existing one.
                                return voiceConnections[voiceChannel.GuildId];
                        }
                        else
                            throw new InvalidOperationException(
                                $"The voice channel is full, and the current authenticated user does not have the administrator permission.");
                    }
                    else
                        throw new DiscoreCacheException("The specified voice channel does not exist in the local cache.");
                }
                else
                    // This really should never ever ever happen.
                    throw new ArgumentException("The current authenticated user is not a member of the specified guild.",
                        nameof(voiceChannel));
            }
            else
                throw new DiscoreCacheException("The specified guild does not exist in the local cache.");
        }

        internal void RemoveVoiceConnection(Snowflake guildId)
        {
            voiceConnections.TryRemove(guildId, out _);
        }

        internal void AddUserToVoiceChannel(Snowflake voiceChannelId, Snowflake userId)
        {
            ConcurrentHashSet<Snowflake> userList;
            if (!voiceChannelUsers.TryGetValue(voiceChannelId, out userList))
            {
                userList = new ConcurrentHashSet<Snowflake>();
                voiceChannelUsers[voiceChannelId] = userList;
            }

            userList.Add(userId);
        }

        internal void RemoveUserFromVoiceChannel(Snowflake voiceChannelId, Snowflake userId)
        {
            if (voiceChannelUsers.TryGetValue(voiceChannelId, out ConcurrentHashSet<Snowflake> userList))
                userList.TryRemove(userId);
        }

        internal void Clear()
        {
            voiceConnections.Clear();
            voiceChannelUsers.Clear();
        }
    }
}
