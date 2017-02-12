using System;

namespace Discore
{
    public enum DiscordGameType
    {
        /// <summary>
        /// Example display: "Playing Overwatch".
        /// </summary>
        Game,
        /// <summary>
        /// Example display: "Streaming Overwatch".
        /// </summary>
        Streaming,

        /// <summary>
        /// Normal application.
        /// </summary>
        [Obsolete("Use DiscordGameType.Game instead.")]
        Default = Game,
        /// <summary>
        /// Twitch.tv game.
        /// </summary>
        [Obsolete("Use DiscordGameType.Streaming instead.")]
        Twitch = Streaming
    }
}
