namespace Discore.WebSocket.Internal
{
    /// <summary>
    /// A shared memory cache for the Gateway URL to be shared across all Gateway connections in the process.
    /// </summary>
    static class GatewayUrlMemoryCache
    {
        public static string? GatewayUrl { get; private set; }

        static readonly object lockObj = new object();

        public static void UpdateUrl(string url)
        {
            lock (lockObj)
            {
                GatewayUrl = url;
            }
        }

        public static void InvalidateUrl()
        {
            lock (lockObj)
            {
                GatewayUrl = null;
            }
        }
    }
}
