namespace Discore.Http
{
    /// <summary>
    /// Defines the way HTTP rate limits are handled locally.
    /// </summary>
    public enum RateLimitHandlingMethod
    {
        /// <summary>
        /// Prevents being rate limited by placing requests into a queue and only sending them out when it is safe. 
        /// Avoids 429's 99% of the time. 
        /// <para>Note: the queue is per rate-limited route, instead of effecting every type of request globally.</para>
        /// </summary>
        Throttle,
        /// <summary>
        /// Doesn't try to control requests, but forces requests to wait after a 429 is received.
        /// </summary>
        Minimal
    }
}
