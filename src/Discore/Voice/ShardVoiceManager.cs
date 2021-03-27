using ConcurrentCollections;
using Discore.WebSocket;
using Discore.WebSocket.Internal;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Discore.Voice
{
    public class ShardVoiceManager
    {
        /// <summary>
        /// Gets a list of all voice connections for the current bot.
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
        /// Attempts to retrieve a voice connection by the guild the connection is in.
        /// </summary>
        public bool TryGetVoiceConnection(Snowflake guildId, out DiscordVoiceConnection connection)
        {
            return voiceConnections.TryGetValue(guildId, out connection);
        }

        /// <summary>
        /// Creates a voice connection for the specified guild.
        /// If a voice connection already exists for the given guild,
        /// a new connection is not created and the existing one is returned.
        /// </summary>
        public DiscordVoiceConnection CreateOrGetConnection(Snowflake guildId)
        {
            DiscordVoiceConnection connection;
            if (voiceConnections.TryGetValue(guildId, out connection))
            {
                // Return existing connection
                return connection;
            }

            connection = new DiscordVoiceConnection(shard, guildId);
            if (voiceConnections.TryAdd(guildId, connection))
                return connection;
            else
                // Connection already exists, just return the existing one.
                return voiceConnections[guildId];
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
