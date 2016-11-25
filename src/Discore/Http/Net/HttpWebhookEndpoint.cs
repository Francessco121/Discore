using System;
using System.Text;
using System.Net.Http;

namespace Discore.Http.Net
{
    class HttpWebhookEndpoint : HttpApiEndpoint
    {
        public HttpWebhookEndpoint(RestClient restClient) 
            : base(restClient)
        {}

        /// <summary>
        /// Create a webhook on a <see cref="DiscordChannel"/>, only supports <see cref="DiscordChannelType.Guild"/> channels
        /// </summary>
        /// <param name="name">Name of the webhook, 2-200 characters</param>
        /// <param name="avatar"><see cref="DiscordAvatarData"/> of the webhook user</param>
        /// <param name="channel"><see cref="WebSocket.DiscordChannel"/> to create the webhook on</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="DiscoreException"/>
        /// <exception cref="InvalidOperationException"/>
        public DiscordApiData CreateWebhook(string name, DiscordAvatarData avatar, WebSocket.DiscordChannel channel)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(avatar));
            if (avatar == null) throw new ArgumentNullException(nameof(avatar));
            if (channel == null) throw new ArgumentNullException(nameof(avatar));

            //webhooks only support guild channels
            if (channel.ChannelType != DiscordChannelType.Guild)
                throw new DiscoreException($"{nameof(channel)} must be a type of {DiscordChannelType.Guild}");

            if (name.Length < 2 || name.Length > 200)
                throw new DiscoreException($"{nameof(name)} must be atleast 2 characters and no more than 200 characters");

            DiscordApiData container = DiscordApiData.CreateContainer();
            container.Set("name", name);
            container.Set("avatar", avatar.ToFormattedString());

            return Rest.Post($"channels/{channel.Id}/webhooks", container, "CreateWebhook");
        }

        /// <summary>
        /// Get all webhooks on a <see cref="DiscordChannel"/>  
        /// </summary>
        /// <param name="channel"><see cref="WebSocket.DiscordChannel"/> to poll</param>
        /// <exception cref="ArgumentException"/>
        public DiscordApiData GetWebhooks(WebSocket.DiscordChannel channel)
        {
            if (channel == null) throw new ArgumentException(nameof(channel));

            return Rest.Get($"channels/{channel.Id}/webhooks", "GetChannelWebhooks");
        }

        /// <summary>
        /// Get all webhooks on a <see cref="WebSocket.DiscordGuild"/>
        /// </summary>
        /// <param name="guild"><see cref="WebSocket.DiscordGuild"/> to poll</param>
        /// <exception cref="ArgumentNullException"/>
        public DiscordApiData GetWebhooks(WebSocket.DiscordGuild guild)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return Rest.Get($"guilds/{guild.Id}/webhooks", "GetGuildWebhooks");
        }

        //https://discordapp.com/developers/docs/resources/webhook#get-webhook
        /// <summary>
        /// Get a existing <see cref="DiscordWebhook"/>
        /// </summary>
        /// <param name="webhook"/>
        public DiscordApiData GetWebhook(DiscordWebhook webhook) {
            return Rest.Get($"webhooks/{webhook.Id}", "GetWebhook");
        }
        //public DiscordApiData GetWebhook(DiscordWebhook, bool usingToken) //TODO: is this call needed?

        /// <summary>
        /// Modify a existing <see cref="DiscordWebhook"/>
        /// </summary>
        /// <param name="webhook"><see cref="DiscordWebhook"/> to modfiy</param>
        /// <param name="name">New <see cref="DiscordWebhook.Name"/>, optional</param>
        /// <param name="avatar">new <see cref="DiscordAvatarData"/>, optional</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public DiscordApiData ModifyWebhook(DiscordWebhook webhook, string name = null, DiscordAvatarData avatar = null)
        {
            if (webhook == null) throw new ArgumentNullException(nameof(webhook));

            DiscordApiData postData = DiscordApiData.CreateContainer();
            if (!string.IsNullOrWhiteSpace(name)) postData.Set("name", name);
            if (avatar != null) postData.Set("avatar", avatar.ToFormattedString());

            return Rest.Patch($"webhooks/{webhook.Id}", postData, "ModifyWebhook");
        }
        //public DiscordApiData ModifyWebhook(DiscordWebhook, bool usingToken)

        /// <summary>
        /// Delete a existing <see cref="DiscordWebhook"/> 
        /// </summary>
        /// <param name="webhook"><see cref="DiscordWebhook"/> to delete</param>
        public DiscordApiData DeleteWebhook(DiscordWebhook webhook)
        {
            return Rest.Delete($"webhooks/{webhook.Id}", "DeleteWebhook");
        }
        //public DiscordApiData DeleteWebhook(DiscordWebhook, bool usingToken)


        public DiscordApiData ExecuteWebhook(DiscordWebhook webhook,
            string content,
            string username = null, 
            Uri avatar = null,
            bool tts = false)
        {
            if (string.IsNullOrWhiteSpace(content) || content.Length < 1 || content.Length > 2000)
                throw new DiscoreException($"{nameof(content)} must be atleast 1 character and no more than 2000 characters");

            DiscordApiData postData = DiscordApiData.CreateContainer();

            if (!string.IsNullOrWhiteSpace(username)) postData.Set("username", username);
            if (avatar != null) postData.Set("avatar", avatar.ToString());
            postData.Set("tts", tts);
            postData.Set("content", content);

            return Rest.Post($"webhooks/{webhook.Id}/{webhook.Token}", postData, "ExecuteWebhook");
        }

        public DiscordApiData ExecuteWebhook(DiscordWebhook webhook,
            byte[] file,
            string filename = null,
            string username = null,
            Uri avatar = null,
            bool tts = false)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentNullException($"{nameof(file)} can't be null or empty");
            
            DiscordApiData postData = DiscordApiData.CreateContainer();

            if (!string.IsNullOrWhiteSpace(username)) postData.Set("username", username);
            if (avatar != null) postData.Set("avatar", avatar.ToString());
            postData.Set("tts", tts);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{RestClient.BASE_URL}/webhooks/{webhook.Id}/{webhook.Token}");

            MultipartFormDataContent form = new MultipartFormDataContent();

            ByteArrayContent content = new ByteArrayContent(file);

            form.Add(content, "file", filename);
            form.Add(new StringContent(postData.SerializeToJson(), Encoding.UTF8, "application/json")); //no fucking idea if this works

            request.Content = form;

            return Rest.Send(request, "ExecuteWebhook");
        }

        public DiscordApiData ExecuteWebhook(DiscordWebhook webhook,
            DiscordEmbed[] embeds,
            string username = null,
            Uri avatar = null,
            bool tts = false)
        {
            if (embeds == null || embeds.Length < 1 || embeds.Length > 2000)
                throw new DiscoreException($"{nameof(embeds)} must have atleast 1 entry");

            DiscordApiData postData = DiscordApiData.CreateContainer();

            if (!string.IsNullOrWhiteSpace(username)) postData.Set("username", username);
            if (avatar != null) postData.Set("avatar", avatar.ToString());
            postData.Set("tts", tts);
            postData.Set("embeds", embeds);

            return Rest.Post($"webhooks/{webhook.Id}/{webhook.Token}", postData, "ExecuteWebhook");
        }
    }
}
