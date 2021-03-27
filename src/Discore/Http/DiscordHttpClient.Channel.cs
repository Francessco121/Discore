using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpClient
    {
        /// <summary>
        /// Gets a DM or guild channel by ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordChannel> GetChannel(Snowflake channelId)
        {
            return GetChannel<DiscordChannel>(channelId);
        }

        /// <summary>
        /// Gets a DM or guild channel by ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<T> GetChannel<T>(Snowflake channelId)
            where T : DiscordChannel
        {
            DiscordApiData data = await rest.Get($"channels/{channelId}", $"channels/{channelId}").ConfigureAwait(false);
            return (T)DeserializeChannelData(data);
        }

        /// <summary>
        /// Updates the settings of a guild text channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <param name="textChannelId">The ID of the guild text channel to modify.</param>
        /// <param name="options">A set of options to modify the channel with.</param>
        /// <returns>Returns the updated guild text channel.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildTextChannel> ModifyTextChannel(Snowflake textChannelId,
            GuildTextChannelOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            DiscordApiData requestData = options.Build();

            DiscordApiData returnData = await rest.Patch($"channels/{textChannelId}", requestData,
                $"channels/{textChannelId}").ConfigureAwait(false);
            return (DiscordGuildTextChannel)DeserializeChannelData(returnData);
        }

        /// <summary>
        /// Updates the settings of a guild text channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <param name="textChannel">The guild text channel to modify.</param>
        /// <param name="options">A set of options to modify the channel with.</param>
        /// <returns>Returns the updated guild text channel.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildTextChannel> ModifyTextChannel(DiscordGuildTextChannel textChannel,
            GuildTextChannelOptions options)
        {
            if (textChannel == null)
                throw new ArgumentNullException(nameof(textChannel));

            return ModifyTextChannel(textChannel.Id, options);
        }

        /// <summary>
        /// Updates the settings of a guild voice channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <param name="voiceChannelId">The ID of the guild voice channel to modify.</param>
        /// <param name="options">A set of options to modify the channel with.</param>
        /// <returns>Returns the updated guild voice channel.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildVoiceChannel> ModifyVoiceChannel(Snowflake voiceChannelId,
            GuildVoiceChannelOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            DiscordApiData requestData = options.Build();

            DiscordApiData returnData = await rest.Patch($"channels/{voiceChannelId}", requestData,
                $"channels/{voiceChannelId}").ConfigureAwait(false);
            return (DiscordGuildVoiceChannel)DeserializeChannelData(returnData);
        }

        /// <summary>
        /// Updates the settings of a guild voice channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <param name="voiceChannel">The guild voice channel to modify.</param>
        /// <param name="options">A set of options to modify the channel with.</param>
        /// <returns>Returns the updated guild voice channel.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildVoiceChannel> ModifyVoiceChannel(DiscordGuildVoiceChannel voiceChannel,
            GuildVoiceChannelOptions options)
        {
            if (voiceChannel == null)
                throw new ArgumentNullException(nameof(voiceChannel));

            return ModifyVoiceChannel(voiceChannel.Id, options);
        }

        /// <summary>
        /// Updates the settings of a guild category channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <param name="categoryChannelId">The ID of the guild category channel to modify.</param>
        /// <param name="options">A set of options to modify the channel with.</param>
        /// <returns>Returns the updated guild category channel.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildCategoryChannel> ModifyCategoryChannel(Snowflake categoryChannelId,
            GuildCategoryChannelOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            DiscordApiData requestData = options.Build();

            DiscordApiData returnData = await rest.Patch($"channels/{categoryChannelId}", requestData,
                $"channels/{categoryChannelId}").ConfigureAwait(false);
            return (DiscordGuildCategoryChannel)DeserializeChannelData(returnData);
        }

        /// <summary>
        /// Updates the settings of a guild category channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <param name="categoryChannel">The guild category channel to modify.</param>
        /// <param name="options">A set of options to modify the channel with.</param>
        /// <returns>Returns the updated guild category channel.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildCategoryChannel> ModifyCategoryChannel(DiscordGuildCategoryChannel categoryChannel,
            GuildCategoryChannelOptions options)
        {
            if (categoryChannel == null)
                throw new ArgumentNullException(nameof(categoryChannel));

            return ModifyCategoryChannel(categoryChannel.Id, options);
        }

        /// <summary>
        /// Updates the settings of a guild news channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <param name="newsChannelId">The ID of the guild news channel to modify.</param>
        /// <param name="options">A set of options to modify the channel with.</param>
        /// <returns>Returns the updated guild news channel.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildNewsChannel> ModifyNewsChannel(Snowflake newsChannelId,
            GuildNewsChannelOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            DiscordApiData requestData = options.Build();

            DiscordApiData returnData = await rest.Patch($"channels/{newsChannelId}", requestData,
                $"channels/{newsChannelId}").ConfigureAwait(false);
            return (DiscordGuildNewsChannel)DeserializeChannelData(returnData);
        }

        /// <summary>
        /// Updates the settings of a guild news channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <param name="newsChannel">The guild news channel to modify.</param>
        /// <param name="options">A set of options to modify the channel with.</param>
        /// <returns>Returns the updated guild news channel.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildNewsChannel> ModifyNewsChannel(DiscordGuildNewsChannel newsChannel,
            GuildNewsChannelOptions options)
        {
            if (newsChannel == null)
                throw new ArgumentNullException(nameof(newsChannel));

            return ModifyNewsChannel(newsChannel.Id, options);
        }

        /// <summary>
        /// Updates the settings of a guild store channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <param name="storeChannelId">The ID of the guild store channel to modify.</param>
        /// <param name="options">A set of options to modify the channel with.</param>
        /// <returns>Returns the updated guild store channel.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildStoreChannel> ModifyStoreChannel(Snowflake storeChannelId,
            GuildStoreChannelOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            DiscordApiData requestData = options.Build();

            DiscordApiData returnData = await rest.Patch($"channels/{storeChannelId}", requestData,
                $"channels/{storeChannelId}").ConfigureAwait(false);
            return (DiscordGuildStoreChannel)DeserializeChannelData(returnData);
        }

        /// <summary>
        /// Updates the settings of a guild store channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <param name="storeChannel">The guild store channel to modify.</param>
        /// <param name="options">A set of options to modify the channel with.</param>
        /// <returns>Returns the updated guild store channel.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildStoreChannel> ModifyStoreChannel(DiscordGuildStoreChannel storeChannel,
            GuildStoreChannelOptions options)
        {
            if (storeChannel == null)
                throw new ArgumentNullException(nameof(storeChannel));

            return ModifyStoreChannel(storeChannel.Id, options);
        }

        /// <summary>
        /// Deletes a guild channel, or closes a DM.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/> if deleting a guild channel.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordChannel> DeleteChannel(Snowflake channelId)
        {
            return DeleteChannel<DiscordChannel>(channelId);
        }

        /// <summary>
        /// Deletes a guild channel, or closes a DM.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/> if deleting a guild channel.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordChannel> DeleteChannel(DiscordChannel channel)
        {
            return DeleteChannel<DiscordChannel>(channel.Id);
        }

        /// <summary>
        /// Deletes a guild channel, or closes a DM.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/> if deleting a guild channel.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<T> DeleteChannel<T>(T channel)
            where T : DiscordChannel
        {
            return DeleteChannel<T>(channel.Id);
        }

        /// <summary>
        /// Deletes a guild channel, or closes a DM.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/> if deleting a guild channel.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<T> DeleteChannel<T>(Snowflake channelId)
            where T : DiscordChannel
        {
            DiscordApiData data = await rest.Delete($"channels/{channelId}", $"channels/{channelId}").ConfigureAwait(false);
            return (T)DeserializeChannelData(data);
        }

        /// <summary>
        /// Edits a guild channel permission overwrite for a user or role.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task EditChannelPermissions(Snowflake channelId, Snowflake overwriteId,
            DiscordPermission allow, DiscordPermission deny, DiscordOverwriteType type)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("allow", (int)allow);
            data.Set("deny", (int)deny);
            data.Set("type", type.ToString().ToLower());

            await rest.Put($"channels/{channelId}/permissions/{overwriteId}", data,
                $"channels/{channelId}/permissions/permission").ConfigureAwait(false);
        }

        /// <summary>
        /// Edits a guild channel permission overwrite for a user or role.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task EditChannelPermissions(DiscordOverwrite overwrite,
            DiscordPermission allow, DiscordPermission deny, DiscordOverwriteType type)
        {
            return EditChannelPermissions(overwrite.ChannelId.Id, overwrite.Id, allow, deny, type);
        }

        /// <summary>
        /// Deletes a guild channel permission overwrite for a user or role.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteChannelPermission(Snowflake channelId, Snowflake overwriteId)
        {
            await rest.Delete($"channels/{channelId}/permissions/{overwriteId}",
                $"channels/{channelId}/permissions/permission").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a guild channel permission overwrite for a user or role.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task DeleteChannelPermission(DiscordOverwrite overwrite)
        {
            return DeleteChannelPermission(overwrite.ChannelId.Id, overwrite.Id);
        }

        /// <summary>
        /// Causes the current bot to appear as typing in this channel.
        /// <para>Note: it is recommended that bots do not generally use this route.
        /// This should only be used if the bot is responding to a command that is expected
        /// to take a few seconds or longer.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task TriggerTypingIndicator(Snowflake channelId)
        {
            await rest.Post($"channels/{channelId}/typing",
                $"channels/{channelId}/typing").ConfigureAwait(false);
        }

        /// <summary>
        /// Causes the current bot to appear as typing in this channel.
        /// <para>Note: it is recommended that bots do not generally use this route.
        /// This should only be used if the bot is responding to a command that is expected
        /// to take a few seconds or longer.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task TriggerTypingIndicator(ITextChannel channel)
        {
            return TriggerTypingIndicator(channel.Id);
        }

        /// <summary>
        /// Gets a list of all channels in a guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordGuildChannel>> GetGuildChannels(Snowflake guildId)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}/channels",
                $"guilds/{guildId}/channels").ConfigureAwait(false);

            DiscordGuildChannel[] channels = new DiscordGuildChannel[data.Values.Count];
            for (int i = 0; i < channels.Length; i++)
                channels[i] = (DiscordGuildChannel)DeserializeChannelData(data.Values[i]);

            return channels;
        }

        /// <summary>
        /// Gets a list of all channels in a guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordGuildChannel>> GetGuildChannels(DiscordGuild guild)
        {
            return GetGuildChannels(guild.Id);
        }

        /// <summary>
        /// Creates a new text channel, voice channel, or channel category for a guild.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildChannel> CreateGuildChannel(Snowflake guildId, CreateGuildChannelOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            DiscordApiData requestData = options.Build();

            DiscordApiData returnData = await rest.Post($"guilds/{guildId}/channels", requestData,
                $"guilds/{guildId}/channels").ConfigureAwait(false);
            return (DiscordGuildChannel)DeserializeChannelData(returnData);
        }

        /// <summary>
        /// Creates a new text channel, voice channel, or channel category for a guild.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildChannel> CreateGuildChannel(DiscordGuild guild, CreateGuildChannelOptions options)
        {
            return CreateGuildChannel(guild.Id, options);
        }

        /// <summary>
        /// Changes the positions of channels in the specified guild. The list of
        /// positions does not need to include every channel, it just needs the 
        /// channels that are being moved.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task ModifyGuildChannelPositions(Snowflake guildId,
            IEnumerable<PositionOptions> positions)
        {
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));

            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Array);
            foreach (PositionOptions positionParam in positions)
                requestData.Values.Add(positionParam.Build());

            await rest.Patch($"guilds/{guildId}/channels", requestData,
                $"guilds/{guildId}/channels").ConfigureAwait(false);
        }

        /// <summary>
        /// Changes the positions of channels in the specified guild. The list of
        /// positions does not need to include every channel, it just needs the 
        /// channels that are being moved.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task ModifyGuildChannelPositions(DiscordGuild guild,
            IEnumerable<PositionOptions> positions)
        {
            return ModifyGuildChannelPositions(guild.Id, positions);
        }
    }
}
