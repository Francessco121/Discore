using Discore.WebSocket;
using System;

namespace Discore.Voice
{
    public class VoiceConnectionEventArgs : EventArgs
    {
        public Shard Shard { get; }
        public DiscordVoiceConnection Connection { get; }

        internal VoiceConnectionEventArgs(Shard shard, DiscordVoiceConnection connection)
        {
            Shard = shard;
            Connection = connection;
        }
    }
}
