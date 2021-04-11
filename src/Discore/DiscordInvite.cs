using System;
using System.Text.Json;

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
        public DiscordInviteGuild? Guild { get; }

        /// <summary>
        /// Gets the channel this invite is for.
        /// </summary>
        public DiscordInviteChannel Channel { get; }

        /// <summary>
        /// Gets the target user of this invite or null if no specific target exists.
        /// </summary>
        public DiscordUser? TargetUser { get; }

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

        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="code"/> or <paramref name="channel"/> is null.
        /// </exception>
        public DiscordInvite(
            string code, 
            DiscordInviteGuild? guild, 
            DiscordInviteChannel channel, 
            DiscordUser? targetUser, 
            DiscordInviteTargetUserType? targetUserType, 
            int? approximatePresenceCount, 
            int? approximateMemberCount)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Guild = guild;
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            TargetUser = targetUser;
            TargetUserType = targetUserType;
            ApproximatePresenceCount = approximatePresenceCount;
            ApproximateMemberCount = approximateMemberCount;
        }

        internal DiscordInvite(JsonElement json)
        {
            JsonElement? guildJson = json.GetPropertyOrNull("guild");
            Guild = guildJson == null ? null : new DiscordInviteGuild(guildJson.Value);

            JsonElement? targetUserJson = json.GetPropertyOrNull("target_user");
            TargetUser = targetUserJson == null ? null : new DiscordUser(targetUserJson.Value, isWebhookUser: false);

            Code = json.GetProperty("code").GetString()!;
            Channel = new DiscordInviteChannel(json.GetProperty("channel"));
            TargetUserType = (DiscordInviteTargetUserType?)json.GetPropertyOrNull("target_user_type")?.GetInt32();
            ApproximatePresenceCount = json.GetPropertyOrNull("approximate_presence_count")?.GetInt32();
            ApproximateMemberCount = json.GetPropertyOrNull("approximate_member_count")?.GetInt32();
        }
    }
}
