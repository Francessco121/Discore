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

        internal DiscordIntegrationAccount(DiscordApiData data)
        {
            Id = data.GetString("id");
            Name = data.GetString("name");
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
