namespace Discore
{
    /// <summary>
    /// Represents a cacheable <see cref="IDiscordObject"/>.
    /// </summary>
    public interface ICacheable : IDiscordObject
    {
        /// <summary>
        /// The id of the <see cref="IDiscordObject"/>.
        /// </summary>
        string Id { get; }
    }
}
