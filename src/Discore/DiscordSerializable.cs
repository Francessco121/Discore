namespace Discore
{
    public abstract class DiscordSerializable
    {
        internal DiscordSerializable() { }

        internal abstract DiscordApiData Serialize();
    }
}
