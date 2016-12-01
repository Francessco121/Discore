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

        public async Task<DiscordWebhook> CreateWebhook(string name, DiscordAvatarData avatar, DiscordChannel channel)
            => new DiscordWebhook(await endpoint.CreateWebhook(name, avatar, channel.Id));

        public async Task<DiscordWebhook> GetWebhook(Snowflake id)
            => new DiscordWebhook(await endpoint.GetWebhook(id));

        public async Task<IReadOnlyList<DiscordWebhook>> GetWebhooks(DiscordChannel channel)
        {
            List<DiscordWebhook> toReturn = new List<DiscordWebhook>();
            DiscordApiData json = await endpoint.GetChannelWebhooks(channel.Id);
            if (json.Type != DiscordApiDataType.Array)
                throw new DiscoreException("Malformed json response");

            foreach (var item in json.Values)
                toReturn.Add(new DiscordWebhook(item));

            return toReturn;
        }

        public async Task<DiscordWebhook> ModifyWebhook(DiscordWebhook webhook, string name = null, DiscordAvatarData avatar = null)
            => new DiscordWebhook(await endpoint.ModifyWebhook(webhook.Id, name, avatar));

        public async Task DeleteWebhook(DiscordWebhook webhook, bool useToken)
        {
            if (useToken)
                await endpoint.DeleteWebhook(webhook.Id, webhook.Token);
            else
                await endpoint.DeleteWebhook(webhook.Id);
        }

        public async Task ExecuteWebhook(DiscordWebhook webhook,
            string content,
            string username = null,
            Uri avatar = null,
            bool tts = false)
            => await endpoint.ExecuteWebhook(webhook.Id, webhook.Token, content, username, avatar, tts);

        public async Task ExecuteWebhook(DiscordWebhook webhook,
            byte[] file,
            string filename = "unknown.jpg",
            string username = null,
            Uri avatar = null,
            bool tts = false)
            => await endpoint.ExecuteWebhook(webhook.Id, webhook.Token, file, filename, username, avatar, tts);

        //Need to setup a way to serialize DiscordEmbed to a json blob
        public async Task ExecuteWebhook(DiscordWebhook webhook,
            DiscordEmbed[] embeds,
            string username = null,
            Uri avatar = null,
            bool tts = false)
        {
            throw new NotImplementedException();

            //return await endpoint.ExecuteWebhook(webhook.Id, webhook.Token, embeds, username, avatar, tts);
        }
    }
}
