using Discore.WebSocket;
using System;

namespace Discore.Voice
{
    public class VoiceConnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the shard that is managing the voice connection.
        /// </summary>
        public Shard Shard { get; }
        /// <summary>
        /// Gets the voice connection that fired the event.
        /// </summary>
        public DiscordVoiceConnection Connection { get; }

        internal VoiceConnectionEventArgs(Shard shard, DiscordVoiceConnection connection)
        {
            Shard = shard;
            Connection = connection;
        }
    }

    public class VoiceConnectionInvalidatedEventArgs : VoiceConnectionEventArgs
    {
        /// <summary>
        /// Gets the reason the voice connection was invalidated.
        /// </summary>
        public VoiceConnectionInvalidationReason Reason { get; }
        /// <summary>
        /// If reason is set to error or timed out, gets a message describing the error that caused the invalidation.
        /// Otherwise returns null.
        /// </summary>
        public string ErrorMessage { get; }

        internal VoiceConnectionInvalidatedEventArgs(Shard shard, DiscordVoiceConnection connection,
            VoiceConnectionInvalidationReason reason, string errorMessage = null)
            : base(shard, connection)
        {
            Reason = reason;
            ErrorMessage = errorMessage;
        }
    }
}
