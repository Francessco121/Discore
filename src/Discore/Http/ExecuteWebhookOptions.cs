using System.Collections.Generic;
using System.Text.Json;

namespace Discore.Http
{
    public class ExecuteWebhookOptions
    {
        /// <summary>
        /// Gets or sets the username to override the webhook's normal username with (or null to not override).
        /// </summary>
        public string? UsernameOverride { get; set; }

        /// <summary>
        /// Gets or sets the URL of the avatar to override the webhook's normal avatar with (or null to not override).
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Gets or sets whether the created message should use text-to-speech (or null to default to false).
        /// </summary>
        public bool? TextToSpeech { get; set; }

        /// <summary>
        /// Gets or sets the text content to include in the message (or null to only use embeds and/or attachments).
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets embeds to include in the created message (or null to only use text content and/or attachments).
        /// </summary>
        public IEnumerable<EmbedOptions>? Embeds { get; set; }

        /// <summary>
        /// Sets the username to override the webhook's normal username with.
        /// </summary>
        public ExecuteWebhookOptions SetUsernameOverride(string username)
        {
            UsernameOverride = username;
            return this;
        }

        /// <summary>
        /// Sets the URL of the avatar to override the webhook's normal avatar with.
        /// </summary>
        public ExecuteWebhookOptions SetAvatarOverride(string avatarUrl)
        {
            AvatarUrl = avatarUrl;
            return this;
        }

        /// <summary>
        /// Sets whether the created message should use text-to-speech.
        /// </summary>
        public ExecuteWebhookOptions SetTextToSpeech(bool useTextToSpeech)
        {
            TextToSpeech = useTextToSpeech;
            return this;
        }

        /// <summary>
        /// Sets the text content to include in the message.
        /// </summary>
        public ExecuteWebhookOptions SetContent(string textContent)
        {
            Content = textContent;
            return this;
        }

        /// <summary>
        /// Sets the embed to be included in the created message.
        /// </summary>
        public ExecuteWebhookOptions SetEmbed(EmbedOptions embed)
        {
            Embeds = new EmbedOptions[] { embed };
            return this;
        }

        /// <summary>
        /// Sets the embeds to be included in the created message.
        /// </summary>
        public ExecuteWebhookOptions SetEmbeds(IEnumerable<EmbedOptions> embeds)
        {
            Embeds = embeds;
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            if (UsernameOverride != null)
                writer.WriteString("username", UsernameOverride);
            if (AvatarUrl != null)
                writer.WriteString("avatar_url", AvatarUrl);
            if (TextToSpeech.HasValue)
                writer.WriteBoolean("tts", TextToSpeech.Value);
            if (Content != null)
                writer.WriteString("content", Content);

            if (Embeds != null)
            {
                writer.WriteStartArray("embeds");

                foreach (EmbedOptions builder in Embeds)
                    builder.Build(writer);

                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}
