namespace Discore
{
    /// <summary>
    /// Allows a Discord API object to be stored in a <see cref="DiscoreCache"/>.
    /// </summary>
    public abstract class DiscordHashableObject
    {
        internal abstract Snowflake DictionaryId { get; }

        internal DiscordHashableObject() { }
    }
}
