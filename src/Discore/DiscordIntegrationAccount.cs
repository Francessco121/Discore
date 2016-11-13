namespace Discore
{
    /// <summary>
    /// The account of an integration.
    /// </summary>
    public sealed class DiscordIntegrationAccount : DiscordIdObject
    {
        /// <summary>
        /// Gets the name of this account.
        /// </summary>
        public string Name { get; private set; }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            Name = data.GetString("name") ?? Name;
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
