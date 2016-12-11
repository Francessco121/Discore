using Discore.Voice;
using Discore.WebSocket.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Discore.WebSocket
{
    public class ShardVoiceManager
    {
        /// <summary>
        /// Gets a list of all voice connections for the current authenticated user.
        /// </summary>
        public ICollection<DiscordVoiceConnection> VoiceConnections
        {
            get { return voiceConnections.Values; }
        }

        ConcurrentDictionary<Snowflake, DiscordVoiceConnection> voiceConnections;
        Shard shard;
        Gateway gateway;
        DiscoreCache cache;

        internal ShardVoiceManager(Shard shard, Gateway gateway)
        {
            this.shard = shard;
            this.gateway = gateway;

            cache = shard.Cache;

            voiceConnections = new ConcurrentDictionary<Snowflake, DiscordVoiceConnection>();
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
        /// Initiates a voice connection to the specified voice channel.
        /// </summary>
        public DiscordVoiceConnection ConnectToVoice(DiscordGuildVoiceChannel voiceChannel)
        {
            DiscordVoiceConnection connection;
            if (voiceConnections.TryRemove(voiceChannel.GuildId, out connection))
                // Close any existing connection.
                connection.Disconnect();

            // Get the guild cache
            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(voiceChannel.GuildId, out guildCache))
            {
                // Get the authenticated user's guild member from cache
                DiscoreMemberCache memberCache;
                if (guildCache.Members.TryGetValue(shard.User.Id, out memberCache))
                {
                    // Create the new connection
                    connection = new DiscordVoiceConnection(shard, gateway, guildCache, memberCache, voiceChannel);
                    if (voiceConnections.TryAdd(voiceChannel.GuildId, connection))
                    {
                        // Initiate connection
                        gateway.SendVoiceStateUpdatePayload(voiceChannel.GuildId, voiceChannel.Id, false, false);
                        return connection;
                    }
                    else
                        // Connection already exists, just return the existing one.
                        return voiceConnections[voiceChannel.GuildId];
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
            DiscordVoiceConnection temp;
            voiceConnections.TryRemove(guildId, out temp);
        }

        internal void Clear()
        {
            voiceConnections.Clear();
        }
    }
}
