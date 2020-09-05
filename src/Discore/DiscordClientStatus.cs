namespace Discore
{
    public sealed class DiscordClientStatus
    {
        /// <summary>
        /// The user's status set for an active desktop (Windows, Linux, Mac) application session.
        /// </summary>
        public string Desktop { get; }

        /// <summary>
        /// The user's status set for an active mobile (iOS, Android) application session.
        /// </summary>
        public string Mobile { get; }

        /// <summary>
        /// The user's status set for an active web (browser, bot account) application session.
        /// </summary>
        public string Web { get; }

        internal DiscordClientStatus(DiscordApiData data)
        {
            Desktop = data.GetString("desktop");
            Mobile = data.GetString("mobile");
            Web = data.GetString("web");
        }
    }
}
