using System.Text.Json;

namespace Discore.Http
{
    /// <summary>
    /// A set of parameters defining the position of an item such as a channel or role.
    /// </summary>
    public class PositionOptions
    {
        /// <summary>
        /// Gets or sets the ID of the item to change the position of (e.g. a channel or role ID).
        /// </summary>
        public Snowflake Id { get; set; }
        /// <summary>
        /// Gets or sets the sorting position of the item. Note: Positions start at 1 not 0.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Sets the ID of the item to change the position of (e.g. a channel or role ID).
        /// </summary>
        public PositionOptions SetId(Snowflake id)
        {
            Id = id;
            return this;
        }

        /// <summary>
        /// Sets the sorting position of the item. Note: Positions start at 1 not 0.
        /// </summary>
        public PositionOptions SetPosition(int position)
        {
            Position = position;
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteSnowflake("id", Id);
            writer.WriteNumber("position", Position);

            writer.WriteEndObject();
        }
    }
}
