using System.Text.Json;

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

        internal DiscordActivityTimestamps(JsonElement json)
        {
            Start = json.GetPropertyOrNull("start")?.GetInt64();
            End = json.GetPropertyOrNull("end")?.GetInt64();
        }
    }
}
