namespace Discore
{
    /// <summary>
    /// An exception thrown by a socket connection in Discore.
    /// </summary>
    public class DiscoreSocketException : DiscoreException
    {
        /// <summary>
        /// Creates a <see cref="DiscoreSocketException"/>
        /// </summary>
        /// <param name="message">The message of the <see cref="DiscoreSocketException"/>.</param>
        public DiscoreSocketException(string message)
            : base(message)
        { }
    }
}
