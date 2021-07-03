using System.Text.Json;

namespace Discore.Http
{
    /// <summary>
    /// A set of options used to modify the properties of a guild widget.
    /// </summary>
    public class ModifyGuildWidgetOptions
    {
        /// <summary>
        /// Gets or sets whether the widget is enabled.
        /// <para>Set to null to leave the enabled state unmodified.</para>
        /// </summary>
        public bool? Enabled { get; set; }
        /// <summary>
        /// Gets or sets the ID of the guild channel this widget is for.
        /// <para>Set to null to leave the channel ID unmodified.</para>
        /// </summary>
        public Snowflake? ChannelId { get; set; }

        /// <summary>
        /// Sets whether the widget is enabled.
        /// </summary>
        public ModifyGuildWidgetOptions SetEnabled(bool enabled)
        {
            Enabled = enabled;
            return this;
        }

        /// <summary>
        /// Sets the ID of the guild channel this widget is for.
        /// </summary>
        public ModifyGuildWidgetOptions SetChannel(Snowflake channelId)
        {
            ChannelId = channelId;
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            if (Enabled != null)
                writer.WriteBoolean("enabled", Enabled.Value);

            if (ChannelId != null)
                writer.WriteSnowflake("channel_id", ChannelId.Value);

            writer.WriteEndObject();
        }
    }
}
