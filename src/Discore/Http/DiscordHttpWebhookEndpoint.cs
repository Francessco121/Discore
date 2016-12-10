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
        /// Create a <see cref="DiscordWebhook"/> on a <see cref="ITextChannel"/>.
        /// </summary>
        /// <param name="name">Webhook bot's username that shows in chat.</param>
        /// <param name="avatar">Webhook bot's avatar.</param>
        /// <param name="channel">The channel the webhook will post to.</param>
        /// <returns><see cref="DiscordWebhook"/></returns>
        public async Task<DiscordWebhook> CreateWebhook(string name, DiscordAvatarData avatar, ITextChannel channel)
        {
            return await CreateWebhook(name, avatar, channel.Id);
        }

        /// <summary>
        /// Create a <see cref="DiscordWebhook"/> on a DiscordChannel.
        /// </summary>
        /// <param name="name">Webhook bot's username that shows in chat.</param>
        /// <param name="avatar">Webhook bot's avatar.</param>
        /// <param name="channelId">The channel the webhook will post to.</param>
        /// <returns><see cref="DiscordWebhook"/></returns>
        public async Task<DiscordWebhook> CreateWebhook(string name, DiscordAvatarData avatar, Snowflake channelId)
        {
            DiscordApiData apiData = DiscordApiData.CreateContainer();
            apiData.Set("name", name);
            apiData.Set("avatar", avatar);

            DiscordApiData returnData = await Rest.Post($"channels/{channelId}/webhooks", apiData, "CreateWebhook");

            return new DiscordWebhook(returnData);
        }

        /// <summary>
        /// Get a <see cref="DiscordWebhook"/> via its <see cref="Snowflake"/>.
        /// </summary>
        /// <returns><see cref="DiscordWebhook"/></returns>
        public async Task<DiscordWebhook> GetWebhook(Snowflake id)
        {
            DiscordApiData apiData = await Rest.Get($"webhooks/{id}", "GetWebhook");

            return new DiscordWebhook(apiData);
        }

        /// <summary>
        /// Same as <see cref="GetWebhook(Snowflake)"/>, except this call does not require authentication and returns no user in the webhook object.
        /// </summary>
        /// <returns><see cref="DiscordWebhook"/></returns>
        public async Task<DiscordWebhook> GetWebhookWithToken(Snowflake id, string token)
        {
            DiscordApiData apiData = await Rest.Get($"webhooks/{id}/{token}", "GetWebhook");

            return new DiscordWebhook(apiData);
        }

        /// <summary>
        /// Get a List of <see cref="DiscordWebhook"/> on a Discord Channel.
        /// </summary>
        /// <param name="channel">Discord Channel to poll.</param>
        /// <returns><see cref="DiscordChannel"/>(s)</returns>
        public async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooks(ITextChannel channel)
        {
            return await GetChannelWebhooks(channel.Id);
        }

        /// <summary>
        /// Get a List of <see cref="DiscordWebhook"/> on a Discord Channel.
        /// </summary>
        /// <param name="id">Discord Channel to poll.</param>
        /// <returns><see cref="DiscordChannel"/>(s)</returns>
        public async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooks(Snowflake id)
        {
            DiscordApiData apiData = await Rest.Get($"channels/{id}/webhooks", "GetChannelWebhooks");

            DiscordWebhook[] webhooks = new DiscordWebhook[apiData.Values.Count];

            for (int i = 0; i < apiData.Values.Count; i++)
                webhooks[i] = new DiscordWebhook(apiData.Values[i]);

            return webhooks;
        }

        /// <summary>
        /// Returns a list of guild webhook objects.
        /// </summary>
        public async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooks(DiscordGuildTextChannel channel)
        {
            return await GetGuildWebhooks(channel.Id);
        }

        /// <summary>
        /// Returns a list of guild webhook objects.
        /// </summary>
        public async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooks(Snowflake id)
        {
            DiscordApiData apiData = await Rest.Get($"guilds/{id}/webhooks", "GetGuildWebhooks");

            DiscordWebhook[] webhooks = new DiscordWebhook[apiData.Values.Count];

            for (int i = 0; i < apiData.Values.Count; i++)
                webhooks[i] = new DiscordWebhook(apiData.Values[i]);

            return webhooks;
        }

        /// <summary>
        /// Modify a <see cref="DiscordWebhook"/>. Returns the updated webhook object on success. All parameters to this endpoint are optional.
        /// </summary>
        /// <param name="webhook"><see cref="DiscordWebhook"/> to modify.</param>
        /// <param name="name">Only updateed if not null.</param>
        /// <param name="avatar">Only updated if not null.</param>
        /// <returns><see cref="DiscordWebhook"/></returns>
        public async Task<DiscordWebhook> ModifyWebhook(DiscordWebhook webhook, string name = null, DiscordAvatarData avatar = null)
        {
            return await ModifyWebhook(webhook.Id, name, avatar);
        }

        /// <summary>
        /// Modify an exsting <see cref="DiscordWebhook"/> via its <see cref="Snowflake"/> Returns the updated webhook object on success. All parameters to this endpoint are optional.
        /// </summary>
        /// <param name="id"><see cref="Snowflake"/> of the <see cref="DiscordWebhook"/> to modify.</param>
        /// <param name="name">Only updateed if not null.</param>
        /// <param name="avatar">Only updated if not null.</param>
        /// <returns><see cref="DiscordWebhook"/></returns>
        public async Task<DiscordWebhook> ModifyWebhook(Snowflake id, string name = null, DiscordAvatarData avatar = null)
        {
            DiscordApiData postData = DiscordApiData.CreateContainer();
            if (!string.IsNullOrWhiteSpace(name)) postData.Set("name", name);
            if (avatar != null) postData.Set("avatar", avatar);

            DiscordApiData apiData = await Rest.Patch($"webhooks/{id}", postData, "ModifyWebhook");

            return new DiscordWebhook(apiData);
        }

        /// <summary>
        /// Delete a webhook permanently. User must be owner.
        /// </summary>
        public async Task<bool> DeleteWebhook(DiscordWebhook webhook)
        {
            return await DeleteWebhook(webhook.Id);
        }

        /// <summary>
        /// Delete a webhook permanently. User must be owner.
        /// </summary>
        public async Task<bool> DeleteWebhook(Snowflake id)
        {
            return (await Rest.Delete($"webhooks/{id}", "DeleteWebhook")).IsNull;
        }

        /// <summary>
        /// Same as <see cref="DeleteWebhook(DiscordWebhook)"/>, except this call does not require authentication.
        /// </summary>
        public async Task<bool> DeleteWebhookWithToken(DiscordWebhook webhook)
        {
            return await DeleteWebhookWithToken(webhook.Id, webhook.Token);
        }

        /// <summary>
        /// Same as <see cref="DeleteWebhook(Snowflake)"/>, except this call does not require authentication.
        /// </summary>
        public async Task<bool> DeleteWebhookWithToken(Snowflake id, string token)
        {
            return (await Rest.Delete($"webhooks/{id}/{token}", "DeleteWebhook")).IsNull;
        }

        /// <summary>
        /// Execute a <see cref="DiscordWebhook"/> with a message
        /// </summary>
        public async Task<bool> ExecuteWebhook(DiscordWebhook webhook,
            string content, string username = null,
            Uri avatar = null, bool tts = false)
        {
            return await ExecuteWebhook(webhook.Id, webhook.Token, content, username, avatar, tts);
        }

        /// <summary>
        /// Execute a <see cref="DiscordWebhook"/> with a message
        /// </summary>
        public async Task<bool> ExecuteWebhook(Snowflake id, string token,
            string content, string username = null,
            Uri avatar = null, bool tts = false)
        {
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentNullException(nameof(content));

            DiscordApiData postData = DiscordApiData.CreateContainer();

            if (!string.IsNullOrWhiteSpace(username)) postData.Set("username", username);
            if (avatar != null) postData.Set("avatar", avatar.ToString());
            postData.Set("tts", tts);
            postData.Set("content", content);

            return (await Rest.Post($"webhooks/{id}/{token}", postData, "ExecuteWebhook")).IsNull;
        }

        /// <summary>
        /// Execute a <see cref="DiscordWebhook"/> with a file
        /// </summary>
        public async Task<bool> ExecuteWebhook(Snowflake id, string token,
            byte[] file, string filename = "unknown.jpg",
            string username = null, Uri avatar = null, bool tts = false)
        {
            DiscordApiData postData = DiscordApiData.CreateContainer();

            if (!string.IsNullOrWhiteSpace(username)) postData.Set("username", username);
            if (avatar != null) postData.Set("avatar", avatar.ToString());
            postData.Set("tts", tts);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{RestClient.BASE_URL}/webhooks/{id.Id}/{token}");

            MultipartFormDataContent form = new MultipartFormDataContent();

            ByteArrayContent content = new ByteArrayContent(file);

            form.Add(content, "file", filename);
            form.Add(new StringContent(postData.SerializeToJson(), Encoding.UTF8, "application/json"));

            request.Content = form;

            return (await Rest.Send(request, "ExecuteWebhook")).IsNull;
        }

        /// <summary>
        /// Execute a <see cref="DiscordWebhook"/> with a file
        /// </summary>
        public async Task<bool> ExecuteWebhook(DiscordWebhook webhook,
            FileInfo file, string username = null,
            Uri avatar = null, bool tts = false)
        {
            return await ExecuteWebhook(webhook.Id, webhook.Token, file, username, avatar, tts);
        }

        /// <summary>
        /// Execute a <see cref="DiscordWebhook"/> with a file
        /// </summary>
        public async Task<bool> ExecuteWebhook(Snowflake id, string token,
            FileInfo file, string username = null,
            Uri avatar = null, bool tts = false)
        {
            using (FileStream fs = file.OpenRead())
            using (MemoryStream ms = new MemoryStream())
            {
                await fs.CopyToAsync(ms);
                return await ExecuteWebhook(id, token, ms.ToArray(), file.Name, username, avatar, tts);
            }
        }

        /// <summary>
        /// Execute a <see cref="DiscordWebhook"/> with <see cref="DiscordEmbed"/>
        /// </summary>
        public async Task<bool> ExecuteWebhook(DiscordWebhook webhook,
            IEnumerable<DiscordEmbed> embeds, string username = null,
            Uri avatar = null, bool tts = false)
        {
            return await ExecuteWebhook(webhook.Id, webhook.Token, embeds, username, avatar, tts);
        }

        /// <summary>
        /// Execute a <see cref="DiscordWebhook"/> with <see cref="DiscordEmbed"/>
        /// </summary>
        public async Task<bool> ExecuteWebhook(Snowflake id, string token,
            IEnumerable<DiscordEmbed> embeds, string username = null,
            Uri avatar = null, bool tts = false)
        {
            DiscordApiData data = DiscordApiData.CreateArray();
            foreach (DiscordEmbed embed in embeds)
                data.Values.Add(embed.Serialize());

            DiscordApiData postData = DiscordApiData.CreateContainer();

            if (!string.IsNullOrWhiteSpace(username)) postData.Set("username", username);
            if (avatar != null) postData.Set("avatar", avatar.ToString());
            postData.Set("tts", tts);
            postData.Set("embeds", embeds);

            return (await Rest.Post($"webhooks/{id}/{token}", postData, "ExecuteWebhook")).IsNull;
        }
    }
}
