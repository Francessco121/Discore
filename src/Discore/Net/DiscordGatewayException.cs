namespace Discore.Net
{
    public class DiscordGatewayException : DiscordioSocketException
    {
        public int DisconnectCode { get; }

        internal DiscordGatewayException(GatewayDisconnectCode dcCode, string message)
            : base($"[{dcCode}:{(int)dcCode}] {message}")
        {
            DisconnectCode = (int)dcCode;
        }
    }
}
