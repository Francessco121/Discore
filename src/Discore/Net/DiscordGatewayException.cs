using System.Net.WebSockets;

namespace Discore.Net
{
    /// <summary>
    /// An exception thrown by an <see cref="IDiscordGateway"/> instance.
    /// </summary>
    public class DiscordGatewayException : DiscoreSocketException
    {
        /// <summary>
        /// The disconnect code sent by the <see cref="IDiscordGateway"/>.
        /// </summary>
        public GatewayDisconnectCode DisconnectCode { get; }

        internal DiscordGatewayException(GatewayDisconnectCode dcCode, string message)
            : base($"[{dcCode}:{(int)dcCode}] {message}", (WebSocketCloseStatus)dcCode)
        {
            DisconnectCode = dcCode;
        }
    }
}
