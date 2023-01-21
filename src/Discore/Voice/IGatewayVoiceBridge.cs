using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Voice
{
    /// <summary>
    /// Provides a bridge between a Gateway connection and associated voice connections.
    /// </summary>
    /// <remarks>
    /// The purpose of this interface is to let voice connections run either in the same
    /// or a different process than the main bot application (or even a different machine 
    /// entirely). Handling voice can be more resource intensive than usual bot operations, 
    /// so allowing voice connections to live and scale independently of the main bot may 
    /// be desirable.
    /// <para/>
    /// Ex. applications may implement this to bridge a network gap between the main bot application
    /// and the application(s) responsible for handling voice connections.
    /// <para/>
    /// Implementers are expected to forward Gateway events to each bridge event and methods
    /// back to the originating Gateway connection. Voice connections are tied to the single
    /// Gateway connection that serves the guild where voice is connected.
    /// </remarks>
    public interface IGatewayVoiceBridge
    {
        /// <summary>
        /// Called when someone joins/leaves/moves voice channels.
        /// </summary>
        event EventHandler<BridgeVoiceStateUpdateEventArgs>? OnVoiceStateUpdate;

        /// <summary>
        /// Called when the voice server for a guild is updated.
        /// </summary>
        /// <remarks>
        /// This is sent when initially connecting to voice and when the current voice 
        /// instance fails over to a new server.
        /// </remarks>
        event EventHandler<BridgeVoiceServerUpdateEventArgs>? OnVoiceServerUpdate;

        /// <summary>
        /// Joins, moves, or disconnects the application from a voice channel.
        /// </summary>
        /// <param name="guildId">The ID of the guild containing the voice channel.</param>
        /// <param name="channelId">The ID of the voice channel to join/move to or null to disconnect from voice.</param>
        /// <param name="isMute">Whether the application is self-mute.</param>
        /// <param name="isDeaf">Whether the application is self-deafened.</param>
        /// <param name="cancellationToken">A token used to cancel the request.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the voice state could not be updated due to a stateful issue related to the Gateway connection.
        /// </exception>
        /// <exception cref="OperationCanceledException">Thrown if the cancellation token is cancelled.</exception>
        Task UpdateVoiceStateAsync(Snowflake guildId, Snowflake? channelId, bool isMute = false, bool isDeaf = false,
            CancellationToken? cancellationToken = null);
    }
}
