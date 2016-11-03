namespace Discore.Net.Sockets
{
    public enum GatewayOPCode
    {
        Dispath = 0,
        Heartbeat,
        Identify,
        StatusUpdate,
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
