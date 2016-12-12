using Discore.Http;
using System.Threading.Tasks;

namespace Discore
{
    public sealed class DiscordGuildEmbed
    {
        /// <summary>
        /// Gets whether this embed is enabled.
        /// </summary>
        public bool Enabled { get; }
        /// <summary>
        /// Gets the embed channel id.
        /// </summary>
        public Snowflake ChannelId { get; }
        /// <summary>
        /// Gets the id of the guild this embed is for.
        /// </summary>
        public Snowflake GuildId { get; }

        DiscordHttpGuildEndpoint guildsHttp;

        internal DiscordGuildEmbed(IDiscordApplication app, Snowflake guildId, DiscordApiData data)
        {
            guildsHttp = app.HttpApi.Guilds;

            GuildId = guildId;

            Enabled = data.GetBoolean("enabled").Value;
            ChannelId = data.GetSnowflake("channel_id").Value;
        }

        /// <summary>
        /// Modifies the properties of this guild embed.
        /// </summary>
        public async Task<DiscordGuildEmbed> Modify(ModifyGuildEmbedParameters parameters)
        {
            return await guildsHttp.ModifyEmbed(GuildId, parameters);
        }
    }
}
