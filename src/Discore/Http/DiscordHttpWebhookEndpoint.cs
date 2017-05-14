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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="avatar"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordWebhook> Create(string name, DiscordAvatarData avatar, Snowflake channelId)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (avatar == null)
                throw new ArgumentNullException(nameof(avatar));

            DiscordApiData apiData = DiscordApiData.CreateContainer();
            apiData.Set("name", name);
            apiData.Set("avatar", avatar);

            DiscordApiData returnData = await Rest.Post($"channels/{channelId}/webhooks", apiData, 
                "channels/channel/webhooks").ConfigureAwait(false);

            return new DiscordWebhook(App, returnData);
        }

        /// <summary>
        /// Gets a webhook via its ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordWebhook> Get(Snowflake webhookId)
        {
            DiscordApiData apiData = await Rest.Get($"webhooks/{webhookId}", 
                "webhooks/webhook").ConfigureAwait(false);

            return new DiscordWebhook(App, apiData);
        }

        /// <summary>
        /// Gets a webhook via its ID.
        /// <para>This call does not require authentication and returns no user in the webhook object.</para>
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the token is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordWebhook> GetWithToken(Snowflake webhookId, string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty or only contain whitespace characters.", nameof(token));

            DiscordApiData apiData = await Rest.Get($"webhooks/{webhookId}/{token}", 
                "webhooks/webhook/token").ConfigureAwait(false);

            return new DiscordWebhook(App, apiData);
        }

        /// <summary>
        /// Gets a list of webhooks active for the specified guild text channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooks(Snowflake channelId)
        {
            DiscordApiData apiData = await Rest.Get($"channels/{channelId}/webhooks", 
                "channels/channel/webhooks").ConfigureAwait(false);

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
            DiscordApiData apiData = await Rest.Get($"guilds/{guildId}/webhooks", 
                "guilds/guild/webhooks").ConfigureAwait(false);

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
            if (!string.IsNullOrWhiteSpace(name))
                postData.Set("name", name);
            if (avatar != null)
                postData.Set("avatar", avatar);

            DiscordApiData apiData = await Rest.Patch($"webhooks/{webhookId}", postData, 
                "webhooks/webhook").ConfigureAwait(false);

            return new DiscordWebhook(App, apiData);
        }

        /// <summary>
        /// Deletes a webhook permanently. The currently authenticated user must be the owner.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> Delete(Snowflake webhookId)
        {
            return (await Rest.Delete($"webhooks/{webhookId}",
                "webhooks/webhook").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Deletes a webhook permanently.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="ArgumentException">Thrown if the token is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> DeleteWithToken(Snowflake webhookId, string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty or only contain whitespace characters.", nameof(token));

            return (await Rest.Delete($"webhooks/{webhookId}/{token}",
                "webhooks/webhook/token").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Executes a webhook.
        /// <para>Note: Returns null unless <paramref name="waitAndReturnMessage"/> is set to true.</para>
        /// </summary>
        /// <param name="webhookId">The ID of the webhook to execute.</param>
        /// <param name="token">The webhook's token.</param>
        /// <param name="waitAndReturnMessage">Whether to wait for the message to be created 
        /// and have it returned from this method.</param>
        /// <exception cref="ArgumentException">Thrown if the token is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the token or <paramref name="parameters"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordMessage> Execute(Snowflake webhookId, string token, ExecuteWebhookParameters parameters,
            bool waitAndReturnMessage = false)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty or only contain whitespace characters.", nameof(token));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Post($"webhooks/{webhookId}/{token}?wait={waitAndReturnMessage}", requestData,
                "webhooks/webhook/token").ConfigureAwait(false);

            return waitAndReturnMessage ? new DiscordMessage(App, returnData) : null;
        }

        /// <summary>
        /// Executes a webhook with a file attachment.
        /// <para>Note: Returns null unless <paramref name="waitAndReturnMessage"/> is set to true.</para>
        /// </summary>
        /// <param name="webhookId">The ID of the webhook to execute.</param>
        /// <param name="token">The webhook's token.</param>
        /// <param name="waitAndReturnMessage">Whether to wait for the message to be created 
        /// and have it returned from this method.</param>
        /// <exception cref="ArgumentException">Thrown if the token is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the token is null, 
        /// or <paramref name="fileData"/> is null,
        /// or the file name is null, empty, or only contains whitespace characters.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> Execute(Snowflake webhookId, string token, Stream fileData, string fileName,
            ExecuteWebhookParameters parameters = null, bool waitAndReturnMessage = false)
        {
            if (fileData == null)
                throw new ArgumentNullException(nameof(fileData));

            return Execute(webhookId, token, new StreamContent(fileData), fileName, parameters, waitAndReturnMessage);
        }

        /// <summary>
        /// Executes a webhook with a file attachment.
        /// <para>Note: Returns null unless <paramref name="waitAndReturnMessage"/> is set to true.</para>
        /// </summary>
        /// <param name="webhookId">The ID of the webhook to execute.</param>
        /// <param name="token">The webhook's token.</param>
        /// <param name="waitAndReturnMessage">Whether to wait for the message to be created 
        /// and have it returned from this method.</param>
        /// <exception cref="ArgumentException">Thrown if the token is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the token is null 
        /// or the file name is null, empty, or only contains whitespace characters.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> Execute(Snowflake webhookId, string token, ArraySegment<byte> fileData, string fileName,
            ExecuteWebhookParameters parameters = null, bool waitAndReturnMessage = false)
        {
            return Execute(webhookId, token, new ByteArrayContent(fileData.Array, fileData.Offset, fileData.Count), fileName, 
                parameters, waitAndReturnMessage);
        }

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        async Task<DiscordMessage> Execute(Snowflake webhookId, string token, HttpContent fileContent, string fileName,
            ExecuteWebhookParameters parameters, bool waitAndReturnMessage)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty or only contain whitespace characters.", nameof(token));
            if (string.IsNullOrEmpty(fileName))
                // Technically already handled when adding the field to the multipart form data.
                throw new ArgumentNullException(nameof(fileName));

            DiscordApiData returnData = await Rest.Send(() =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                    $"{RestClient.BASE_URL}/webhooks/{webhookId}/{token}");

                MultipartFormDataContent data = new MultipartFormDataContent();
                data.Add(fileContent, "file", fileName);

                if (parameters != null)
                {
                    DiscordApiData payloadJson = parameters.Build();
                    data.Add(new StringContent(payloadJson.SerializeToJson()), "payload_json");
                }

                request.Content = data;

                return request;
            }, "webhooks/webhook/token").ConfigureAwait(false);

            return waitAndReturnMessage ? new DiscordMessage(App, returnData) : null;
        }
    }
}
