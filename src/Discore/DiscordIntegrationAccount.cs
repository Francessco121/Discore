#nullable enable

using System.Text.Json;

namespace Discore
{
    /// <summary>
    /// The account of an integration.
    /// </summary>
    public sealed class DiscordIntegrationAccount
    {
        /// <summary>
        /// Gets the ID of this account.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the name of this account.
        /// </summary>
        public string Name { get; }

        public DiscordIntegrationAccount(string id, string name)
        {
            Id = id;
            Name = name;
        }

        internal DiscordIntegrationAccount(JsonElement json)
        {
            Id = json.GetProperty("id").GetString()!;
            Name = json.GetProperty("name").GetString()!;
        }

        /// <summary>
        /// Returns the name of this integration account.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }
}

#nullable restore
