namespace Discore
{
    /// <summary>
    /// Represents an object containing a <see cref="DiscordApiCache"/>.
    /// </summary>
    public interface ICacheContainer
    {
        /// <summary>
        /// Gets the <see cref="DiscordApiCache"/> inside this container.
        /// </summary>
        DiscordApiCache Cache { get; }
    }
}
