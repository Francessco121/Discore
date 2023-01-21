using System;

namespace Discore.Voice
{
    public abstract class GatewayVoiceBridgeEventArgs : EventArgs { }

    public class BridgeVoiceStateUpdateEventArgs : GatewayVoiceBridgeEventArgs
    {
        /// <summary>
        /// The voice state of the user who's voice state changed.
        /// </summary>
        public DiscordVoiceState VoiceState { get; }

        public BridgeVoiceStateUpdateEventArgs(DiscordVoiceState state)
        {
            VoiceState = state;
        }
    }

    public class BridgeVoiceServerUpdateEventArgs : GatewayVoiceBridgeEventArgs
    {
        /// <summary>
        /// The voice server that changed.
        /// </summary>
        public DiscordVoiceServer VoiceServer { get; }

        public BridgeVoiceServerUpdateEventArgs(DiscordVoiceServer server)
        {
            VoiceServer = server;
        }
    }
}
