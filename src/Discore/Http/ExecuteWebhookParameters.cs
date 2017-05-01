using System.Collections.Generic;

namespace Discore.Http
{
    public class ExecuteWebhookParameters
    {
        /// <summary>
        /// Gets or sets the username to override the webhook's normal username with (or null to not override).
        /// </summary>
        public string UsernameOverride { get; set; }

        /// <summary>
        /// Gets or sets the URL of the avatar to override the webhook's normal avatar with (or null to not override).
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Gets or sets whether the created message should use text-to-speech (or null to default to false).
        /// </summary>
        public bool? TextToSpeech { get; set; }

        /// <summary>
        /// Gets or sets the text content to include in the message (or null to only use embeds and/or attachments).
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets embeds to include in the created message (or null to only use text content and/or attachments).
        /// </summary>
        public IEnumerable<DiscordEmbedBuilder> Embeds { get; set; }

        /// <summary>
        /// Sets the username to override the webhook's normal username with.
        /// </summary>
        public ExecuteWebhookParameters SetUsernameOverride(string username)
        {
            UsernameOverride = username;
            return this;
        }

        /// <summary>
        /// Sets the URL of the avatar to override the webhook's normal avatar with.
        /// </summary>
        public ExecuteWebhookParameters SetAvatarOverride(string avatarUrl)
        {
            AvatarUrl = avatarUrl;
            return this;
        }

        /// <summary>
        /// Sets whether the created message should use text-to-speech.
        /// </summary>
        public ExecuteWebhookParameters SetTextToSpeech(bool useTextToSpeech)
        {
            TextToSpeech = useTextToSpeech;
            return this;
        }

        /// <summary>
        /// Sets the text content to include in the message.
        /// </summary>
        public ExecuteWebhookParameters SetContent(string textContent)
        {
            Content = textContent;
            return this;
        }

        /// <summary>
        /// Sets the embed to be included in the created message.
        /// </summary>
        public ExecuteWebhookParameters SetEmbed(DiscordEmbedBuilder embed)
        {
            Embeds = new DiscordEmbedBuilder[] { embed };
            return this;
        }

        /// <summary>
        /// Sets the embeds to be included in the created message.
        /// </summary>
        public ExecuteWebhookParameters SetEmbeds(IEnumerable<DiscordEmbedBuilder> embeds)
        {
            Embeds = embeds;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);

            if (UsernameOverride != null)
                data.Set("username", UsernameOverride);
            if (AvatarUrl != null)
                data.Set("avatar_url", AvatarUrl);
            if (TextToSpeech.HasValue)
                data.Set("tts", TextToSpeech.Value);
            if (Content != null)
                data.Set("content", Content);

            if (Embeds != null)
            {
                DiscordApiData embedArray = new DiscordApiData(DiscordApiDataType.Array);
                foreach (DiscordEmbedBuilder builder in Embeds)
                    embedArray.Values.Add(builder.Build());

                data.Set("embeds", embedArray);
            }

            return data;
        }
    }
}
