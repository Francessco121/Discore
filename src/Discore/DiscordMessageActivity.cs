namespace Discore
{
    public class DiscordMessageActivity
    {
        /// <summary>
        /// Gets the type of activity.
        /// </summary>
        public DiscordMessageActivityType Type { get; }

        /// <summary>
        /// Gets the party ID from a Rich Presence event.
        /// May be null.
        /// </summary>
        public string PartyId { get; }

        internal DiscordMessageActivity(DiscordApiData data)
        {
            Type = (DiscordMessageActivityType)(data.GetInteger("type") ?? 0);
            PartyId = data.GetString("party_id");
        }
    }
}
