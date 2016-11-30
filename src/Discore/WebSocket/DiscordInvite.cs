namespace Discore.WebSocket
{
    public class DiscordInvite : DiscordObject
    {
        /// <summary>
        /// Gets the unique invite code ID.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Gets the guild this invite is for.
        /// </summary>
        public DiscordInviteGuild Guild { get; private set; }

        /// <summary>
        /// Gets the channel this invite is for.
        /// </summary>
        public DiscordInviteChannel Channel { get; private set; }

        Shard shard;

        internal DiscordInvite(Shard shard)
        {
            this.shard = shard;
        }

        internal override void Update(DiscordApiData data)
        {
            Code = data.GetString("code");

            DiscordApiData guildData = data.Get("guild");
            if (guildData != null)
            {
                if (Guild == null)
                    Guild = new DiscordInviteGuild(shard);

                Guild.Update(guildData);
            }

            DiscordApiData channelData = data.Get("channel");
            if (channelData != null)
            {
                if (Channel == null)
                    Channel = new DiscordInviteChannel(shard);

                Channel.Update(channelData);
            }
        }
    }
}
