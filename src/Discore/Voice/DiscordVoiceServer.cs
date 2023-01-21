using System.Text.Json;

namespace Discore.Voice
{
    /// <summary>
    /// A voice server the application is connecting/connected to.
    /// </summary>
    public class DiscordVoiceServer
    {
        /// <summary>
        /// The ID of the guild that the voice server is for.
        /// </summary>
        public Snowflake? GuildId { get; }
        /// <summary>
        /// Token allocated for the application that is used to authenticate.
        /// </summary>
        public string Token { get; }
        /// <summary>
        /// The voice server host endpoint. 
        /// May be null if the previous voice server is down and a new one hasn't been allocated yet.
        /// </summary>
        public string? Endpoint { get; }

        public DiscordVoiceServer(Snowflake? guildId, string token, string? endpoint)
        {
            GuildId = guildId;
            Token = token;
            Endpoint = endpoint;
        }

        internal DiscordVoiceServer(JsonElement json)
        {
            GuildId = json.GetPropertyOrNull("guild_id")?.GetSnowflake();
            Token = json.GetProperty("token").GetString()!;
            Endpoint = json.GetProperty("endpoint").GetString();
        }
    }
}
