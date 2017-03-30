using Discore.WebSocket;
using System;

namespace Discore.Voice
{
    public class VoiceConnectionErrorEventArgs : VoiceConnectionEventArgs
    {
        public Exception Exception { get; }

        internal VoiceConnectionErrorEventArgs(Shard shard, DiscordVoiceConnection connection, Exception exception)
            : base(shard, connection)
        {
            Exception = exception;
        }
    }
}
