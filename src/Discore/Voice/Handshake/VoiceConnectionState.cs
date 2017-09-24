using Discore.Voice.Net;
using Discore.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discore.Voice.Handshake
{
    class VoiceConnectionState
    {
        public Shard Shard;
        public Snowflake GuildId;
        public DiscordVoiceState VoiceState;
        public VoiceWebSocket WebSocket;
        public string EndPoint;
        public string Token;
        public int HeartbeatInterval;
    }
}
