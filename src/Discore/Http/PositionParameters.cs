namespace Discore.Http
{
    /// <summary>
    /// A set of parameters defining the position of an item such as a channel or role.
    /// </summary>
    public class PositionParameters
    {
        /// <summary>
        /// The ID of the item to change the position of.
        /// </summary>
        public Snowflake Id { get; set; }
        /// <summary>
        /// The sorting position of the item.
        /// </summary>
        public int Position { get; set; }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("id", Id);
            data.Set("position", Position);

            return data;
        }
    }
}
