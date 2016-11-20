namespace Discore.Http
{
    public class DiscordInvite
    {
        /// <summary>
        /// Gets the unique invite code ID.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the guild this invite is for.
        /// </summary>
        public DiscordInviteGuild Guild { get; }

        /// <summary>
        /// Gets the channel this invite is for.
        /// </summary>
        public DiscordInviteChannel Channel { get; }

        public DiscordInvite(DiscordApiData data)
        {
            Code = data.GetString("code");

            DiscordApiData guildData = data.Get("guild");
            if (guildData != null)
                Guild = new DiscordInviteGuild(guildData);

            DiscordApiData channelData = data.Get("channel");
            if (channelData != null)
                Channel = new DiscordInviteChannel(channelData);
        }
    }
}
