using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace Discore.Http.Net
{
    class HttpWebhookEndpoint : HttpApiEndpoint
    {
        public HttpWebhookEndpoint(RestClient restClient)
            : base(restClient)
        { }

        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="DiscoreException"/>
        /// <exception cref="InvalidOperationException"/>
        public async Task<DiscordApiData> CreateWebhook(string name, DiscordAvatarData avatar, Snowflake id)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (avatar == null) throw new ArgumentNullException(nameof(avatar));
            if (id == null) throw new ArgumentNullException(nameof(id));

            if (name.Length < 2 || name.Length > 200)
                throw new DiscoreException($"{nameof(name)} must be atleast 2 characters and no more than 200 characters");

            DiscordApiData container = DiscordApiData.CreateContainer();
            container.Set("name", name);
            container.Set("avatar", avatar.ToFormattedString());

            return await Rest.Post($"channels/{id}/webhooks", container, "CreateWebhook");
        }

        /// <exception cref="ArgumentException"/>
        public async Task<DiscordApiData> GetChannelWebhooks(Snowflake id)
        {
            if (id == null) throw new ArgumentException(nameof(id));

            return await Rest.Get($"channels/{id}/webhooks", "GetChannelWebhooks");
        }

        /// <exception cref="ArgumentNullException"/>
        public async Task<DiscordApiData> GetGuildWebhooks(Snowflake id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            return await Rest.Get($"guilds/{id}/webhooks", "GetGuildWebhooks");
        }

        public async Task<DiscordApiData> GetWebhook(Snowflake id)
        {
            return await Rest.Get($"webhooks/{id}", "GetWebhook");
        }

        public async Task<DiscordApiData> GetWebhook(Snowflake id, string token)
        {
            return await Rest.Get($"webhooks/{id}/{token}", "GetWebhookToken");
        }

        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        public async Task<DiscordApiData> ModifyWebhook(Snowflake id, string name = null, DiscordAvatarData avatar = null)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            DiscordApiData postData = DiscordApiData.CreateContainer();
            if (!string.IsNullOrWhiteSpace(name)) postData.Set("name", name);
            if (avatar != null) postData.Set("avatar", avatar.ToFormattedString());

            return await Rest.Patch($"webhooks/{id}", postData, "ModifyWebhook");
        }

        public async Task<DiscordApiData> ModifyWebhook(Snowflake id, string token, string name = null, DiscordAvatarData avatar = null)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentNullException(nameof(token));

            DiscordApiData postData = DiscordApiData.CreateContainer();
            if (!string.IsNullOrWhiteSpace(name)) postData.Set("name", name);
            if (avatar != null) postData.Set("avatar", avatar.ToFormattedString());

            return await Rest.Patch($"webhooks/{id}/{token}", postData, "ModifyWebhookToken");
        }

        public async Task<DiscordApiData> DeleteWebhook(Snowflake id)
        {
            return await Rest.Delete($"webhooks/{id}", "DeleteWebhook");
        }

        public async Task<DiscordApiData> DeleteWebhook(Snowflake id, string token)
        {
            return await Rest.Delete($"webhooks/{id}/{token}", "DeleteWebhookToken");
        }

        public async Task<DiscordApiData> ExecuteWebhook(Snowflake id,
            string token,
            string content,
            string username = null,
            Uri avatar = null,
            bool tts = false)
        {
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentNullException(nameof(content));

            DiscordApiData postData = DiscordApiData.CreateContainer();

            if (!string.IsNullOrWhiteSpace(username)) postData.Set("username", username);
            if (avatar != null) postData.Set("avatar", avatar.ToString());
            postData.Set("tts", tts);
            postData.Set("content", content);

            return await Rest.Post($"webhooks/{id.Id}/{token}", postData, "ExecuteWebhook");
        }

        public async Task<DiscordApiData> ExecuteWebhook(Snowflake id,
            string token,
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

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{RestClient.BASE_URL}/webhooks/{id.Id}/{token}");

            MultipartFormDataContent form = new MultipartFormDataContent();

            ByteArrayContent content = new ByteArrayContent(file);

            form.Add(content, "file", filename);
            form.Add(new StringContent(postData.SerializeToJson(), Encoding.UTF8, "application/json"));

            request.Content = form;

            return await Rest.Send(request, "ExecuteWebhook");
        }

        public async Task<DiscordApiData> ExecuteWebhook(Snowflake id,
            string token,
            DiscordApiData embeds,
            string username = null,
            Uri avatar = null,
            bool tts = false)
        {
            DiscordApiData postData = DiscordApiData.CreateContainer();

            if (!string.IsNullOrWhiteSpace(username)) postData.Set("username", username);
            if (avatar != null) postData.Set("avatar", avatar.ToString());
            postData.Set("tts", tts);
            postData.Set("embeds", embeds);

            return await Rest.Post($"webhooks/{id.Id}/{token}", postData, "ExecuteWebhook");
        }
    }
}
