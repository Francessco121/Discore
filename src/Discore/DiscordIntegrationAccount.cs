namespace Discore
{
    /// <summary>
    /// The account of a <see cref="DiscordIntegration"/>.
    /// </summary>
    public class DiscordIntegrationAccount : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of this account.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets the name of this account.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Updates this integration account with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this integration account with.</param>
        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            Name = data.GetString("name") ?? Name;
        }
    }
}
