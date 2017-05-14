using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpApi
    {
        /// <summary>
        /// Adds a reaction to a message.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task CreateReaction(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            if (emoji == null)
                throw new ArgumentNullException(nameof(emoji));

            await rest.Put($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me",
                "channels/channel/messages/message/reactions/emoji/@me").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a reaction the currently authenticated user has made for a message.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteOwnReaction(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            if (emoji == null)
                throw new ArgumentNullException(nameof(emoji));

            await rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me",
                "channels/channel/messages/message/reactions/emoji/@me").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a reaction posted by any user.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteUserReaction(Snowflake channelId, Snowflake messageId, Snowflake userId, DiscordReactionEmoji emoji)
        {
            if (emoji == null)
                throw new ArgumentNullException(nameof(emoji));

            await rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/{userId}",
                "channels/channel/messages/message/reactions/emoji/user").ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a list of all users who reacted to the specified message with the specified emoji.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordUser>> GetReactions(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            if (emoji == null)
                throw new ArgumentNullException(nameof(emoji));

            DiscordApiData data = await rest.Get($"channels/{channelId}/messages/{messageId}/reactions/{emoji}",
                "channels/channel/messages/message/reactions/emoji").ConfigureAwait(false);

            DiscordUser[] users = new DiscordUser[data.Values.Count];
            for (int i = 0; i < users.Length; i++)
                users[i] = new DiscordUser(data.Values[i]);

            return users;
        }

        /// <summary>
        /// Deletes all reactions on a message.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteAllReactions(Snowflake channelId, Snowflake messageId)
        {
            await rest.Delete($"channels/{channelId}/messages/{messageId}/reactions",
                "channels/channel/messages/message/reactions").ConfigureAwait(false);
        }
    }
}
