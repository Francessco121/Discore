using System;
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

        internal DiscordGatewayException(GatewayDisconnectCode dcCode, string message, Exception innerException)
            : base($"[{dcCode}:{(int)dcCode}] {message}", (WebSocketCloseStatus)dcCode, innerException)
        {
            DisconnectCode = dcCode;
        }
    }
}
