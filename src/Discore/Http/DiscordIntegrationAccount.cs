namespace Discore.Http
{
    /// <summary>
    /// The account of an integration.
    /// </summary>
    public class DiscordIntegrationAccount : DiscordIdObject
    {
        /// <summary>
        /// Gets the name of this account.
        /// </summary>
        public string Name { get; }

        public DiscordIntegrationAccount(DiscordApiData data)
            : base(data)
        {
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
