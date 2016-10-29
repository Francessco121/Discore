namespace Discore
{
    /// <summary>
    /// The base class for all Discord API objects.
    /// </summary>
    public abstract class DiscordObject
    {
        internal abstract void Update(DiscordApiData data);

        internal virtual new DiscordObject MemberwiseClone()
        {
            return (DiscordObject)base.MemberwiseClone();
        }
    }
}
