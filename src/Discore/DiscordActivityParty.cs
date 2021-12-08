using System.Collections.Generic;
using System.Linq;

namespace Discore
{
    public class DiscordActivityParty
    {
        /// <summary>
        /// Gets the ID of the party. May be null.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the party's current and maximum size ([0] and [1] respectively). May be null.
        /// </summary>
        public IReadOnlyList<int> Size { get; }

        internal DiscordActivityParty(DiscordApiData data)
        {
            Id = data.GetString("id");
            Size = data.GetArray("size")?.Select(d => d.ToInteger().Value).ToArray();
        }
    }
}
