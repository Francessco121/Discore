using System.Text.Json;

namespace Discore.Http
{
    /// <summary>
    /// A set of options used to modify the properties of a guild embed.
    /// </summary>
    public class ModifyGuildEmbedOptions
    {
        /// <summary>
        /// Gets or sets whether the embed is enabled.
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Gets or sets the ID of the guild channel this embed is for.
        /// </summary>
        public Snowflake ChannelId { get; set; }

        /// <summary>
        /// Sets whether the embed is enabled.
        /// </summary>
        public ModifyGuildEmbedOptions SetEnabled(bool enabled)
        {
            Enabled = enabled;
            return this;
        }

        /// <summary>
        /// Sets the ID of the guild channel this embed is for.
        /// </summary>
        public ModifyGuildEmbedOptions SetChannel(Snowflake channelId)
        {
            ChannelId = channelId;
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteBoolean("enabled", Enabled);
            writer.WriteSnowflake("channel_id", ChannelId);

            writer.WriteEndObject();
        }
    }
}
