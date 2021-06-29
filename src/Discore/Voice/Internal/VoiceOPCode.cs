namespace Discore.Voice.Internal
{
    enum VoiceOPCode : int
    {
        /// <summary>
        /// C->S Used to begin a voice websocket connection.
        /// </summary>
        Identify = 0,
        /// <summary>
        /// Used to select the voice protocol.
        /// </summary>
        SelectProtocol = 1,
        /// <summary>
        /// Used to complete the websocket handshake.
        /// </summary>
        Ready = 2,
        /// <summary>
        /// Used to keep the websocket connection alive.
        /// </summary>
        Heartbeat = 3,
        /// <summary>
        /// Used to describe the session.
        /// </summary>
        SessionDescription = 4,
        /// <summary>
        /// Used to indicate which users are speaking.
        /// </summary>
        Speaking = 5,
        HeartbeatAck = 6,
        Resume = 7,
        Hello = 8,
        Resumed = 9,
        ClientDisconnect = 13
    }
}
