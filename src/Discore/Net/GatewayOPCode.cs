
namespace Discore.Net
{
    enum GatewayOPCode : int
    {
        /// <summary>
        /// C←S - Used to send most events.
        /// </summary>
        Dispath = 0,
        /// <summary>
        /// C→S - Used to keep the connection alive and measure latency.
        /// </summary>
        Heartbeat = 1,
        /// <summary>
        /// C→S - Used to associate a connection with a token and specify configuration.
        /// </summary>
        Identify = 2,
        /// <summary>
        /// C→S - Used to update client's status and current game id.
        /// </summary>
        StatusUpdate = 3,
        /// <summary>
        /// C↔S - Used to join a particular voice channel.
        /// </summary>
        VoiceStateUpdate = 4,
        /// <summary>
        /// C→S - Used to ensure the server's voice server is alive. Only send this if voice connection fails or suddenly drops.
        /// </summary>
        VoiceServerPing = 5,
        /// <summary>
        /// C→S - Used to resume a connection after a redirect occurs.
        /// </summary>
        Resume = 6,
        /// <summary>
        /// C←S - Used to notify a client that they must reconnect to another gateway.
        /// </summary>
        Reconnect = 7,
        /// <summary>
        /// C→S - Used to request all members that were withheld by large_threshold.
        /// </summary>
        RequestGuildMembers = 8,
        /// <summary>
        /// C←S - Used to notify a client that they have an invalid session id.
        /// </summary>
        InvalidSession = 9,
        /// <summary>
        /// C←S - Sent immediately after connecting, contains heartbeat and server debug information.
        /// </summary>
        Hello = 10,
        /// <summary>
        /// C←S - Sent immediately following a client heartbeat that was received.
        /// </summary>
        HeartbeatACK = 11,
    }
}
