using Discore.Http.Internal;
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
            DiscordApiData data = await rest.Get("users/@me", "users/@me").ConfigureAwait(false);
            return new DiscordUser(false, data);
        }

        /// <summary>
        /// Gets a user by their ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordUser> GetUser(Snowflake id)
        {
            DiscordApiData data = await rest.Get($"users/{id}", "users/user").ConfigureAwait(false);
            return new DiscordUser(false, data);
        }

        /// <summary>
        /// Modifies the current bot's user object.
        /// Parameters left null will leave the properties unchanged.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordUser> ModifyCurrentUser(string username = null, DiscordImageData avatar = null)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            if (username != null)
                requestData.Set("username", username);
            if (avatar != null)
                requestData.Set("avatar", avatar.ToDataUriScheme());

            DiscordApiData returnData = await rest.Patch("users/@me", requestData, "users/@me").ConfigureAwait(false);
            return returnData.IsNull ? null : new DiscordUser(false, returnData);
        }

        /// <summary>
        /// Gets a list of user guilds the current bot is in.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordUserGuild[]> GetCurrentUserGuilds(int? limit = null,
            Snowflake? baseGuildId = null, GuildGetStrategy getStrategy = GuildGetStrategy.After)
        {
            UrlParametersBuilder paramBuilder = new UrlParametersBuilder();

            if (baseGuildId.HasValue)
                paramBuilder.Add(getStrategy.ToString().ToLower(), baseGuildId.ToString());

            if (limit.HasValue)
                paramBuilder.Add("limit", limit.Value.ToString());

            DiscordApiData data = await rest.Get($"users/@me/guilds{paramBuilder.ToQueryString()}", 
                "users/@me/guilds").ConfigureAwait(false);
            DiscordUserGuild[] guilds = new DiscordUserGuild[data.Values.Count];

            for (int i = 0; i < guilds.Length; i++)
                guilds[i] = new DiscordUserGuild(data.Values[i]);

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
        /// Gets a list of currently opened DM channels for the current bot.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordDMChannel[]> GetUserDMs()
        {
            DiscordApiData data = await rest.Get("users/@me/channels", "users/@me/channels").ConfigureAwait(false);
            DiscordDMChannel[] dms = new DiscordDMChannel[data.Values.Count];

            for (int i = 0; i < dms.Length; i++)
                dms[i] = new DiscordDMChannel(this, data.Values[i]);

            return dms;
        }

        /// <summary>
        /// Opens a DM channel with the specified user.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordDMChannel> CreateDM(Snowflake recipientId)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.SetSnowflake("recipient_id", recipientId);

            DiscordApiData returnData = await rest.Post("users/@me/channels", requestData,
                "users/@me/channels").ConfigureAwait(false);
            return new DiscordDMChannel(this, returnData);
        }
    }
}
