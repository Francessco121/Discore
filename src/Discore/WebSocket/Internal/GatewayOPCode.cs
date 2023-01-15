namespace Discore.WebSocket.Internal
{
    enum GatewayOPCode
    {
        Dispatch = 0,
        Heartbeat,
        Identify,
        PresenceUpdate,
        VoiceStateUpdate,
        VoiceServerPing,
        Resume,
        Reconnect,
        RequestGuildMembers,
        InvalidSession,
        Hello,
        HeartbeatAck
    }
}
