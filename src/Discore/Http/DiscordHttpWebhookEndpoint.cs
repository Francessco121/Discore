using System;
using Discore.Http.Net;
using System.Collections.Generic;

namespace Discore.Http
{
    public sealed class DiscordHttpWebhookEndpoint
    {
        HttpWebhookEndpoint endpoint;

        internal DiscordHttpWebhookEndpoint(HttpWebhookEndpoint endpoint)
        {
            this.endpoint = endpoint;
        }

        public DiscordWebhook CreateWebhook(string name, DiscordAvatarData avatar, WebSocket.DiscordChannel channel) 
            => new DiscordWebhook(endpoint.CreateWebhook(name, avatar, channel));

        public DiscordWebhook GetWebhook(DiscordWebhook webhook)
            => new DiscordWebhook(endpoint.GetWebhook(webhook));

        public IReadOnlyList<DiscordWebhook> GetWebhooks(WebSocket.DiscordChannel channel)
        {
            List<DiscordWebhook> toReturn = new List<DiscordWebhook>();
            DiscordApiData json = endpoint.GetWebhooks(channel);
            if (json.Type != DiscordApiDataType.Array)
                throw new DiscoreException("Malformed json response");

            foreach (var item in json.Values)
                toReturn.Add(new DiscordWebhook(item));

            return toReturn;
        }

        public DiscordWebhook ModifyWebhook(DiscordWebhook webhook, string name = null, DiscordAvatarData avatar = null)
            => new DiscordWebhook(endpoint.ModifyWebhook(webhook, name, avatar));

        public void DeleteWebhook(DiscordWebhook webhook)
            => endpoint.DeleteWebhook(webhook);

        public void ExecuteWebhook(DiscordWebhook webhook,
            string content,
            string username = null,
            Uri avatar = null,
            bool tts = false)
            => endpoint.ExecuteWebhook(webhook, content, username, avatar, tts);

        public void ExecuteWebhook(DiscordWebhook webhook,
            byte[] file,
            string filename = "unknown.jpg",
            string username = null,
            Uri avatar = null,
            bool tts = false)
            => endpoint.ExecuteWebhook(webhook, file, filename, username, avatar, tts);

        public void ExecuteWebhook(DiscordWebhook webhook,
            DiscordEmbed[] embeds,
            string username = null,
            Uri avatar = null,
            bool tts = false)
            => endpoint.ExecuteWebhook(webhook, embeds, username, avatar, tts);
        }
}
