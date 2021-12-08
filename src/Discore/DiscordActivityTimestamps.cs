namespace Discore
{
    public class DiscordActivityTimestamps
    {
        /// <summary>
        /// Gets the unix time (in milliseconds) of when the activity started.
        /// </summary>
        public long? Start { get; }

        /// <summary>
        /// Gets the unix time (in milliseconds) of when the activity ends.
        /// </summary>
        public long? End { get; }

        internal DiscordActivityTimestamps(DiscordApiData data)
        {
            Start = data.GetInt64("start");
            End = data.GetInt64("end");
        }
    }
}
