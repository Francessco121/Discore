namespace Discore
{
    /// <summary>
    /// Exception raised when an error occurs internally with the gateway caching system.
    /// While it should never happen, if this is ever received it should be reported to the Discore developers.
    /// </summary>
    public class DiscoreCacheException : DiscoreException
    {
        internal DiscoreCacheException(string message)
            : base($"{message}. This should be reported to the Discore developers.")
        { }
    }
}
