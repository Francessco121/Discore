using Discore.Http.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpClient
    {
        /// <summary>
        /// Creates a webhook.
        /// <para>Requires <see cref="DiscordPermission.ManageWebhooks"/>.</para>
        /// </summary>
        /// <param name="channelId">The ID of the channel the webhook will post to.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="avatar"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordWebhook> CreateWebhook(string name, DiscordImageData avatar, Snowflake channelId)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (avatar == null)
                throw new ArgumentNullException(nameof(avatar));

            string requestData = BuildJsonContent(writer =>
            {
                writer.WriteStartObject();
                writer.WriteString("name", name);
                writer.WriteString("avatar", avatar.ToDataUriScheme());
                writer.WriteEndObject();
            });

            using JsonDocument? returnData = await rest.Post($"channels/{channelId}/webhooks", jsonContent: requestData,
                $"channels/{channelId}/webhooks").ConfigureAwait(false);

            return new DiscordWebhook(returnData!.RootElement);
        }

        /// <summary>
        /// Creates a webhook.
        /// <para>Requires <see cref="DiscordPermission.ManageWebhooks"/>.</para>
        /// </summary>
        /// <param name="channel">The channel the webhook will post to.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="channel"/>, <paramref name="name"/>, or <paramref name="avatar"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordWebhook> CreateWebhook(string name, DiscordImageData avatar, ITextChannel channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            return CreateWebhook(name, avatar, channel.Id);
        }

        /// <summary>
        /// Gets a webhook via its ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordWebhook> GetWebhook(Snowflake webhookId)
        {
            using JsonDocument? data = await rest.Get($"webhooks/{webhookId}",
                $"webhooks/{webhookId}").ConfigureAwait(false);

            return new DiscordWebhook(data!.RootElement);
        }

        /// <summary>
        /// Gets a webhook via its ID.
        /// <para>This call does not require authentication and returns no user in the webhook object.</para>
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the token is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException">Thrown if token is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordWebhook> GetWebhookWithToken(Snowflake webhookId, string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty or only contain whitespace characters.", nameof(token));

            using JsonDocument? data = await rest.Get($"webhooks/{webhookId}/{token}",
                $"webhooks/{webhookId}/token").ConfigureAwait(false);

            return new DiscordWebhook(data!.RootElement);
        }

        /// <summary>
        /// Gets a list of webhooks active for the specified text channel.
        /// <para>Requires <see cref="DiscordPermission.ManageWebhooks"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooks(Snowflake channelId)
        {
            using JsonDocument? data = await rest.Get($"channels/{channelId}/webhooks",
                $"channels/{channelId}/webhooks").ConfigureAwait(false);

            JsonElement values = data!.RootElement;

            var webhooks = new DiscordWebhook[values.GetArrayLength()];

            for (int i = 0; i < webhooks.Length; i++)
                webhooks[i] = new DiscordWebhook(values[i]);

            return webhooks;
        }

        /// <summary>
        /// Gets a list of webhooks active for the specified text channel.
        /// <para>Requires <see cref="DiscordPermission.ManageWebhooks"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooks(ITextChannel channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            return GetChannelWebhooks(channel.Id);
        }

        /// <summary>
        /// Gets a list of all webhooks in a guild.
        /// <para>Requires <see cref="DiscordPermission.ManageWebhooks"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooks(Snowflake guildId)
        {
            using JsonDocument? data = await rest.Get($"guilds/{guildId}/webhooks",
                $"guilds/{guildId}/webhooks").ConfigureAwait(false);

            JsonElement values = data!.RootElement;

            var webhooks = new DiscordWebhook[values.GetArrayLength()];

            for (int i = 0; i < webhooks.Length; i++)
                webhooks[i] = new DiscordWebhook(values[i]);

            return webhooks;
        }

        /// <summary>
        /// Gets a list of all webhooks in a guild.
        /// <para>Requires <see cref="DiscordPermission.ManageWebhooks"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooks(DiscordGuild guild)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return GetGuildWebhooks(guild.Id);
        }

        /// <summary>
        /// Modifies an existing webhook.
        /// <para>Requires <see cref="DiscordPermission.ManageWebhooks"/>.</para>
        /// </summary>
        /// <param name="channelId">The ID of the text channel to move the webhook to (or null to not move).</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordWebhook> ModifyWebhook(Snowflake webhookId,
            string? name = null, DiscordImageData? avatar = null, Snowflake? channelId = null)
        {
            string requestData = BuildJsonContent(writer =>
            {
                writer.WriteStartObject();

                if (name != null)
                    writer.WriteString("name", name);
                if (avatar != null)
                    writer.WriteString("avatar", avatar.ToDataUriScheme());
                if (channelId.HasValue)
                    writer.WriteSnowflake("channel_id", channelId.Value);

                writer.WriteEndObject();
            });

            using JsonDocument? responseData = await rest.Patch($"webhooks/{webhookId}", jsonContent: requestData,
                $"webhooks/{webhookId}").ConfigureAwait(false);

            return new DiscordWebhook(responseData!.RootElement);
        }

        /// <summary>
        /// Modifies an existing webhook.
        /// <para>Requires <see cref="DiscordPermission.ManageWebhooks"/>.</para>
        /// </summary>
        /// <param name="channel">The text channel to move the webhook to (or null to not move).</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordWebhook> ModifyWebhook(DiscordWebhook webhook,
            string? name = null, DiscordImageData? avatar = null, ITextChannel? channel = null)
        {
            if (webhook == null) throw new ArgumentNullException(nameof(webhook));

            return ModifyWebhook(webhook.Id, name, avatar, channel?.Id);
        }

        /// <summary>
        /// Modifies an existing webhook.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the token is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException">Thrown if token is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task ModifyWebhookWithToken(Snowflake webhookId, string token,
            string? name = null, DiscordImageData? avatar = null)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty or only contain whitespace characters.", nameof(token));

            string requestData = BuildJsonContent(writer =>
            {
                writer.WriteStartObject();

                if (name != null)
                    writer.WriteString("name", name);
                if (avatar != null)
                    writer.WriteString("avatar", avatar.ToDataUriScheme());

                writer.WriteEndObject();
            });

            await rest.Patch($"webhooks/{webhookId}/{token}", jsonContent: requestData,
                $"webhooks/{webhookId}/token").ConfigureAwait(false);
        }

        /// <summary>
        /// Modifies an existing webhook.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the token is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="webhook"/> or <paramref name="token"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task ModifyWebhookWithToken(DiscordWebhook webhook, string token,
            string? name = null, DiscordImageData? avatar = null)
        {
            if (webhook == null) throw new ArgumentNullException(nameof(webhook));

            return ModifyWebhookWithToken(webhook.Id, token, name, avatar);
        }

        /// <summary>
        /// Deletes a webhook permanently. The current bot must be the owner.
        /// <para>Requires <see cref="DiscordPermission.ManageWebhooks"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteWebhook(Snowflake webhookId)
        {
            await rest.Delete($"webhooks/{webhookId}",
                $"webhooks/{webhookId}").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a webhook permanently. The current bot must be the owner.
        /// <para>Requires <see cref="DiscordPermission.ManageWebhooks"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task DeleteWebhook(DiscordWebhook webhook)
        {
            if (webhook == null) throw new ArgumentNullException(nameof(webhook));

            return DeleteWebhook(webhook.Id);
        }

        /// <summary>
        /// Deletes a webhook permanently.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the token is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException">Thrown if token is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteWebhookWithToken(Snowflake webhookId, string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty or only contain whitespace characters.", nameof(token));

            await rest.Delete($"webhooks/{webhookId}/{token}",
                $"webhooks/{webhookId}/token").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a webhook permanently.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the token is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException">Thrown if token is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task DeleteWebhookWithToken(DiscordWebhook webhook, string token)
        {
            if (webhook == null) throw new ArgumentNullException(nameof(webhook));

            return DeleteWebhookWithToken(webhook.Id, token);
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
        /// <exception cref="ArgumentNullException">Thrown if the token or <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordMessage?> ExecuteWebhook(Snowflake webhookId, string token, ExecuteWebhookOptions options,
            bool waitAndReturnMessage = false)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty or only contain whitespace characters.", nameof(token));

            string requestData = BuildJsonContent(options.Build);

            using JsonDocument? returnData = await rest.Post($"webhooks/{webhookId}/{token}?wait={waitAndReturnMessage}", jsonContent: requestData,
                $"webhooks/{webhookId}/token").ConfigureAwait(false);

            return waitAndReturnMessage ? new DiscordMessage(returnData!.RootElement) : null;
        }

        /// <summary>
        /// Executes a webhook.
        /// <para>Note: Returns null unless <paramref name="waitAndReturnMessage"/> is set to true.</para>
        /// </summary>
        /// <param name="webhook">The webhook to execute.</param>
        /// <param name="token">The webhook's token.</param>
        /// <param name="waitAndReturnMessage">Whether to wait for the message to be created 
        /// and have it returned from this method.</param>
        /// <exception cref="ArgumentException">Thrown if the token is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="webhook"/>, the token, or <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage?> ExecuteWebhook(DiscordWebhook webhook, string token, ExecuteWebhookOptions options,
            bool waitAndReturnMessage = false)
        {
            if (webhook == null) throw new ArgumentNullException(nameof(webhook));

            return ExecuteWebhook(webhook.Id, token, options, waitAndReturnMessage);
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
        public Task<DiscordMessage?> ExecuteWebhook(Snowflake webhookId, string token, Stream fileData, string fileName,
            ExecuteWebhookOptions? options = null, bool waitAndReturnMessage = false)
        {
            if (fileData == null)
                throw new ArgumentNullException(nameof(fileData));

            return ExecuteWebhook(webhookId, token, new StreamContent(fileData), fileName, options, waitAndReturnMessage);
        }

        /// <summary>
        /// Executes a webhook with a file attachment.
        /// <para>Note: Returns null unless <paramref name="waitAndReturnMessage"/> is set to true.</para>
        /// </summary>
        /// <param name="webhook">The webhook to execute.</param>
        /// <param name="token">The webhook's token.</param>
        /// <param name="waitAndReturnMessage">Whether to wait for the message to be created 
        /// and have it returned from this method.</param>
        /// <exception cref="ArgumentException">Thrown if the token is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="webhook"/>, the token, 
        /// or <paramref name="fileData"/> is null,
        /// or the file name is null, empty, or only contains whitespace characters.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage?> ExecuteWebhook(DiscordWebhook webhook, string token, Stream fileData, string fileName,
            ExecuteWebhookOptions? options = null, bool waitAndReturnMessage = false)
        {
            if (webhook == null) throw new ArgumentNullException(nameof(webhook));

            return ExecuteWebhook(webhook.Id, token, fileData, fileName, options, waitAndReturnMessage);
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
        public Task<DiscordMessage?> ExecuteWebhook(Snowflake webhookId, string token, ArraySegment<byte> fileData, string fileName,
            ExecuteWebhookOptions? options = null, bool waitAndReturnMessage = false)
        {
            return ExecuteWebhook(webhookId, token, new ByteArrayContent(fileData.Array, fileData.Offset, fileData.Count), fileName,
                options, waitAndReturnMessage);
        }

        /// <summary>
        /// Executes a webhook with a file attachment.
        /// <para>Note: Returns null unless <paramref name="waitAndReturnMessage"/> is set to true.</para>
        /// </summary>
        /// <param name="webhook">The webhook to execute.</param>
        /// <param name="token">The webhook's token.</param>
        /// <param name="waitAndReturnMessage">Whether to wait for the message to be created 
        /// and have it returned from this method.</param>
        /// <exception cref="ArgumentException">Thrown if the token is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="webhook"/>, the token,
        /// or the file name is null, empty, or only contains whitespace characters.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage?> ExecuteWebhook(DiscordWebhook webhook, string token, ArraySegment<byte> fileData, string fileName,
            ExecuteWebhookOptions? options = null, bool waitAndReturnMessage = false)
        {
            if (webhook == null) throw new ArgumentNullException(nameof(webhook));

            return ExecuteWebhook(webhook.Id, token, fileData, fileName, options, waitAndReturnMessage);
        }

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        async Task<DiscordMessage?> ExecuteWebhook(Snowflake webhookId, string token, HttpContent fileContent, string fileName,
            ExecuteWebhookOptions? options, bool waitAndReturnMessage)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty or only contain whitespace characters.", nameof(token));
            if (string.IsNullOrEmpty(fileName))
                // Technically already handled when adding the field to the multipart form data.
                throw new ArgumentNullException(nameof(fileName));

            using JsonDocument? returnData = await rest.Send(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post,
                    $"{RestClient.BASE_URL}/webhooks/{webhookId}/{token}");

                var data = new MultipartFormDataContent();
                data.Add(fileContent, "file", fileName);

                if (options != null)
                {
                    string payloadJson = BuildJsonContent(options.Build);
                    data.Add(new StringContent(payloadJson), "payload_json");
                }

                request.Content = data;

                return request;
            }, $"webhooks/{webhookId}/token").ConfigureAwait(false);

            return waitAndReturnMessage ? new DiscordMessage(returnData!.RootElement) : null;
        }
    }
}
