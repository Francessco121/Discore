using Discore.WebSocket;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Discore.Voice
{
    public class ShardVoiceManager
    {
        /// <summary>
        /// Gets a list of all voice connections for the current bot.
        /// </summary>
        public ICollection<DiscordVoiceConnection> VoiceConnections => voiceConnections.Values;

        readonly ConcurrentDictionary<Snowflake, DiscordVoiceConnection> voiceConnections;

        readonly Shard shard;

        internal ShardVoiceManager(Shard shard)
        {
            this.shard = shard;

            voiceConnections = new ConcurrentDictionary<Snowflake, DiscordVoiceConnection>();
        }

        /// <summary>
        /// Attempts to retrieve a voice connection by the guild the connection is in.
        /// </summary>
        public bool TryGetVoiceConnection(Snowflake guildId, [NotNullWhen(true)] out DiscordVoiceConnection? connection)
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
            DiscordVoiceConnection? connection;
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

        internal void Clear()
        {
            voiceConnections.Clear();
        }
    }
}
