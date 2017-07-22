namespace Discore
{
    /// <summary>
    /// This enum represents all API v6 channel types. It will remain internal
    /// to allow Discore to keep the same public API until the next major
    /// release, where DiscordChannelType will get these values.
    /// </summary>
    enum InternalChannelType
    {
        GuildText = 0,
        DM = 1,
        GuildVoice = 2,
        GroupDM = 3,
        GuildCategory = 4
    }
}
