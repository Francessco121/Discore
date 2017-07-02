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

        DiscordHttpClient http;

        internal DiscordInvite(DiscordHttpClient http, DiscordApiData data)
        {
            this.http = http;

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
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordInvite> Delete()
        {
            return http.DeleteInvite(Code);
        }
    }
}
