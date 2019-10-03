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

        /// <summary>
        /// Gets the target user of this invite or null if no specific target exists.
        /// </summary>
        public DiscordUser TargetUser { get; }

        /// <summary>
        /// Gets the type of target user or null if no specific target user exists.
        /// </summary>
        /// <seealso cref="TargetUser"/>
        public DiscordInviteTargetUserType? TargetUserType { get; }

        /// <summary>
        /// Gets the approximate number of online members in the guild which this invite is for.
        /// Will be null if not available.
        /// </summary>
        public int? ApproximatePresenceCount { get; }

        /// <summary>
        /// Gets the approximate number of total members in the guild which this invite is for.
        /// Will be null if not available.
        /// </summary>
        public int? ApproximateMemberCount { get; }

        readonly DiscordHttpClient http;

        internal DiscordInvite(DiscordHttpClient http, DiscordApiData data)
        {
            this.http = http;

            Code = data.GetString("code");
            TargetUserType = (DiscordInviteTargetUserType?)data.GetInteger("target_user_type");
            ApproximatePresenceCount = data.GetInteger("approximate_presence_count");
            ApproximateMemberCount = data.GetInteger("approximate_member_count");

            DiscordApiData guildData = data.Get("guild");
            if (guildData != null)
                Guild = new DiscordInviteGuild(guildData);

            DiscordApiData channelData = data.Get("channel");
            if (channelData != null)
                Channel = new DiscordInviteChannel(channelData);

            DiscordApiData userData = data.Get("target_user");
            if (userData != null)
                TargetUser = new DiscordUser(isWebhookUser: false, userData);
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
