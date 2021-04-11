using Discore.Http.Internal;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpClient
    {
        /// <summary>
        /// Gets the user object of the current bot.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordUser> GetCurrentUser()
        {
            using JsonDocument? data = await rest.Get("users/@me", "users/@me").ConfigureAwait(false);
            return new DiscordUser(data!.RootElement, isWebhookUser: false);
        }

        /// <summary>
        /// Gets a user by their ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordUser> GetUser(Snowflake id)
        {
            using JsonDocument? data = await rest.Get($"users/{id}", "users/user").ConfigureAwait(false);
            return new DiscordUser(data!.RootElement, isWebhookUser: false);
        }

        /// <summary>
        /// Modifies the current bot's user object.
        /// Parameters left null will leave the properties unchanged.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordUser> ModifyCurrentUser(string? username = null, DiscordImageData? avatar = null)
        {
            string requestData = BuildJsonContent(writer =>
            {
                writer.WriteStartObject();

                if (username != null)
                    writer.WriteString("username", username);
                if (avatar != null)
                    writer.WriteString("avatar", avatar.ToDataUriScheme());

                writer.WriteEndObject();
            });

            using JsonDocument? returnData = await rest.Patch("users/@me", jsonContent: requestData, "users/@me").ConfigureAwait(false);
            return new DiscordUser(returnData!.RootElement, isWebhookUser: false);
        }

        /// <summary>
        /// Gets a list of user guilds the current bot is in.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordUserGuild[]> GetCurrentUserGuilds(int? limit = null,
            Snowflake? baseGuildId = null, GuildGetStrategy getStrategy = GuildGetStrategy.After)
        {
            var paramBuilder = new UrlParametersBuilder();

            if (baseGuildId.HasValue)
                paramBuilder.Add(getStrategy.ToString().ToLower(), baseGuildId.ToString());

            if (limit.HasValue)
                paramBuilder.Add("limit", limit.Value.ToString());

            using JsonDocument? data = await rest.Get($"users/@me/guilds{paramBuilder.ToQueryString()}", 
                "users/@me/guilds").ConfigureAwait(false);

            JsonElement values = data!.RootElement;

            var guilds = new DiscordUserGuild[values.GetArrayLength()];

            for (int i = 0; i < guilds.Length; i++)
                guilds[i] = new DiscordUserGuild(values[i]);

            return guilds;
        }

        /// <summary>
        /// Removes the current bot from the specified guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task LeaveGuild(Snowflake guildId)
        {
            await rest.Delete($"users/@me/guilds/{guildId}", "users/@me/guilds/guild").ConfigureAwait(false);
        }

        /// <summary>
        /// Removes the current bot from the specified guild.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task LeaveGuild(DiscordGuild guild)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return LeaveGuild(guild.Id);
        }

        /// <summary>
        /// Gets a list of currently opened DM channels for the current bot.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        [Obsolete("Bots are no longer able to get a list of open DM channels per a Discord API update. This method will be removed in a future release.")]
        public async Task<DiscordDMChannel[]> GetUserDMs()
        {
            using JsonDocument? data = await rest.Get("users/@me/channels", "users/@me/channels").ConfigureAwait(false);

            JsonElement values = data!.RootElement;
            
            var dms = new DiscordDMChannel[values.GetArrayLength()];

            for (int i = 0; i < dms.Length; i++)
                dms[i] = new DiscordDMChannel(values[i]);

            return dms;
        }

        /// <summary>
        /// Opens a DM channel with the specified user.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordDMChannel> CreateDM(Snowflake recipientId)
        {
            string requestData = BuildJsonContent(writer =>
            {
                writer.WriteStartObject();
                writer.WriteSnowflake("recipient_id", recipientId);
                writer.WriteEndObject();
            });

            using JsonDocument? returnData = await rest.Post("users/@me/channels", jsonContent: requestData,
                "users/@me/channels").ConfigureAwait(false);

            return new DiscordDMChannel(returnData!.RootElement);
        }

        /// <summary>
        /// Opens a DM channel with the specified user.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordDMChannel> CreateDM(DiscordUser recipient)
        {
            if (recipient == null) throw new ArgumentNullException(nameof(recipient));

            return CreateDM(recipient.Id);
        }
    }
}
