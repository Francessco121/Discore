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
        public DiscordInviteChannel? Channel { get; }

        /// <summary>
        /// Gets the user who created the invite.
        /// </summary>
        public DiscordUser? Inviter { get; }

        /// <summary>
        /// Gets the target user of this invite or null if no specific target exists.
        /// </summary>
        public DiscordUser? TargetUser { get; }

        /// <summary>
        /// Gets the type of target or null if no specific target exists.
        /// </summary>
        public DiscordInviteTargetType? TargetType { get; }

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

        internal DiscordInvite(JsonElement json)
        {
            JsonElement? guildJson = json.GetPropertyOrNull("guild");
            Guild = guildJson == null ? null : new DiscordInviteGuild(guildJson.Value);

            JsonElement channelJson = json.GetProperty("channel");
            Channel = channelJson.ValueKind == JsonValueKind.Null ? null : new DiscordInviteChannel(channelJson);

            JsonElement? inviterJson = json.GetPropertyOrNull("inviter");
            Inviter = inviterJson == null ? null : new DiscordUser(inviterJson.Value, isWebhookUser: false);

            JsonElement? targetUserJson = json.GetPropertyOrNull("target_user");
            TargetUser = targetUserJson == null ? null : new DiscordUser(targetUserJson.Value, isWebhookUser: false);

            Code = json.GetProperty("code").GetString()!;
            TargetType = (DiscordInviteTargetType?)json.GetPropertyOrNull("target_type")?.GetInt32();
            ApproximatePresenceCount = json.GetPropertyOrNull("approximate_presence_count")?.GetInt32();
            ApproximateMemberCount = json.GetPropertyOrNull("approximate_member_count")?.GetInt32();
        }
    }
}
