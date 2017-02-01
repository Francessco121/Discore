using Discore.Http.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Discore.Http
{
    public sealed class DiscordHttpWebhookEndpoint : DiscordHttpApiEndpoint
    {
        internal DiscordHttpWebhookEndpoint(IDiscordApplication app, RestClient rest) 
            : base(app, rest)
        { }

        /// <summary>
        /// Creates a webhook.
        /// </summary>
        /// <param name="channelId">The id of the channel the webhook will post to.</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordWebhook> Create(string name, DiscordAvatarData avatar, Snowflake channelId)
        {
            DiscordApiData apiData = DiscordApiData.CreateContainer();
            apiData.Set("name", name);
            apiData.Set("avatar", avatar);

            DiscordApiData returnData = await Rest.Post($"channels/{channelId}/webhooks", apiData, "CreateWebhook").ConfigureAwait(false);

            return new DiscordWebhook(App, returnData);
        }

        /// <summary>
        /// Gets a webhook via its ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordWebhook> Get(Snowflake webhookId)
        {
            DiscordApiData apiData = await Rest.Get($"webhooks/{webhookId}", "GetWebhook").ConfigureAwait(false);

            return new DiscordWebhook(App, apiData);
        }

        /// <summary>
        /// Gets a webhook via its ID.
        /// <para>This call does not require authentication and returns no user in the webhook object.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordWebhook> GetWithToken(Snowflake webhookId, string token)
        {
            DiscordApiData apiData = await Rest.Get($"webhooks/{webhookId}/{token}", "GetWebhook").ConfigureAwait(false);

            return new DiscordWebhook(App, apiData);
        }

        /// <summary>
        /// Gets a list of webhooks active for the specified guild text channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooks(Snowflake channelId)
        {
            DiscordApiData apiData = await Rest.Get($"channels/{channelId}/webhooks", "GetChannelWebhooks").ConfigureAwait(false);

            DiscordWebhook[] webhooks = new DiscordWebhook[apiData.Values.Count];

            for (int i = 0; i < apiData.Values.Count; i++)
                webhooks[i] = new DiscordWebhook(App, apiData.Values[i]);

            return webhooks;
        }

        /// <summary>
        /// Gets a list of all webhooks in a guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooks(Snowflake guildId)
        {
            DiscordApiData apiData = await Rest.Get($"guilds/{guildId}/webhooks", "GetGuildWebhooks").ConfigureAwait(false);

            DiscordWebhook[] webhooks = new DiscordWebhook[apiData.Values.Count];

            for (int i = 0; i < apiData.Values.Count; i++)
                webhooks[i] = new DiscordWebhook(App, apiData.Values[i]);

            return webhooks;
        }

        /// <summary>
        /// Modifies an exsting webhook.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordWebhook> Modify(Snowflake webhookId, string name = null, DiscordAvatarData avatar = null)
        {
            DiscordApiData postData = DiscordApiData.CreateContainer();
            if (!string.IsNullOrWhiteSpace(name)) postData.Set("name", name);
            if (avatar != null) postData.Set("avatar", avatar);

            DiscordApiData apiData = await Rest.Patch($"webhooks/{webhookId}", postData, "ModifyWebhook").ConfigureAwait(false);

            return new DiscordWebhook(App, apiData);
        }

        /// <summary>
        /// Deletes a webhook permanently. The currently authenticated user must be the owner.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> Delete(Snowflake webhookId)
        {
            return (await Rest.Delete($"webhooks/{webhookId}", "DeleteWebhook").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Deletes a webhook permanently.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> DeleteWithToken(Snowflake webhookId, string token)
        {
            return (await Rest.Delete($"webhooks/{webhookId}/{token}", "DeleteWebhook").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Executes a webhook with a message as the content.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> Execute(Snowflake webhookId, string token,
            string content, string username = null,
            string avatarUrl = null, bool tts = false)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException(nameof(content));

            DiscordApiData postData = DiscordApiData.CreateContainer();

            postData.Set("username", username);
            postData.Set("avatar_url", avatarUrl);
            postData.Set("tts", tts);
            postData.Set("content", content);

            return (await Rest.Post($"webhooks/{webhookId}/{token}", postData, "ExecuteWebhook").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Executes a webhook with a file as the content.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> Execute(Snowflake webhookId, string token,
            byte[] file, string filename = "unknown.jpg",
            string username = null, string avatarUrl = null, bool tts = false)
        {
            DiscordApiData postData = DiscordApiData.CreateContainer();

            postData.Set("username", username);
            postData.Set("avatar_url", avatarUrl);
            postData.Set("tts", tts);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{RestClient.BASE_URL}/webhooks/{webhookId}/{token}");

            MultipartFormDataContent form = new MultipartFormDataContent();

            ByteArrayContent content = new ByteArrayContent(file);

            form.Add(content, "file", filename);
            form.Add(new StringContent(postData.SerializeToJson(), Encoding.UTF8, "application/json"));

            request.Content = form;

            return (await Rest.Send(request, "ExecuteWebhook").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Executes a webhook with a file as the content.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> Execute(Snowflake webhookId, string token,
            FileInfo file, string username = null,
            string avatarUrl = null, bool tts = false)
        {
            using (FileStream fs = file.OpenRead())
            using (MemoryStream ms = new MemoryStream())
            {
                await fs.CopyToAsync(ms).ConfigureAwait(false);
                return await Execute(webhookId, token, ms.ToArray(), file.Name, username, avatarUrl, tts).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes a webhook with a embed as contents.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> Execute(Snowflake webhookId, string token,
            DiscordEmbedBuilder embed, string username = null,
            string avatarUrl = null, bool tts = false)
        {
            DiscordEmbedBuilder[] builders = new DiscordEmbedBuilder[] { embed };

            return await Execute(webhookId, token, builders, username, avatarUrl, tts).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a webhook with embeds as the contents.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> Execute(Snowflake webhookId, string token,
            IEnumerable<DiscordEmbedBuilder> embedBuilders, string username = null,
            string avatarUrl = null, bool tts = false)
        {
            DiscordApiData data = DiscordApiData.CreateArray();
            foreach (DiscordEmbedBuilder embedBuilder in embedBuilders)
                data.Values.Add(embedBuilder.Build());

            DiscordApiData postData = DiscordApiData.CreateContainer();

            postData.Set("username", username);
            postData.Set("avatar_url", avatarUrl);
            postData.Set("tts", tts);

            DiscordApiData embedData = new DiscordApiData(DiscordApiDataType.Array);
            foreach (DiscordEmbedBuilder builder in embedBuilders)
                embedData.Values.Add(builder.Build());

            postData.Set("embeds", embedData);

            return (await Rest.Post($"webhooks/{webhookId}/{token}", postData, "ExecuteWebhook").ConfigureAwait(false)).IsNull;
        }
    }
}
