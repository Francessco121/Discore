using System.Collections.Generic;
using System.Text.Json;

namespace Discore
{
    public class DiscordActivityParty
    {
        /// <summary>
        /// Gets the ID of the party. May be null.
        /// </summary>
        public string? Id { get; }

        /// <summary>
        /// Gets the party's current and maximum size ([0] and [1] respectively). May be null.
        /// </summary>
        public IReadOnlyList<int>? Size { get; }

        internal DiscordActivityParty(JsonElement json)
        {
            Id = json.GetPropertyOrNull("id")?.GetString();

            JsonElement? sizeJson = json.GetPropertyOrNull("size");
            if (sizeJson != null)
            {
                JsonElement _sizeJson = sizeJson.Value;
                int[] size = new int[_sizeJson.GetArrayLength()];

                for (int i = 0; i < size.Length; i++)
                    size[i] = _sizeJson[i].GetInt32();

                Size = size;
            }
        }
    }
}
