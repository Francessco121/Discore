using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpClient
    {
        /// <summary>
        /// Gets a list of all emojis in a guild.
        /// </summary>
        /// <param name="guildId">The ID of the guild.</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordEmoji>> ListGuildEmojis(Snowflake guildId)
        {
            using JsonDocument? data = await rest.Get($"guilds/{guildId}/emojis",
                $"guilds/{guildId}/emojis").ConfigureAwait(false);

            JsonElement values = data!.RootElement;

            var emojis = new DiscordEmoji[values.GetArrayLength()];
            for (int i = 0; i < emojis.Length; i++)
                emojis[i] = new DiscordEmoji(values[i]);

            return emojis;
        }

        /// <summary>
        /// Gets a list of all emojis in a guild.
        /// </summary>
        /// <param name="guild">The guild.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="guild"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordEmoji>> ListGuildEmojis(DiscordGuild guild)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return ListGuildEmojis(guild.Id);
        }

        /// <summary>
        /// Gets a guild's emoji.
        /// </summary>
        /// <param name="guildId">The ID of the guild.</param>
        /// <param name="emojiId">The ID of the emoji in the guild.</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordEmoji> GetGuildEmoji(Snowflake guildId, Snowflake emojiId)
        {
            using JsonDocument? data = await rest.Get($"guilds/{guildId}/emojis/{emojiId}",
                $"guilds/{guildId}/emojis/emoji").ConfigureAwait(false);

            return new DiscordEmoji(data!.RootElement);
        }

        /// <summary>
        /// Gets a guild's emoji.
        /// </summary>
        /// <param name="guild">The guild.</param>
        /// <param name="emojiId">The ID of the emoji in the guild.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="guild"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordEmoji> GetGuildEmoji(DiscordGuild guild, Snowflake emojiId)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return GetGuildEmoji(guild.Id, emojiId);
        }

        /// <summary>
        /// Creates and returns a new guild emoji.
        /// <para>Requires <see cref="DiscordPermission.ManageEmojis"/>.</para>
        /// </summary>
        /// <param name="guildId">The ID of the guild to give the new emoji to.</param>
        /// <param name="options">Options describing the properties of the new emoji.</param>
        /// <returns>Returns the newly created emoji.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordEmoji> CreateGuildEmoji(Snowflake guildId, CreateGuildEmojiOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string requestData = BuildJsonContent(options.Build);

            using JsonDocument? responseData = await rest.Post($"guilds/{guildId}/emojis", jsonContent: requestData,
                $"guilds/{guildId}/emojis").ConfigureAwait(false);

            return new DiscordEmoji(responseData!.RootElement);
        }

        /// <summary>
        /// Creates and returns a new guild emoji.
        /// <para>Requires <see cref="DiscordPermission.ManageEmojis"/>.</para>
        /// </summary>
        /// <param name="guild">The guild to give the new emoji to.</param>
        /// <param name="options">Options describing the properties of the new emoji.</param>
        /// <returns>Returns the newly created emoji.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="guild"/> or <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordEmoji> CreateGuildEmoji(DiscordGuild guild, CreateGuildEmojiOptions options)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return CreateGuildEmoji(guild.Id, options);
        }

        /// <summary>
        /// Updates an existing guild emoji.
        /// <para>Requires <see cref="DiscordPermission.ManageEmojis"/>.</para>
        /// </summary>
        /// <param name="guildId">The ID of the guild the emoji is in.</param>
        /// <param name="emojiId">The ID of the emoji.</param>
        /// <param name="options">Options describing the properties of the new emoji.</param>
        /// <returns>Returns the updated emoji.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordEmoji> ModifyGuildEmoji(Snowflake guildId, Snowflake emojiId,
            ModifyGuildEmojiOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string requestData = BuildJsonContent(options.Build);

            using JsonDocument? responseData = await rest.Patch($"guilds/{guildId}/emojis/{emojiId}", jsonContent: requestData,
                $"guilds/{guildId}/emojis/emoji").ConfigureAwait(false);

            return new DiscordEmoji(responseData!.RootElement);
        }

        /// <summary>
        /// Updates an existing guild emoji.
        /// <para>Requires <see cref="DiscordPermission.ManageEmojis"/>.</para>
        /// </summary>
        /// <param name="guild">The guild the emoji is in.</param>
        /// <param name="emoji">The emoji to modify.</param>
        /// <param name="options">Options describing the properties of the new emoji.</param>
        /// <returns>Returns the updated emoji.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="guild"/>, <paramref name="emoji"/>, or <paramref name="options"/> is null.
        /// </exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordEmoji> ModifyGuildEmoji(DiscordGuild guild, DiscordEmoji emoji,
            ModifyGuildEmojiOptions options)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));
            if (emoji == null) throw new ArgumentNullException(nameof(emoji));

            return ModifyGuildEmoji(guild.Id, emoji.Id, options);
        }

        /// <summary>
        /// Deletes a guild's emoji.
        /// <para>Requires <see cref="DiscordPermission.ManageEmojis"/>.</para>
        /// </summary>
        /// <param name="guildId">The ID of the guild the emoji is in.</param>
        /// <param name="emojiId">The ID of the emoji in the guild to delete.</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteGuildEmoji(Snowflake guildId, Snowflake emojiId)
        {
            await rest.Delete($"guilds/{guildId}/emojis/{emojiId}",
                $"guilds/{guildId}/emojis/emoji").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a guild's emoji.
        /// <para>Requires <see cref="DiscordPermission.ManageEmojis"/>.</para>
        /// </summary>
        /// <param name="guild">The guild the emoji is in.</param>
        /// <param name="emoji">The emoji to delete.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="guild"/> or <paramref name="emoji"/> is null.
        /// </exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task DeleteGuildEmoji(DiscordGuild guild, DiscordEmoji emoji)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));
            if (emoji == null) throw new ArgumentNullException(nameof(emoji));

            return DeleteGuildEmoji(guild.Id, emoji.Id);
        }
    }
}
