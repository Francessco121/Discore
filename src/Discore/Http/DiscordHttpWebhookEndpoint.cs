using Discore.Http.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discore.Http
{
    public sealed class DiscordHttpWebhookEndpoint
    {
        HttpWebhookEndpoint endpoint;

        internal DiscordHttpWebhookEndpoint(HttpWebhookEndpoint endpoint)
        {
            this.endpoint = endpoint;
        }

        /// <summary>
        /// Create a <see cref="DiscordWebhook"/> on a <see cref="DiscordChannel"/>
        /// </summary>
        /// <param name="name">Webhook bot's username that shows in chat</param>
        /// <param name="avatar">Webhook bot's avatar</param>
        /// <param name="channel">The channel the webhook lives on</param>
        /// <returns><see cref="DiscordWebhook"/></returns>
        public async Task<DiscordWebhook> CreateWebhook(string name, DiscordAvatarData avatar, DiscordChannel channel)
            => await CreateWebhook(name, avatar, channel.Id);

        /// <summary>
        /// Create a <see cref="DiscordWebhook"/> using a <see cref="Snowflake"/> of a <see cref="DiscordChannel"/>
        /// </summary>
        /// <param name="name">Webhook bot's username that shows in chat</param>
        /// <param name="avatar">Webhook bot's avatar</param>
        /// <param name="Id">The channel the webhook lives on</param>
        /// <returns><see cref="DiscordWebhook"/></returns>
        public async Task<DiscordWebhook> CreateWebhook(string name, DiscordAvatarData avatar, Snowflake Id)
            => new DiscordWebhook(await endpoint.CreateWebhook(name, avatar, Id));

        /// <summary>
        /// Get a <see cref="DiscordWebhook"/> via its <see cref="Snowflake"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns><see cref="DiscordWebhook"/></returns>
        public async Task<DiscordWebhook> GetWebhook(Snowflake id)
            => new DiscordWebhook(await endpoint.GetWebhook(id));

        /// <summary>
        /// Get a List of <see cref="DiscordWebhook"/> on a <see cref="DiscordChannel"/>
        /// </summary>
        /// <param name="channel"><see cref="DiscordChannel"/> to poll</param>
        /// <returns><see cref="DiscordChannel"/>(s)</returns>
        public async Task<IReadOnlyList<DiscordWebhook>> GetWebhooks(DiscordChannel channel)
            => await GetWebhooks(channel.Id);

        /// <summary>
        /// Get a List of <see cref="DiscordWebhook"/> on a <see cref="Snowflake"/>
        /// </summary>
        /// <param name="id"><see cref="Snowflake"/> of a <see cref="DiscordChannel"/> to poll</param>
        /// <returns><see cref="IReadOnlyList{DiscordChannel}"/></returns>
        public async Task<IReadOnlyList<DiscordWebhook>> GetWebhooks(Snowflake id)
        {
            List<DiscordWebhook> toReturn = new List<DiscordWebhook>();
            DiscordApiData json = await endpoint.GetChannelWebhooks(id);
            if (json.Type != DiscordApiDataType.Array)
                throw new DiscoreException("Malformed json response");

            foreach (var item in json.Values)
                toReturn.Add(new DiscordWebhook(item));

            return toReturn;
        }

        /// <summary>
        /// Modify an exsting <see cref="DiscordWebhook"/>
        /// </summary>
        /// <param name="webhook"><see cref="DiscordWebhook"/> to modify</param>
        /// <param name="name">Only updateed if not null</param>
        /// <param name="avatar">Only updated if not null</param>
        /// <returns><see cref="DiscordWebhook"/></returns>
        public async Task<DiscordWebhook> ModifyWebhook(DiscordWebhook webhook, string name = null, DiscordAvatarData avatar = null)
            => await ModifyWebhook(webhook.Id, name, avatar);

        /// <summary>
        /// Modify an exsting <see cref="DiscordWebhook"/> via its <see cref="Snowflake"/>
        /// </summary>
        /// <param name="Id"><see cref="Snowflake"/> of the <see cref="DiscordWebhook"/> to modify</param>
        /// <param name="name">Only updateed if not null</param>
        /// <param name="avatar">Only updated if not null</param>
        /// <returns><see cref="DiscordWebhook"/></returns>
        public async Task<DiscordWebhook> ModifyWebhook(Snowflake Id, string name = null, DiscordAvatarData avatar = null)
            => new DiscordWebhook(await endpoint.ModifyWebhook(Id, name, avatar));

        /// <summary>
        /// Delete an existing <see cref="DiscordWebhook"/>
        /// </summary>
        /// <param name="webhook"><see cref="DiscordWebhook"/> to delete</param>
        public async Task DeleteWebhook(DiscordWebhook webhook)
        {
            if (webhook.HasToken) // if we have a token
                await endpoint.DeleteWebhook(webhook.Id, webhook.Token); 
            else
                await endpoint.DeleteWebhook(webhook.Id);
        }

        /// <summary>
        /// Delete an existing <see cref="DiscordWebhook"/> via its <see cref="Snowflake"/>
        /// </summary>
        /// <param name="Id"><see cref="Snowflake"/> of a <see cref="DiscordWebhook"/> to delete</param>
        public async Task DeleteWebhook(Snowflake Id)
        {
            DiscordWebhook wh = await GetWebhook(Id);
            await DeleteWebhook(wh);
        }

        /// <summary>
        /// Execute a <see cref="DiscordWebhook"/> with a message
        /// </summary>
        /// <param name="webhook"><see cref="DiscordWebhook"/> to execute</param>
        /// <param name="content">Message to post to the <see cref="DiscordChannel"/></param>
        /// <param name="username">Temporally override <see cref="DiscordWebhook.Name"/></param>
        /// <param name="avatar"><see cref="Uri"/> to an image, temporally override <see cref="DiscordWebhook.Avatar"/></param>
        /// <param name="tts">Text to Speech</param>
        public async Task ExecuteWebhook(DiscordWebhook webhook,
            string content,
            string username = null,
            Uri avatar = null,
            bool tts = false)
            => await endpoint.ExecuteWebhook(webhook.Id, webhook.Token, content, username, avatar, tts);

        /// <summary>
        /// Execute a <see cref="DiscordWebhook"/> with a file
        /// </summary>
        /// <param name="webhook"><see cref="DiscordWebhook"/> to execute</param>
        /// <param name="file">File to post to the <see cref="DiscordChannel"/></param>
        /// <param name="filename">Filename of the file, defaults to unknown.jpg</param>
        /// <param name="username">Temporally override <see cref="DiscordWebhook.Name"/></param>
        /// <param name="avatar"><see cref="Uri"/> to an image, temporally override <see cref="DiscordWebhook.Avatar"/></param>
        /// <param name="tts">Text to Speech</param>
        public async Task ExecuteWebhook(DiscordWebhook webhook,
            byte[] file,
            string filename = "unknown.jpg",
            string username = null,
            Uri avatar = null,
            bool tts = false)
            => await endpoint.ExecuteWebhook(webhook.Id, webhook.Token, file, filename, username, avatar, tts);

        /// <summary>
        /// Execute a <see cref="DiscordWebhook"/> with a file
        /// </summary>
        /// <param name="webhook"><see cref="DiscordWebhook"/> to execute</param>
        /// <param name="file">File to post to the <see cref="DiscordChannel"/></param>
        /// <param name="username">Temporally override <see cref="DiscordWebhook.Name"/></param>
        /// <param name="avatar"><see cref="Uri"/> to an image, temporally override <see cref="DiscordWebhook.Avatar"/></param>
        /// <param name="tts">Text to Speech</param>
        public async Task ExecuteWebhook(DiscordWebhook webhook,
            FileInfo file,
            string username = null,
            Uri avatar = null,
            bool tts = false)
        {

            using (FileStream fs = file.OpenRead())
            using (MemoryStream ms = new MemoryStream())
            {
                await fs.CopyToAsync(ms);
                await endpoint.ExecuteWebhook(webhook.Id, webhook.Token, ms.ToArray(), file.Name, username, avatar, tts);
            }

        }

        /// <summary>
        /// Execute a <see cref="DiscordWebhook"/> with <see cref="DiscordEmbed"/>
        /// </summary>
        /// <param name="webhook"><see cref="DiscordWebhook"/> to execute</param>
        /// <param name="embeds"><see cref="DiscordEmbed"/>(s) to post</param>
        /// <param name="username">Temporally override <see cref="DiscordWebhook.Name"/></param>
        /// <param name="avatar"><see cref="Uri"/> to an image, temporally override <see cref="DiscordWebhook.Avatar"/></param>
        /// <param name="tts">Text to Speech</param>
        public async Task ExecuteWebhook(DiscordWebhook webhook,
            IEnumerable<DiscordEmbed> embeds,
            string username = null,
            Uri avatar = null,
            bool tts = false)
        {
            DiscordApiData data = DiscordApiData.ArrayType;
            foreach (DiscordEmbed embed in embeds)
                data.Values.Add(embed.Serialize());

            await endpoint.ExecuteWebhook(webhook.Id, webhook.Token, data, username, avatar, tts);
        }
    }
}
