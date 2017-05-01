namespace Discore.Http
{
    /// <summary>
    /// A set of parameters defining the position of an item such as a channel or role.
    /// </summary>
    public class PositionParameters
    {
        /// <summary>
        /// Gets or sets the ID of the item to change the position of (e.g. a channel or role ID).
        /// </summary>
        public Snowflake Id { get; set; }
        /// <summary>
        /// Gets or sets the sorting position of the item.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Sets the ID of the item to change the position of (e.g. a channel or role ID).
        /// </summary>
        public PositionParameters SetId(Snowflake id)
        {
            Id = id;
            return this;
        }

        /// <summary>
        /// Sets the sorting position of the item.
        /// </summary>
        public PositionParameters SetPosition(int position)
        {
            Position = position;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("id", Id);
            data.Set("position", Position);

            return data;
        }
    }
}
