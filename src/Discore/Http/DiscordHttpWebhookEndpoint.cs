using System;
using Discore.Http.Net;
using System.Collections.Generic;
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

        public async Task<DiscordWebhook> CreateWebhook(string name, DiscordAvatarData avatar, WebSocket.DiscordChannel channel) 
            => new DiscordWebhook(await endpoint.CreateWebhook(name, avatar, channel));

        public async Task<DiscordWebhook> GetWebhook(DiscordWebhook webhook)
            => new DiscordWebhook(await endpoint.GetWebhook(webhook));

        public async Task<IReadOnlyList<DiscordWebhook>> GetWebhooks(WebSocket.DiscordChannel channel)
        {
            List<DiscordWebhook> toReturn = new List<DiscordWebhook>();
            DiscordApiData json = await endpoint.GetWebhooks(channel);
            if (json.Type != DiscordApiDataType.Array)
                throw new DiscoreException("Malformed json response");

            foreach (var item in json.Values)
                toReturn.Add(new DiscordWebhook(item));

            return toReturn;
        }

        public async Task<DiscordWebhook> ModifyWebhook(DiscordWebhook webhook, string name = null, DiscordAvatarData avatar = null)
            => new DiscordWebhook(await endpoint.ModifyWebhook(webhook, name, avatar));

        public async Task DeleteWebhook(DiscordWebhook webhook)
            => await endpoint.DeleteWebhook(webhook);

        public async Task ExecuteWebhook(DiscordWebhook webhook,
            string content,
            string username = null,
            Uri avatar = null,
            bool tts = false)
            => await endpoint.ExecuteWebhook(webhook, content, username, avatar, tts);

        public async Task ExecuteWebhook(DiscordWebhook webhook,
            byte[] file,
            string filename = "unknown.jpg",
            string username = null,
            Uri avatar = null,
            bool tts = false)
            => await endpoint.ExecuteWebhook(webhook, file, filename, username, avatar, tts);

        public async Task ExecuteWebhook(DiscordWebhook webhook,
            DiscordEmbed[] embeds,
            string username = null,
            Uri avatar = null,
            bool tts = false)
            => await endpoint.ExecuteWebhook(webhook, embeds, username, avatar, tts);
        }
}
