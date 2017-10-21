using Discore.Http.Net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpClient
    {
        /// <summary>
        /// Adds a reaction to a message.
        /// <para>Requires <see cref="DiscordPermission.ReadMessageHistory"/>.</para>
        /// <para>Requires <see cref="DiscordPermission.AddReactions"/> if nobody else has reacted to the message prior.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task CreateReaction(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            if (emoji == null)
                throw new ArgumentNullException(nameof(emoji));

            await rest.Put($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me",
                $"channels/{channelId}/messages/message/reactions/emoji/@me").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a reaction that the current bot has added to a message.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteOwnReaction(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            if (emoji == null)
                throw new ArgumentNullException(nameof(emoji));

            await rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me",
                $"channels/{channelId}/messages/message/reactions/emoji/@me").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a reaction posted by any user.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteUserReaction(Snowflake channelId, Snowflake messageId, Snowflake userId, DiscordReactionEmoji emoji)
        {
            if (emoji == null)
                throw new ArgumentNullException(nameof(emoji));

            await rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/{userId}",
                $"channels/{channelId}/messages/message/reactions/emoji/user").ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a paginated list of users who reacted to the specified message with the specified emoji.
        /// </summary>
        /// <param name="baseUserId">The user ID to start at when retrieving reactions.</param>
        /// <param name="limit">The maximum number of reactions to return or null to use the default.</param>
        /// <param name="getStrategy">The pagination strategy to use based on <paramref name="baseUserId"/>.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordUser>> GetReactions(Snowflake channelId, Snowflake messageId, 
            DiscordReactionEmoji emoji, Snowflake? baseUserId = null, int? limit = null,
            ReactionGetStrategy getStrategy = ReactionGetStrategy.Before)
        {
            if (emoji == null)
                throw new ArgumentNullException(nameof(emoji));

            UrlParametersBuilder builder = new UrlParametersBuilder();
            if (baseUserId.HasValue)
                builder.Add(getStrategy.ToString().ToLower(), baseUserId.Value.ToString());
            if (limit.HasValue)
                builder.Add("limit", limit.Value.ToString());

            DiscordApiData data = await rest.Get(
                $"channels/{channelId}/messages/{messageId}/reactions/{emoji}{builder.ToQueryString()}",
                $"channels/{channelId}/messages/message/reactions/emoji").ConfigureAwait(false);

            DiscordUser[] users = new DiscordUser[data.Values.Count];
            for (int i = 0; i < users.Length; i++)
                users[i] = new DiscordUser(false, data.Values[i]);

            return users;
        }

        /// <summary>
        /// Deletes all reactions on a message.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteAllReactions(Snowflake channelId, Snowflake messageId)
        {
            await rest.Delete($"channels/{channelId}/messages/{messageId}/reactions",
                $"channels/{channelId}/messages/message/reactions").ConfigureAwait(false);
        }
    }
}
