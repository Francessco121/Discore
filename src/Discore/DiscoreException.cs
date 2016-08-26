using System;

namespace Discore
{
    /// <summary>
    /// The base exception for all Discore-specific exceptions.
    /// </summary>
    public class DiscoreException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="DiscoreException"/> instance.
        /// </summary>
        /// <param name="message">The message of the <see cref="DiscoreException"/>.</param>
        public DiscoreException(string message)
            : base(message)
        { }
    }
}
