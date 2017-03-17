using Discore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discore
{
    public sealed class DiscordWebhook : DiscordIdObject
    {
        /// <summary> 
        /// Gets the ID of the guild this webhook belongs to.
        /// </summary> 
        public Snowflake GuildId { get; }
        /// <summary> 
        /// Gets the ID of the channel this webhook is active for.
        /// </summary> 
        public Snowflake ChannelId { get; }
        /// <summary> 
        /// Gets the wser that created this webhook.
        /// </summary> 
        public DiscordUser User { get; }
        /// <summary> 
        /// Gets the public name of this webhook.
        /// </summary> 
        public string Name { get; }
        /// <summary> 
        /// Gets the avatar of this webhook.
        /// </summary> 
        public DiscordAvatarData Avatar { get; }
        /// <summary> 
        /// Gets the token of this webhook. 
        /// <para>This is only populated if the current authenticated user created the webhook, otherwise it's empty/null.</para> 
        /// <para>It's used for Executing, Updating, and Deleting this webhook without the need of authorization.</para> 
        /// </summary> 
        public string Token { get; }
        /// <summary>
        /// Gets whether this webhook instance contains the webhook token.
        /// </summary>
        public bool HasToken => !string.IsNullOrWhiteSpace(Token);

        DiscordHttpWebhookEndpoint webhookHttp;

        internal DiscordWebhook(IDiscordApplication app, DiscordApiData data)
            : base(data)
        {
            webhookHttp = app.HttpApi.Webhooks;

            GuildId = data.GetSnowflake("guild_id").Value;
            ChannelId = data.GetSnowflake("channel_id").Value;

            DiscordApiData userData = data.Get("user");
            if (!userData.IsNull)
                User = new DiscordUser(userData);

            Name = data.GetString("name");
            Avatar = new DiscordAvatarData(data.GetString("avatar"));
            Token = data.GetString("token");
        }

        /// <summary>
        /// Modifies the settings of this webhook.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordWebhook> Modify(string name = null, DiscordAvatarData avatar = null)
        {
            return webhookHttp.Modify(Id, name, avatar);
        }

        /// <summary>
        /// Deletes this webhook permanently.
        /// Current authenticated user might be the owner.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> Delete()
        {
            return webhookHttp.Delete(Id);
        }

        /// <summary>
        /// Deletes this webhook permanently.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> DeleteWithToken(string token)
        {
            return webhookHttp.DeleteWithToken(Id, token);
        }

        /// <summary>
        /// Executes this webhook with a message as the content.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> Execute(string token, string content, 
            string username = null, string avatarUrl = null, bool tts = false)
        {
            return webhookHttp.Execute(Id, token, content, username, avatarUrl, tts);
        }

        /// <summary>
        /// Executes this webhook with a file as the content.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> Execute(string token, byte[] file, 
            string filename = "unknown.jpg", string username = null, string avatarUrl = null, bool tts = false)
        {
            return webhookHttp.Execute(Id, token, file, filename, username, avatarUrl, tts);
        }

        /// <summary>
        /// Executes this webhook with a file as the content.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> Execute(string token, FileInfo fileInfo, 
            string username = null, string avatarUrl = null, bool tts = false)
        {
            return webhookHttp.Execute(Id, token, fileInfo, username, avatarUrl, tts);
        }

        /// <summary>
        /// Executes this webhook with embeds as the contents.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> Execute(string token, IEnumerable<DiscordEmbedBuilder> embedBuilders, 
            string username = null, string avatarUrl = null, bool tts = false)
        {
            return webhookHttp.Execute(Id, token, embedBuilders, username, avatarUrl, tts);
        }
    }
}
