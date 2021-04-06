using Discore.Http.Internal;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Discore.Http
{
    public sealed partial class DiscordHttpClient : IDisposable
    {
        /// <summary> 
        /// Gets or sets whether a single HTTP client should be used for all API requests per 
        /// <see cref="DiscordHttpClient"/> instance. 
        /// <para>In rare cases using a single client causes requests to hang until they timeout 
        /// (believed to be a .NET Core bug).</para>
        /// <para>This is true by default.</para> 
        /// <para>Note: This only applies to newly created <see cref="DiscordHttpClient"/> instances.</para> 
        /// </summary> 
        public static bool UseSingleHttpClient { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to resend requests that get rate-limited.
        /// </summary>
        public bool RetryWhenRateLimited
        {
            get => rest.RetryOnRateLimit;
            set => rest.RetryOnRateLimit = value;
        }

        readonly RestClient rest;

        public DiscordHttpClient(string botToken)
        {
            rest = new RestClient(botToken);
        }

        string BuildJsonContent(Action<Utf8JsonWriter> builder)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            builder(writer);
            writer.Flush();

            return Encoding.UTF8.GetString(stream.GetBuffer().AsSpan(0, (int)stream.Length));
        }

        DiscordChannel DeserializeChannelData(JsonElement data)
        {
            DiscordChannelType type = (DiscordChannelType)data.GetProperty("type").GetInt32();

            if (type == DiscordChannelType.DirectMessage)
                return new DiscordDMChannel(data);
            else if (type == DiscordChannelType.GuildText)
                return new DiscordGuildTextChannel(data);
            else if (type == DiscordChannelType.GuildVoice)
                return new DiscordGuildVoiceChannel(data);
            else if (type == DiscordChannelType.GuildCategory)
                return new DiscordGuildCategoryChannel(data);
            else if (type == DiscordChannelType.GuildNews)
                return new DiscordGuildNewsChannel(data);
            else if (type == DiscordChannelType.GuildStore)
                return new DiscordGuildStoreChannel(data);
            else
                throw new NotSupportedException($"{type} isn't a known type of {nameof(DiscordChannel)}.");
        }

        public void Dispose()
        {
            rest.Dispose();
        }
    }
}
