using Discore.Http;
using System.Threading.Tasks;

namespace Discore
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

        DiscordHttpInviteEndpoint invitesHttp;

        internal DiscordInvite(IDiscordApplication app, DiscordApiData data)
        {
            invitesHttp = app.HttpApi.Invites;

            Code = data.GetString("code");

            DiscordApiData guildData = data.Get("guild");
            if (guildData != null)
                Guild = new DiscordInviteGuild(guildData);

            DiscordApiData channelData = data.Get("channel");
            if (channelData != null)
                Channel = new DiscordInviteChannel(channelData);
        }

        /// <summary>
        /// Deletes this invite.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordInvite> Delete()
        {
            return invitesHttp.Delete(Code);
        }

        /// <summary>
        /// Accepts this invite.
        /// Note: This does not work for bot accounts.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordInvite> Accept()
        {
            return invitesHttp.Accept(Code);
        }
    }
}
