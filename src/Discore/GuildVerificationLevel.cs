namespace Discore
{
    /// <summary>
    /// Member verification levels for Discord guilds.
    /// </summary>
    public enum GuildVerificationLevel
    {
        /// <summary>
        /// Unrestricted.
        /// </summary>
        None,
        /// <summary>
        /// Must have a verified email on their Discord account.
        /// </summary>
        Low,
        /// <summary>
        /// Must have a verified email on their Discord account 
        /// and be registered on Discord for longer than 5 minutes.
        /// </summary>
        Medium,
        /// <summary>
        /// Must have a verified email on their Discord account,
        /// be registered on Discord for longer than 5 minutes,
        /// and be a member of this server for longer than 10 minutes.
        /// <para>AKA: (╯°□°）╯︵ ┻━┻</para>
        /// </summary>
        High,
        /// <summary>
        /// Must have a verified phone on their Discord account.
        /// <para>AKA: ┻━┻ ﾐヽ(ಠ益ಠ)ノ彡┻━┻</para>
        /// </summary>
        VeryHigh
    }
}
